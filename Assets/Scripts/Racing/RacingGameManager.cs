using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RacingGameManager : MonoBehaviourPunCallbacks
{
    [Header("Vehicle Spawning")]
    [SerializeField] public TextMeshProUGUI timerText;
    public GameObject[] vehiclePrefabs;
    public Transform[] startingPositions;

    [Header("Racing Mode")]
    public List<GameObject> lapTriggers = new List<GameObject>();
    public GameObject[] finisherTextUi;

    [Header("Quit Game")]
    public GameObject quitGameButton;

    // Convert to Singleton
    public static RacingGameManager instance = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Destroy(gameObject);
        }

        // DontDestroyOnLoad(gameObject); // Makes it a persistent GameObject
    }

    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            object playerSelectionNumber;

            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(Constants.PLAYER_SELECTION_NUMBER, out playerSelectionNumber))
            {
                Debug.Log((int) playerSelectionNumber);

                int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
                Vector3 instantiatePosition = startingPositions[actorNumber - 1].position;

                PhotonNetwork.Instantiate(vehiclePrefabs[(int)playerSelectionNumber].name, instantiatePosition, Quaternion.identity);
            }
        }

        foreach (GameObject go in finisherTextUi)
        {
            go.SetActive(false);
        }

        quitGameButton.SetActive(false);
    }

    public void DisplayQuitButton()
    {
        quitGameButton.SetActive(true);
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("LobbyScene");
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    /*
    // Needed because the code might not be run in the correct order when starting, Update: The problems was a "==" error in the singleton
    public TextMeshProUGUI GetTimeText()
    {
        return timeText;
    }
    */

}
