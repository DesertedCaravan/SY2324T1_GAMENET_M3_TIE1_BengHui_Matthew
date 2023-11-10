using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;

public class PlayerSetup : MonoBehaviourPunCallbacks
{
    public Camera camera;
    public TextMeshProUGUI playerNameText;
    public Image healthBarBackground;
    public Image healthBarImage;

    // public GameObject playerUiPrefab;

    // Start is called before the first frame update
    void Start()
    {
        this.camera = transform.Find("Camera").GetComponent<Camera>();
        this.playerNameText.enabled = !photonView.IsMine;
        this.healthBarBackground.enabled = !photonView.IsMine;
        this.healthBarImage.enabled = !photonView.IsMine;

        GetComponent<VehicleMovementController>().enabled = photonView.IsMine; // enable Vehicle Movement only if it's the client's Vehicle Movement
        GetComponent<CountdownManager>().enabled = photonView.IsMine; // enable CountdownManager only if it's the client's Vehicle Movement

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("rc"))
        {
            GetComponent<LapController>().enabled = photonView.IsMine;
            GetComponent<ShootingController>().enabled = false;
        }
        else if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("dr"))
        {
            GetComponent<LapController>().enabled = false;
            GetComponent<ShootingController>().enabled = true; // health and shooting are in the same .cs script
        }

        camera.enabled = photonView.IsMine;

        playerNameText.text = photonView.Owner.NickName;

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
