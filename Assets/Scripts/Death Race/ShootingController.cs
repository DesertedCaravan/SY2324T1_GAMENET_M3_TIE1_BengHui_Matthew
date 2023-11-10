using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;
// using Photon.Realtime; // Unnecessary as it's mainly for handling all the rooms and lobbies.

public class ShootingController : MonoBehaviourPunCallbacks
{
    [Header("HP Related Stuff")]
    public float maxHealth = 100f;
    public float health;
    public Image healthBar;
    public Image displayedHealthBar;

    [Header("Shooting")]
    public GameObject bullet;
    public float bulletSpeed = 0f;
    public float fireRate = 0.1f;
    private float fireTimer = 0f;

    [Header("Raycasting")]
    // public Camera camera;
    public GameObject shootingPoint;
    public GameObject hitEffectPrefab;

    [Header("Status")]
    public int survivors = 0;
    public TextMeshProUGUI survivorsText; // number of vehicles left
    public TextMeshProUGUI winText;
    public GameObject deathEffectPrefab;
    
    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("dr"))
        {
            health = maxHealth; // Set current health to 100%
            healthBar = DeathRaceGameManager.instance.healthBar; // get health bar from RacingGameManager
            healthBar.fillAmount = health / maxHealth; // set health bar fill to full
            displayedHealthBar.fillAmount = health / maxHealth; // set displayed health bar to full

            // camera = transform.Find("Camera").GetComponent<Camera>(); // get camera attached to vehicle

            survivorsText = DeathRaceGameManager.instance.survivorsText; // gets kills text from RacingGameManager
            survivors = PhotonNetwork.CurrentRoom.PlayerCount;
            survivorsText.text = "Vehicles Left: " + survivors;

            winText = DeathRaceGameManager.instance.winText; // get win text from RacingGameManager
            winText.text = "";
        }
    }

    void Update()
    {
        if (fireTimer < fireRate)
        {
            fireTimer += Time.deltaTime;
        }

        if (Input.GetButton("Fire1") && fireTimer > fireRate && photonView.IsMine && this.GetComponent<VehicleMovementController>().isControlEnabled == true) // Left Mouse
        {
            fireTimer = 0.0f;
            FireBullet();
            Debug.Log("Firing Bullet");
        }
        else if (Input.GetButton("Fire2") && fireTimer > fireRate && photonView.IsMine && this.GetComponent<VehicleMovementController>().isControlEnabled == true) // Right Mouse
        {
            fireTimer = 0.0f;
            FireRay();
            Debug.Log("Firing Ray");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            photonView.RPC("CreateHitEffects", RpcTarget.All, collision.transform.position);
            photonView.RPC("TakeDamage", RpcTarget.AllBuffered, 20);

            Destroy(collision.gameObject);
        }
    }

    public void FireBullet()
    {
        bulletSpeed = 7500f + this.GetComponent<VehicleMovementController>().speed;

        GameObject b = Instantiate(bullet, shootingPoint.transform.position, shootingPoint.transform.rotation);

        b.GetComponent<Rigidbody>().AddForce(transform.forward * bulletSpeed); // ensure that the bullet speed is faster than the current speed of the vehicle

        Destroy(b, 10f);
    }

    public void FireRay()
    {
        RaycastHit hit;
        Ray ray = new Ray(shootingPoint.transform.position, shootingPoint.transform.forward);
        
        // camera.ViewportPointToRay(new Vector3(0.5f, 0.5f)); // middle point of camera

        if (Physics.Raycast(ray, out hit, 7500f)) // ray terminated up to 7500
        {
            Debug.Log(hit.collider.gameObject.name);

            photonView.RPC("CreateHitEffects", RpcTarget.All, hit.point);

            if (hit.collider.gameObject.CompareTag("Player") && !hit.collider.gameObject.GetComponent<PhotonView>().IsMine)
            {
                // Need to broadcast to all players in the room that a specific player took damage.
                // RpcTarget.All only calls players who are in the room when the event occurs, but doesn't apply to those who come in afterwards.
                // RpcTarget.AllBuffered calls players who are in the room when the event occurs, as well as to those who come in afterwards.
                hit.collider.gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllBuffered, 10);
            }
        }
    }

    // Pun Remote Procedure Calls
    [PunRPC] // Will be seen by everyone
    public void CreateHitEffects(Vector3 position)
    {
        GameObject hitEffectGameObject = Instantiate(hitEffectPrefab, position, Quaternion.identity);
        Destroy(hitEffectGameObject, 0.2f);
    }

    [PunRPC] // Will be seen by everyone
    public void CreateDeathEffects(Vector3 position)
    {
        GameObject deathEffectGameObject = Instantiate(deathEffectPrefab, position, Quaternion.identity);
        Destroy(deathEffectGameObject, 0.2f);
    }

    [PunRPC] // Will be registered by everyone
    public void TakeDamage(int damage, PhotonMessageInfo info) // Provides information on who fired the shot
    {
        // this (optional) means that damage is only applied to the owner, but everyone else will also see the damage on their ends.
        this.health -= damage;
        Debug.Log(health);

        if (PhotonNetwork.LocalPlayer.UserId != info.Sender.UserId) // Will only apply to the one being shot
        {
            this.healthBar.fillAmount = health / maxHealth;
        }

        this.displayedHealthBar.fillAmount = health / maxHealth;

        if (health <= 0)
        {
            photonView.RPC("CreateDeathEffects", RpcTarget.All, transform.position);
            Die();
            // Debug.Log(info.Sender.NickName + " has killed " + info.photonView.Owner.NickName);

            DeathRaceGameManager.instance.AddKillFeed(info.Sender.NickName, info.photonView.Owner.NickName); // The Function that calls this is an RPC, making it an RPC by extension
            // Yellow text is all that's needed as tag when calling a PunRPC function.

            if (PhotonNetwork.LocalPlayer.UserId == info.Sender.UserId) // Causes only the shooter to benefit
            {
                photonView.RPC("ReduceSurvivors", RpcTarget.AllBuffered, info.Sender.NickName); // Applies to all and the changes to a SPECIFIC gameObject (ie. another vehicle) will be seen elsewhere
                // ReduceSurvivors(); // Only applies to the local player and will not be seen elsewhere
            }
        }
    }

    public void Die()
    {
        if (photonView.IsMine)
        {
            transform.GetComponent<VehicleMovementController>().enabled = false; // get the VehicleMovementController attached to the current prefab (ie. vehicle)
            transform.GetComponent<VehicleMovementController>().isControlEnabled = false; // variable is still active
        }
    }

    [PunRPC] // Will only change the one calling it
    public void ReduceSurvivors(string potentialVictor)
    {
        survivors--;

        /*
        // CheckSurvivors(survivors);
        photonView.RPC("CheckSurvivors", RpcTarget.AllBuffered, survivors);
    }

    [PunRPC]
    public void CheckSurvivors(int vehiclesLeft)
    {
        */

        /*
        GameObject stateText = GameObject.Find("SurvivorsText (TMP)"); // Will find for all clients
        stateText.GetComponent<TextMeshProUGUI>().text = "Vehicles Left: " + survivors;
        */
        
        survivorsText.text = "Vehicles Left: " + survivors;

        if (survivors <= 1 && health <= 0) // >
        {
            survivors = 1;

            string winner = potentialVictor; // PhotonNetwork.LocalPlayer.NickName;

            photonView.RPC("WinState", RpcTarget.AllBuffered, winner);
        }

        // IMPORTANT GOAL: Need to find a way to get ALL variables of the same name to change

        /*
        if (!photonView.IsMine)
        {
            survivors = vehiclesLeft;
        }
        */

        /*
        else if (survivors <= 1 && this.health <= 0)
        {
            photonView.RPC("TieState", RpcTarget.AllBuffered);
        }
        */
    }

    [PunRPC] // Will be registered by everyone
    public void WinState(string winner)
    {
        winText.text = winner + " is the winner!";

        RacingGameManager.instance.DisplayQuitButton();
    }

    [PunRPC] // Will be registered by everyone
    public void TieState()
    {
        winText.text = "No one wins!";

        RacingGameManager.instance.DisplayQuitButton();
    }
}
