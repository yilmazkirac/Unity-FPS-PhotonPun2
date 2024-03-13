using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public Transform[] SpawnPoints;
    public static SpawnPoint Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        Instance = this;

    }
    private void Start()
    {
        foreach (Transform t in SpawnPoints)
        {
            t.gameObject.SetActive(false);
        }
    }
    public Transform GetSpawnPoint()
    {
        return SpawnPoints[Random.Range(0,SpawnPoints.Length)];
    }
}
