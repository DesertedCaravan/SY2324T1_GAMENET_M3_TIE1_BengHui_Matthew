using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeathRaceGameManager : MonoBehaviour
{
    [Header("Death Race Mode")]
    [SerializeField] public Image healthBar;
    [SerializeField] public GameObject killFeedTextParent;
    [SerializeField] private GameObject killFeedTextPrefab;
    [SerializeField] public TextMeshProUGUI survivorsText;
    [SerializeField] public TextMeshProUGUI winText;

    // Convert to Singleton
    public static DeathRaceGameManager instance = null;

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

    public void AddKillFeed(string attacker, string target)
    {
        GameObject killFeedItem = Instantiate(killFeedTextPrefab);
        killFeedItem.transform.SetParent(killFeedTextParent.transform);
        killFeedItem.transform.localScale = Vector3.one; // update scale

        killFeedItem.GetComponent<TextMeshProUGUI>().text = attacker + " has killed " + target;

        Destroy(killFeedItem, 5.0f);
    }
}
