using Photon.Realtime;
using TMPro;
using UnityEngine;

public class RomButton : MonoBehaviour
{
    public TextMeshProUGUI ButtonText;
    private RoomInfo roomInfo;
      
    
    public void SetButtonDetails(RoomInfo inputInfo)
    {
        roomInfo = inputInfo;
        ButtonText.text = roomInfo.Name;
    }
    public void OpenRoom()
    {
        Launcher.Instance.JoinRoom(roomInfo);
    }
}
