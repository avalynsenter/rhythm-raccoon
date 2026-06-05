using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable; 

public class NetworkManager : MonoBehaviourPunCallbacks 
{
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
    // --- NEW: Slot for the Ready Count Text ---
    public TMP_Text readyCountText; 

    private bool isReady = false;

    void Start()
    {
        createButton.interactable = false;
        joinButton.interactable = false;
        waitingRoomPanel.SetActive(false); 
        lobbyPanel.SetActive(true);
        statusText.text = "Connecting to Servers...";

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
    }

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

    public override void OnJoinedRoom()
    {
        lobbyPanel.SetActive(false);
        waitingRoomPanel.SetActive(true);
        
        countdownText.text = "";
        SetPlayerReadyState(false);
        UpdateWaitingRoomText();
        UpdateReadyCountUI(); // --- NEW: Update text on join ---
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateWaitingRoomText();
        UpdateReadyCountUI(); // --- NEW ---
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        SetPlayerReadyState(false);
        UpdateWaitingRoomText();
        UpdateReadyCountUI(); // --- NEW ---
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

    // --- NEW: Helper method to count ready players ---
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
            UpdateReadyCountUI(); // --- NEW: Update UI when someone clicks ready ---
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

        // --- UPDATED: Safer, cleaner RPC call ---
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