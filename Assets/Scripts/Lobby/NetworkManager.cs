using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{

    [Header("Login UI")]
    public GameObject LoginUIPanel;
    public InputField PlayerNameInput;

    [Header("Connecting Info Panel")]
    public GameObject ConnectingInfoUIPanel;

    [Header("Creating Room Info Panel")]
    public GameObject CreatingRoomInfoUIPanel;

    [Header("GameOptions  Panel")]
    public GameObject GameOptionsUIPanel;

    [Header("Create Room Panel")]
    public GameObject CreateRoomUIPanel;
    public InputField RoomNameInputField;
    public string GameMode;

    [Header("Inside Room Panel")]
    public GameObject InsideRoomUIPanel;
    public Text RoomInfoText;
    public GameObject playerListPrefab;
    public GameObject playerListParent;
    public GameObject StartGameButton;
    public Text GameModeText;

    [Header("Join Random Room Panel")]
    public GameObject JoinRandomRoomUIPanel;

    // Used to help update the Inside Room on all players
    private Dictionary<int, GameObject> playerListGameObjects;
    
    #region Unity Methods
    // Start is called before the first frame update
    void Start()
    {
        ActivatePanel(LoginUIPanel.name);

        PhotonNetwork.AutomaticallySyncScene = true; // when host loads a scene, all other players will load that same scene
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    #endregion

    #region UI Callback Methods
    public void OnLoginButtonClicked()
    {
        string playerName = PlayerNameInput.text;

        if (!string.IsNullOrEmpty(playerName))
        {
            ActivatePanel(ConnectingInfoUIPanel.name);

            if (!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.LocalPlayer.NickName = playerName;
                PhotonNetwork.ConnectUsingSettings();
            }
        }
        else
        {
            Debug.Log("PlayerName is invalid!");
        }
    }

    public void OnCancelButtonClicked()
    {
        ActivatePanel(GameOptionsUIPanel.name);
    }
    
    public void OnCreateRoomButtonClicked()
    {
        ActivatePanel(CreatingRoomInfoUIPanel.name);

        if (GameMode != null) // null guard
        {
            string roomName = RoomNameInputField.text;

            if (string.IsNullOrEmpty(roomName))
            {
                roomName = "Room " + Random.Range(1000, 10000);
            }

            RoomOptions roomOptions = new RoomOptions();
            string[] roomPropertiesInLobby = { "gm" }; // gm = game mode property

            // Use Photon's Hashtable Class
            // Game Modes:
            // rc = racing
            // dr = death race
            ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "gm", GameMode } }; // originally, game mode is set to "rc" by default

            roomOptions.CustomRoomPropertiesForLobby = roomPropertiesInLobby;
            roomOptions.CustomRoomProperties = customRoomProperties;

            PhotonNetwork.CreateRoom(roomName, roomOptions);
        }
    }

    public void OnJoinRandomRoomClicked(string gameMode)
    {
        GameMode = gameMode;

        ExitGames.Client.Photon.Hashtable expectedCustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "gm", GameMode } };
        PhotonNetwork.JoinRandomRoom(expectedCustomRoomProperties, 0);
    }

    public void OnBackButtonClicked()
    {
        ActivatePanel(GameOptionsUIPanel.name);
    }

    public void OnLeaveGameButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void OnStartGameButtonClicked()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("gm"))
        {
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("gm"))
            {
                if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("rc")) // racing game mode
                {
                    PhotonNetwork.LoadLevel("RacingScene");
                }
                else if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("dr")) // death race mode
                {
                    PhotonNetwork.LoadLevel("DeathRaceScene");
                }
            }
        }
    }

    #endregion

    #region Photon Callbacks
    public override void OnConnected()
    {
        Debug.Log("Connected to Internet");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName+ " is connected to Photon");
        ActivatePanel(GameOptionsUIPanel.name);
    }

    public override void OnCreatedRoom()
    {
        // base.OnCreatedRoom();
        Debug.Log(PhotonNetwork.CurrentRoom + " has been created!");
    }

    public override void OnJoinedRoom()
    {
        // base.OnJoinedRoom();
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " has joined the " + PhotonNetwork.CurrentRoom.Name);
        Debug.Log("Player count: " + PhotonNetwork.CurrentRoom.PlayerCount);

        ActivatePanel(InsideRoomUIPanel.name);

        object gameModeName;

        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("gm", out gameModeName))
        {
            Debug.Log(gameModeName.ToString());
            RoomInfoText.text = "Room name: " + PhotonNetwork.CurrentRoom.Name + " " + PhotonNetwork.CurrentRoom.PlayerCount + " / " + PhotonNetwork.CurrentRoom.MaxPlayers;

            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("rc"))
            {
                GameModeText.text = "Racing Mode";
            }
            else if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("dr"))
            {
                GameModeText.text = "Death Race Mode";
            }
        }

        if (playerListGameObjects == null)
        {
            playerListGameObjects = new Dictionary<int, GameObject>();
        }

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject playerListItem = Instantiate(playerListPrefab);
            playerListItem.transform.SetParent(playerListParent.transform);
            playerListItem.transform.localScale = Vector3.one;

            playerListItem.GetComponent<PlayerListItemInitializer>().Initialize(player.ActorNumber, player.NickName);

            object isPlayerReady;
            if (player.CustomProperties.TryGetValue(Constants.PLAYER_READY, out isPlayerReady))
            {
                playerListItem.GetComponent<PlayerListItemInitializer>().SetPlayerReady((bool) isPlayerReady);
            }

            playerListGameObjects.Add(player.ActorNumber, playerListItem);
        }

        StartGameButton.SetActive(false); // default state
    }

    // Used to update the Inside Room on all players
    // OnPlayerEnteredRoom and OnPlayerLeftRoom are callbacks for other players joining and leaving the room that you're currently in
    public override void OnPlayerEnteredRoom(Player newPlayer) // First player will see the names of newer players appear on their end
    {
        // base.OnPlayerEnteredRoom(newPlayer);

        GameObject playerListItem = Instantiate(playerListPrefab);
        playerListItem.transform.SetParent(playerListParent.transform);
        playerListItem.transform.localScale = Vector3.one;

        playerListItem.GetComponent<PlayerListItemInitializer>().Initialize(newPlayer.ActorNumber, newPlayer.NickName);

        playerListGameObjects.Add(newPlayer.ActorNumber, playerListItem);

        // To update player count
        RoomInfoText.text = "Room name: " + PhotonNetwork.CurrentRoom.Name + " " + PhotonNetwork.CurrentRoom.PlayerCount + " / " + PhotonNetwork.CurrentRoom.MaxPlayers;

        StartGameButton.SetActive(CheckAllPlayerReady()); // Check status of Start Game Button whenever a new player enters
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // base.OnPlayerLeftRoom(otherPlayer);

        Destroy(playerListGameObjects[otherPlayer.ActorNumber].gameObject); // Destroy playerListItem Game Object
        playerListGameObjects.Remove(otherPlayer.ActorNumber); // Remove from Dictionary

        // To update player count
        RoomInfoText.text = "Room name: " + PhotonNetwork.CurrentRoom.Name + " " + PhotonNetwork.CurrentRoom.PlayerCount + " / " + PhotonNetwork.CurrentRoom.MaxPlayers;

    }

    public override void OnLeftRoom()
    {
        // base.OnLeftRoom();

        ActivatePanel(GameOptionsUIPanel.name);

        foreach(GameObject playerlistGameObject in playerListGameObjects.Values)
        {
            Destroy(playerlistGameObject);
        }

        playerListGameObjects.Clear();
        playerListGameObjects = null;
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        // base.OnJoinRandomFailed(returnCode, message);
        Debug.Log(message);

        if (GameMode != null) // null guard
        {
            string roomName = RoomNameInputField.text;

            if (string.IsNullOrEmpty(roomName))
            {
                roomName = "Room " + Random.Range(1000, 10000);
            }

            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = 3;
            string[] roomPropertiesInLobby = { "gm" }; // gm = game mode property

            // Use Photon's Hashtable Class
            // Game Modes:
            // rc = racing
            // dr = death race
            ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "gm", GameMode } }; // originally, game mode is set to "rc" by default

            roomOptions.CustomRoomPropertiesForLobby = roomPropertiesInLobby;
            roomOptions.CustomRoomProperties = customRoomProperties;

            PhotonNetwork.CreateRoom(roomName, roomOptions);
        }
    }

    // Used to update properties (ie. check mark bool) for all players
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        // base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
        GameObject playerlistGameObject;

        if (playerListGameObjects.TryGetValue(targetPlayer.ActorNumber, out playerlistGameObject))
        {
            object isPlayerReady;

            if (changedProps.TryGetValue(Constants.PLAYER_READY, out isPlayerReady))
            {
                playerlistGameObject.GetComponent<PlayerListItemInitializer>().SetPlayerReady((bool) isPlayerReady);
            }
        }

        StartGameButton.SetActive(CheckAllPlayerReady()); // Update Start Game Button in case another player changes his/her status.
    }

    public override void OnMasterClientSwitched(Player newMasterClient) // In case the master client switches
    {
        // base.OnMasterClientSwitched(newMasterClient);
        if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
        {
            StartGameButton.SetActive(CheckAllPlayerReady());
        }
    }
    #endregion

    #region Public Methods
    public void ActivatePanel(string panelNameToBeActivated)
    {
        LoginUIPanel.SetActive(LoginUIPanel.name.Equals(panelNameToBeActivated));
        ConnectingInfoUIPanel.SetActive(ConnectingInfoUIPanel.name.Equals(panelNameToBeActivated));
        CreatingRoomInfoUIPanel.SetActive(CreatingRoomInfoUIPanel.name.Equals(panelNameToBeActivated));
        CreateRoomUIPanel.SetActive(CreateRoomUIPanel.name.Equals(panelNameToBeActivated));
        GameOptionsUIPanel.SetActive(GameOptionsUIPanel.name.Equals(panelNameToBeActivated));
        JoinRandomRoomUIPanel.SetActive(JoinRandomRoomUIPanel.name.Equals(panelNameToBeActivated));
        InsideRoomUIPanel.SetActive(InsideRoomUIPanel.name.Equals(panelNameToBeActivated));
    }

    public void SetGameMode(string gameMode)
    {
        GameMode = gameMode;
    }
    #endregion

    #region Private Methods
    private bool CheckAllPlayerReady()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return false;
        }

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            object isPlayerReady;

            if (p.CustomProperties.TryGetValue(Constants.PLAYER_READY, out isPlayerReady)) // if at least one isPlayerReady is false
            {
                if (!(bool) isPlayerReady)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        return true; // only occurs if no other end statement resulted in false, which ends the function prematurely.
    }
    #endregion
}
