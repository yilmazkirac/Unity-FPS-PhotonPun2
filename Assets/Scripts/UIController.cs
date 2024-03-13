using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public Slider WeaponTempSlider;
    public GameObject DeathScreen;
    public TextMeshProUGUI DeathText;
    public TextMeshProUGUI KillsText;
    public TextMeshProUGUI DeathsText;
    public Slider HealthSlider;
    public GameObject LeaderBoard;
    public LeaderBoardPlayer LeaderBoardPlayerDisplay;



    public static UIController Instance;


    private void Awake()
    {
        if (Instance != null&&Instance!=this)
        {
            Destroy(gameObject);
        }
        Instance = this;

    }
    public TextMeshProUGUI OverHeatedMassage;
   // public TMP_Text OverHeatedMassage;
}
