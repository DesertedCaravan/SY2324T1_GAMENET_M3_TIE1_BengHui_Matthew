using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using TMPro;

public class LapController : MonoBehaviourPunCallbacks
{
    public List<GameObject> lapTriggers = new List<GameObject>();

    public enum RaiseEventsCode
    {
        WhoFinishedEventCode = 0
    }

    private int finishOrder = 0;

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == (byte)RaiseEventsCode.WhoFinishedEventCode)
        {
            object[] data = (object[])photonEvent.CustomData;

            // object data array in GameFinish()
            string nickNameOffFinishedPlayer = (string)data[0];
            finishOrder = (int)data[1];
            int viewId = (int)data[2];

            Debug.Log(nickNameOffFinishedPlayer + " " + finishOrder);

            GameObject orderUiText = RacingGameManager.instance.finisherTextUi[finishOrder - 1];
            orderUiText.SetActive(true);

            if (viewId == photonView.ViewID)
            {
                orderUiText.GetComponent<TextMeshProUGUI>().text = finishOrder + " " + nickNameOffFinishedPlayer + "(YOU)";
                orderUiText.GetComponent<TextMeshProUGUI>().color = Color.red;
            }
            else
            {
                orderUiText.GetComponent<TextMeshProUGUI>().text = finishOrder + " " + nickNameOffFinishedPlayer;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        foreach (GameObject go in RacingGameManager.instance.lapTriggers)
        {
            lapTriggers.Add(go);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("egrgefr");

        if (lapTriggers.Contains(other.gameObject))
        {
            int indexOfTrigger = lapTriggers.IndexOf(other.gameObject);

            Debug.Log("Lap Trigger: " + indexOfTrigger);

            lapTriggers[indexOfTrigger].SetActive(false);
        }

        if (other.gameObject.tag == "FinishTrigger")
        {
            GameFinish();
        }

    }

    public void GameFinish()
    {
        GetComponent<PlayerSetup>().camera.transform.parent = null;
        GetComponent<VehicleMovementController>().enabled = false;

        // increment finish order
        finishOrder++;

        string nickName = photonView.Owner.NickName;
        int viewId = photonView.ViewID;

        // event data
        object[] data = new object[] { nickName, finishOrder, viewId };

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.All,
            CachingOption = EventCaching.AddToRoomCache
        };

        SendOptions sendOptions = new SendOptions
        {
            Reliability = false
        };

        PhotonNetwork.RaiseEvent((byte) RaiseEventsCode.WhoFinishedEventCode, data, raiseEventOptions, sendOptions);

        // data.Length is 3
        if (finishOrder >= PhotonNetwork.CurrentRoom.PlayerCount)
        {
            photonView.RPC("DisplayQuitButton", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    public void DisplayQuitButton()
    {
        RacingGameManager.instance.DisplayQuitButton();
    }
}
