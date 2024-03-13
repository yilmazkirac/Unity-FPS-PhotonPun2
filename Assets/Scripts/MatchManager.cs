using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public enum EventCodes : byte
{
    NewPlayer,
    ListPlayers,
    UpdateStat
}


public class MatchManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public List<PlayerInfo> AllPlayers = new List<PlayerInfo>();

    private int index;
    private List<LeaderBoardPlayer> lboardPlayers= new List<LeaderBoardPlayer>();


    public static MatchManager Instance;

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
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            NewPlayerSend(PhotonNetwork.NickName);
        }
    }
    private void Update()
    {
       if(Input.GetKeyDown(KeyCode.Tab))
        {
            if (UIController.Instance.LeaderBoard.activeInHierarchy)
            {
                UIController.Instance.LeaderBoard.SetActive(false);
            }
            else
            {
                ShowLeaderBoard();
            }
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code < 200)
        {
            EventCodes theEvent = (EventCodes)photonEvent.Code;
            object[] data = (object[])photonEvent.CustomData;

            switch (theEvent)
            {
                case EventCodes.NewPlayer:
                    NewPlayerReceive(data);
                    break;

                case EventCodes.ListPlayers:
                    ListPlayersReceive(data);
                    break;

                case EventCodes.UpdateStat:
                    UpdateStatsReceive(data);
                    break;
            }
        }
    }
    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }
    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void NewPlayerSend(string userName)
    {
        object[] package = new object[4];
        package[0] = userName;
        package[1] = PhotonNetwork.LocalPlayer.ActorNumber;
        package[2] = 0;
        package[3] = 0;

        PhotonNetwork.RaiseEvent((byte)EventCodes.NewPlayer,package,new RaiseEventOptions {Receivers=ReceiverGroup.MasterClient},new SendOptions { Reliability=true});
    }
    public void NewPlayerReceive(object[] dataReceived)
    {
        PlayerInfo player = new PlayerInfo((string)dataReceived[0],(int)dataReceived[1], (int)dataReceived[2], (int)dataReceived[3]);
        AllPlayers.Add(player);
        ListPlayersSend();
    }
    public void ListPlayersSend()
    {
        object[] package =new object[AllPlayers.Count];
        for (int i = 0; i < AllPlayers.Count; i++)
        {
            object[] piece=new object[4];
            piece[0] = AllPlayers[i].Name;
            piece[1] = AllPlayers[i].Actor;
            piece[2] = AllPlayers[i].Kills;
            piece[3] = AllPlayers[i].Death;

            package[i] = piece;
        }
        PhotonNetwork.RaiseEvent((byte)EventCodes.ListPlayers, package, new RaiseEventOptions { Receivers = ReceiverGroup.All }, new SendOptions { Reliability = true });
    }
    public void ListPlayersReceive(object[] dataReceived)
    {
        AllPlayers.Clear();
        for (int i = 0; i < dataReceived.Length; i++)
        {
            object[] piece= (object[])dataReceived[i];
            PlayerInfo player = new PlayerInfo((string)piece[0], (int)piece[1], (int)piece[2], (int)piece[3]);

            AllPlayers.Add(player);
            if (PhotonNetwork.LocalPlayer.ActorNumber==player.Actor)
            {
                index = i;
            }
        }
    }
    public void UpdateStatsSend(int actorSending,int statToUpdate,int amountToChange)
    {
        object[] package = new object[] { actorSending, statToUpdate, amountToChange };
        PhotonNetwork.RaiseEvent((byte)EventCodes.UpdateStat, package, new RaiseEventOptions { Receivers = ReceiverGroup.All }, new SendOptions { Reliability = true });
    }
    public void UpdateStatsReceive(object[] dataReceived)
    {
        int actor = (int)dataReceived[0];
        int statType = (int)dataReceived[1];
        int amount = (int)dataReceived[2];

        for (int i = 0; i < AllPlayers.Count; i++)
        {
            if (AllPlayers[i].Actor==actor)
            {
                switch (statType)
                {
                    case 0:
                        AllPlayers[i].Kills += amount;
                        
                        break;
                    case 1:
                        AllPlayers[i].Death += amount;

                        break;
                }
                if (i==index)
                {
                    UpdateStatsDisplay();
                }
                if(UIController.Instance.LeaderBoard.activeInHierarchy)
                {
                    ShowLeaderBoard();
                }
                break;
            }
        }
    }
    public void UpdateStatsDisplay()
    {
        if (AllPlayers.Count>index)
        {
            UIController.Instance.KillsText.text = "KILLS: " + AllPlayers[index].Kills;
            UIController.Instance.DeathsText.text = "DEATHS: " + AllPlayers[index].Death;
        }
        else
        {
            UIController.Instance.KillsText.text = "KILLS: 0";
            UIController.Instance.DeathsText.text = "DEATHS: 0";
        }
    }
    void ShowLeaderBoard()
    {
        UIController.Instance.LeaderBoard.SetActive(true);
        foreach (var item in lboardPlayers)
        {
            Destroy(item.gameObject);
        }
        lboardPlayers.Clear();
        UIController.Instance.LeaderBoardPlayerDisplay.gameObject.SetActive(false);

        List<PlayerInfo> sorted = SortPlayers(AllPlayers);

        foreach (var item in sorted)
        {
            LeaderBoardPlayer newPlayerDisplay = Instantiate(UIController.Instance.LeaderBoardPlayerDisplay,UIController.Instance.LeaderBoardPlayerDisplay.transform.parent);
            newPlayerDisplay.SetDetails(item.Name, item.Kills, item.Death);
            newPlayerDisplay.gameObject.SetActive(true);
            lboardPlayers.Add(newPlayerDisplay);
        }
    }
    private List<PlayerInfo> SortPlayers(List<PlayerInfo> players)
    {
        List<PlayerInfo> sorted= new List<PlayerInfo>();
        while (sorted.Count < players.Count)
        {
            int highes = -1;
            PlayerInfo selectedPlayer = players[0];

            foreach (PlayerInfo player in players)
            {
                if(!sorted.Contains(player))
                { 
                if (player.Kills> highes)
                {
                    selectedPlayer = player;
                    highes=player.Kills;
                }
               }
            }
            sorted.Add(selectedPlayer);
        }
        return sorted;
    }
}


[Serializable]
public class PlayerInfo
{
    public string Name;
    public int Actor, Kills, Death;
    public PlayerInfo(string name, int actor, int kills, int death)
    {
        Name = name;
        Actor = actor;
        Kills = kills;
        Death = death;
    }
}