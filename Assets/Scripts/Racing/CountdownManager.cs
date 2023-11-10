using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class CountdownManager : MonoBehaviourPunCallbacks
{
    public TextMeshProUGUI timerText; // No [SerializeField] because it's part of a prefab

    public float timeToStartRace = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
        // Text timerText = playerUi.transform.Find("TimerText").GetComponent<Text>();
        this.timerText = RacingGameManager.instance.timerText; // GetTimeText()        
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (PhotonNetwork.IsMasterClient)
        {
        }
        */

        if (timeToStartRace > 0)
        {
            timeToStartRace -= Time.deltaTime;
            // photonView.RPC("SetTime", RpcTarget.AllBuffered, timeToStartRace);
            SetTime(timeToStartRace);
        }
        else if (timeToStartRace < 0)
        {
            // photonView.RPC("StartRace", RpcTarget.AllBuffered);
            StartRace();
        }
    }

    /*
    public void SetTimerText(Text text)
    {
        this.timerText = text;
        Debug.Log(timerText.text);
    }
    */

    [PunRPC]
    public void SetTime(float time)
    {
        if (time > 0)
        {
            timerText.text = time.ToString("F1");
        }
        else
        {
            timerText.text = "";
        }

        /*
        if (photonView.IsMine) // Because it's a PunRPC, you need to separate it from other players
        {
        }
        */
    }

    [PunRPC]
    public void StartRace()
    {
        GetComponent<VehicleMovementController>().isControlEnabled = true;
        this.enabled = false;
    }
}
