using UnityEngine;

public class Gun : MonoBehaviour
{
    public bool isAutomatic;
    public float TimeBetweenShots=.1f,HeatPerShot=1f;
    public GameObject MuzzleFlash;
    public int ShotDamage;
}
