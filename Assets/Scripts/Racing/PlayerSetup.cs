using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;

public class PlayerSetup : MonoBehaviourPunCallbacks
{
    public Camera camera;
    public GameObject playerNameText;
    public GameObject healthBarBackground;
    public GameObject healthBarImage;

    // public GameObject playerUiPrefab;

    // Start is called before the first frame update
    void Start()
    {
        this.camera = transform.Find("Camera").GetComponent<Camera>();
        camera.enabled = photonView.IsMine;

        playerNameText.GetComponent<TextMeshProUGUI>().text = photonView.Owner.NickName;
        this.playerNameText.SetActive(!photonView.IsMine);

        /*
        this.playerNameText.enabled = !photonView.IsMine;
        this.healthBarBackground.enabled = !photonView.IsMine;
        this.healthBarImage.enabled = !photonView.IsMine;
        */

        GetComponent<VehicleMovementController>().enabled = photonView.IsMine; // enable Vehicle Movement only if it's the client's Vehicle Movement
        GetComponent<CountdownManager>().enabled = photonView.IsMine; // enable CountdownManager only if it's the client's Vehicle Movement

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("rc"))
        {
            this.healthBarBackground.SetActive(false);
            this.healthBarImage.SetActive(false);

            GetComponent<LapController>().enabled = photonView.IsMine;
            GetComponent<ShootingController>().enabled = false;
        }
        else if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("dr"))
        {
            this.healthBarBackground.SetActive(!photonView.IsMine);
            this.healthBarImage.SetActive(!photonView.IsMine);

            GetComponent<LapController>().enabled = false;
            GetComponent<ShootingController>().enabled = true; // health and shooting are in the same .cs script
        }

        // CUSTOM: Made the Player UI into a separate prefab
        /*
        if (photonView.IsMine)
        {
            GameObject playerUi = Instantiate(playerUiPrefab); // Instantiate playerUiPrefab, assigning it the name "playerUi"
            Text countdownText = playerUi.transform.Find("TimerText").GetComponent<Text>();
            CountdownManager.instance.SetTimerText(countdownText);
        }
        */
    }
}
