using UnityEngine;

public class DestroyOverTime : MonoBehaviour
{
    public float lifetime = 1.5f;

    private void Start()
    {
        Destroy(gameObject,lifetime);
    }
}
