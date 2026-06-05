using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable; 

public class NetworkManager : MonoBehaviourPunCallbacks 
{
    [Header("Start Menu UI")]
    public GameObject startPanel; // --- NEW: Holds Single Player & Multiplayer buttons ---

    [Header("Lobby UI Elements")]
    public GameObject lobbyPanel; 
    public Button createButton;
    public Button joinButton;
    public TMP_Text statusText;

    [Header("Waiting Room UI Elements")]
    public GameObject waitingRoomPanel; 
    public Button readyButton;
    public TMP_Text waitingRoomText;
    public TMP_Text countdownText;
    public TMP_Text readyCountText; 

    private bool isReady = false;

    void Start()
    {
        // 1. Show only the Start Menu when the game boots up
        startPanel.SetActive(true);
        lobbyPanel.SetActive(false);
        waitingRoomPanel.SetActive(false);
        
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // --- NEW: START MENU BUTTON METHODS ---

    public void OnSinglePlayerClicked()
    {
        // Disconnect from the internet just in case, turn on the fake local server, and create a fake room
        if (PhotonNetwork.IsConnected) PhotonNetwork.Disconnect();
        
        PhotonNetwork.OfflineMode = true;
        PhotonNetwork.CreateRoom("OfflineRoom"); 
    }

    public void OnMultiplayerClicked()
    {
        // Hide the Start Menu, show the Lobby, and connect to the internet
        startPanel.SetActive(false);
        lobbyPanel.SetActive(true);
        
        createButton.interactable = false;
        joinButton.interactable = false;
        statusText.text = "Connecting to Servers...";
        
        PhotonNetwork.ConnectUsingSettings();
    }

    // --- EXISTING LOBBY LOGIC ---

    public override void OnConnectedToMaster()
    {
        statusText.text = "Connected to Master! Joining Lobby...";
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        statusText.text = "In Lobby. Ready to play!";
        createButton.interactable = true;
        joinButton.interactable = true;
    }

    public void CreateGameRoom()
    {
        statusText.text = "Creating Room...";
        RoomOptions roomOptions = new RoomOptions() { MaxPlayers = 2 };
        PhotonNetwork.CreateRoom("TypingArena", roomOptions);
    }

    public void JoinGameRoom()
    {
        statusText.text = "Joining Room...";
        PhotonNetwork.JoinRoom("TypingArena");
    }

    // --- UPDATED ROOM LOGIC ---

    public override void OnJoinedRoom()
    {
        // --- NEW: If we are in Single Player, skip the waiting room and load instantly! ---
        if (PhotonNetwork.OfflineMode)
        {
            PhotonNetwork.LoadLevel("GameScene");
            return;
        }

        // Otherwise, do the normal Multiplayer Waiting Room stuff
        lobbyPanel.SetActive(false);
        waitingRoomPanel.SetActive(true);
        
        countdownText.text = "";
        SetPlayerReadyState(false);
        UpdateWaitingRoomText();
        UpdateReadyCountUI(); 
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateWaitingRoomText();
        UpdateReadyCountUI(); 
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        SetPlayerReadyState(false);
        UpdateWaitingRoomText();
        UpdateReadyCountUI(); 
        countdownText.text = "Player left. Waiting...";
    }

    public void ToggleReady()
    {
        SetPlayerReadyState(!isReady);
    }

    private void SetPlayerReadyState(bool ready)
    {
        isReady = ready;
        readyButton.GetComponentInChildren<TMP_Text>().text = isReady ? "UNREADY" : "READY";

        Hashtable props = new Hashtable() { { "IsReady", isReady } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    private void UpdateWaitingRoomText()
    {
        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        waitingRoomText.text = $"Players in Room: {playerCount} / 2";
    }

    private void UpdateReadyCountUI()
    {
        if (readyCountText == null) return;

        int readyPlayers = 0;
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if (p.CustomProperties.TryGetValue("IsReady", out object readyState) && (bool)readyState)
            {
                readyPlayers++;
            }
        }
        readyCountText.text = $"Players Ready: {readyPlayers} / 2";
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey("IsReady"))
        {
            UpdateReadyCountUI(); 
            CheckIfAllPlayersReady();
        }
    }

    private void CheckIfAllPlayersReady()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (PhotonNetwork.CurrentRoom.PlayerCount != 2) return;

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if (!p.CustomProperties.TryGetValue("IsReady", out object readyState) || !(bool)readyState)
            {
                return; 
            }
        }

        photonView.RPC("StartCountdown_RPC", RpcTarget.All);
    }

    [PunRPC]
    private void StartCountdown_RPC()
    {
        readyButton.interactable = false; 
        StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        for (int i = 3; i > 0; i--)
        {
            countdownText.text = $"Game Starting In: {i}";
            yield return new WaitForSeconds(1f);
        }

        countdownText.text = "GO!";
        yield return new WaitForSeconds(0.5f);

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("GameScene");
        }
    }
}