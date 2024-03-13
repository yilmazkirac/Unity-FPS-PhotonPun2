using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using System.Collections.Generic;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance);
            return;
        }
        Instance = this;
    }
    public GameObject MenuButtons;

    public GameObject LoadingScreen;
    public TextMeshProUGUI LoadingText;


    public GameObject CreateRoomScreen;
    public TMP_InputField RoomNameInput;

    public GameObject RoomScreen;
    public TextMeshProUGUI RoomNameText,PlayerNameText;
    public List<TextMeshProUGUI> AllPlayerNames = new List<TextMeshProUGUI>();

    public GameObject ErrorScreen;
    public TextMeshProUGUI ErrorText;

    public GameObject RoomBrowserScreen;
    public RomButton TheRoomButton;
    public List<RomButton> AllRoomButtons=new List<RomButton>();

    public GameObject NameInputScreen;
    public TMP_InputField NameInput;
    private bool hasSetNick;

    public string LevelToPlay;
    public GameObject StartButton;

    public GameObject RoomTestButton;

 
    private void Start()
    {
        CloseMenus();
        LoadingScreen.SetActive(true);
        LoadingText.text = "Connecting To Network...";
        PhotonNetwork.ConnectUsingSettings();

#if UNITY_EDITOR
        RoomTestButton.SetActive(true);
#endif

    }
    public void CloseMenus()
    {
        LoadingScreen.SetActive(false);
        MenuButtons.SetActive(false);
        CreateRoomScreen.SetActive(false);
        RoomScreen.SetActive(false);
        ErrorScreen.SetActive(false);
        RoomBrowserScreen.SetActive(false);
        NameInputScreen.SetActive(false);
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene=true;
        LoadingText.text = "Joining Lobby...";
    }
    public override void OnJoinedLobby()
    {
        CloseMenus();
        MenuButtons.SetActive(true);
       // PhotonNetwork.NickName=Random.Range(0, 1000).ToString();
        if (!hasSetNick)
        {
            CloseMenus();
            NameInputScreen.SetActive(true);
            if (PlayerPrefs.HasKey("playerName"))
            {
                NameInput.text = PlayerPrefs.GetString("playerName");
            }
        }
        else
        {
            PhotonNetwork.NickName= PlayerPrefs.GetString("playerName");
        }
    }
    public void OpenRoomCreate()
    {
        CloseMenus();
        CreateRoomScreen.SetActive(true);
    }
    public void CreateRoom()
    {
        if (!string.IsNullOrEmpty(RoomNameInput.text))
        {
            RoomOptions options= new RoomOptions();
            options.MaxPlayers = 8;
         
            PhotonNetwork.CreateRoom(RoomNameInput.text, options);

            CloseMenus();
            LoadingText.text = "Creating Room...";
            LoadingScreen.SetActive(true);
        }
    }

    public override void OnJoinedRoom()
    {
        CloseMenus();
        RoomScreen.SetActive(true);

        RoomNameText.text = PhotonNetwork.CurrentRoom.Name;
        ListAllPlayers();
        if (PhotonNetwork.IsMasterClient)
        {
            StartButton.SetActive(true);
        }
        else
        {
            StartButton.SetActive(false);
        }
    }
    private void ListAllPlayers()
    {
        foreach (var item in AllPlayerNames)
        {
            Destroy(item.gameObject);
        }
        AllPlayerNames.Clear();

        Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            TextMeshProUGUI newPlayerText = Instantiate(PlayerNameText, PlayerNameText.transform.parent);
            newPlayerText.text = players[i].NickName;
            newPlayerText.gameObject.SetActive(true);

            AllPlayerNames.Add(newPlayerText);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        TextMeshProUGUI newPlayerText = Instantiate(PlayerNameText, PlayerNameText.transform.parent);
        newPlayerText.text = newPlayer.NickName;
        newPlayerText.gameObject.SetActive(true);

        AllPlayerNames.Add(newPlayerText);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ListAllPlayers();
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {        
        ErrorText.text = "Failed To Create Room: " + message;
        CloseMenus();
        ErrorScreen.SetActive(true);
    }
    public void CloseErrorScreen()
    {
        CloseMenus();
        MenuButtons.SetActive(true);
    }
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        CloseMenus();
        LoadingText.text = "Leaving Room";
        LoadingScreen.SetActive(true);
    }
    public override void OnLeftLobby()
    {
        CloseMenus();
        MenuButtons.SetActive(true);
    }
    public void OpenRoomBrowser()
    {
        CloseMenus();
        RoomBrowserScreen.SetActive(true);
    }
    public void CloseRoomBrowser()
    {
        CloseMenus();
        MenuButtons.SetActive(true);
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RomButton rb in AllRoomButtons)
        {
            Destroy(rb.gameObject);
        }
        AllRoomButtons.Clear();
        TheRoomButton.gameObject.SetActive(false);

        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].PlayerCount != roomList[i].MaxPlayers && !roomList[i].RemovedFromList)
            {
                RomButton newButton = Instantiate(TheRoomButton, TheRoomButton.transform.parent);
                newButton.SetButtonDetails(roomList[i]);
                newButton.gameObject.SetActive(true);
                AllRoomButtons.Add(newButton);
            }
        }
    }
    public void JoinRoom(RoomInfo inputInfo)
    {
        PhotonNetwork.JoinRoom(inputInfo.Name);
        CloseMenus();
        LoadingText.text = "Joining Room";
        LoadingScreen.SetActive(true);
    }
  
    public void SetNickName()
    {
        if (!string.IsNullOrEmpty(NameInput.text))
        {
            PhotonNetwork.NickName = NameInput.text;
            PlayerPrefs.SetString("playerName", NameInput.text);
            CloseMenus();
            MenuButtons.SetActive(true);
            hasSetNick = true;
        }
    }
    public void StartGames()
    {
        PhotonNetwork.LoadLevel(LevelToPlay);
    }
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartButton.SetActive(true);
        }
        else
        {
            StartButton.SetActive(false);
        }
    }
    public void QuickJoin()
    {
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 8;
        PhotonNetwork.CreateRoom("Test", options);
        CloseMenus();
        LoadingText.text = "Creating Room";
        LoadingScreen.SetActive(true);  
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
