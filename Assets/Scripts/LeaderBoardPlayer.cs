using TMPro;
using UnityEngine;

public class LeaderBoardPlayer : MonoBehaviour
{
    public TextMeshProUGUI PlayerNameText, KillsText, DeathsText;
    public void SetDetails(string name,int kills,int deaths)
    {
        PlayerNameText.text = name;
        KillsText.text = kills.ToString();
        DeathsText.text = deaths.ToString();
    }
}
