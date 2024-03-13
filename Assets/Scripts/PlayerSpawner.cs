using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public static PlayerSpawner Instance;
    public GameObject PlayerPrefab;
    private GameObject player;
    public GameObject DeathEffect;
    public float RespawnTime=5f;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            SpawnPlayer();
        }
    }
  public void SpawnPlayer()
    {
        Transform spawnPoint= SpawnPoint.Instance.GetSpawnPoint();
        player= PhotonNetwork.Instantiate(PlayerPrefab.name,spawnPoint.position,spawnPoint.rotation);
    }
    public void Die(string damager)
    {
       
        UIController.Instance.DeathText.text = "You were killed by " + damager;

        // PhotonNetwork.Destroy(player);
        // SpawnPlayer();
        MatchManager.Instance.UpdateStatsSend(PhotonNetwork.LocalPlayer.ActorNumber,1,1);
        if (player!=null)
        {
            StartCoroutine(DieCo());
        }
    }

    public IEnumerator DieCo()
    {
        PhotonNetwork.Instantiate(DeathEffect.name, player.transform.position, Quaternion.identity);
        PhotonNetwork.Destroy(player);
        UIController.Instance.DeathScreen.SetActive(true);
        yield return new WaitForSeconds(RespawnTime);
        UIController.Instance.DeathScreen.SetActive(false);
        SpawnPlayer();
    }
}
