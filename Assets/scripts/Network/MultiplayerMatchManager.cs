using UnityEngine;
using Photon.Pun;
using TMPro;

public class MultiplayerMatchManager : MonoBehaviourPun
{
    public static MultiplayerMatchManager Instance { get; private set; }

    [Header("Opponent UI")]
    public TMP_Text opponentScoreText;
    public TMP_Text opponentStaminaText;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    // ==========================================
    // 1. SYNCING SCORE & STAMINA
    // ==========================================

    public void SyncMyScore(int myTotalScore)
    {
        if (PhotonNetwork.OfflineMode || !PhotonNetwork.IsConnected) return;
        
        // Shout my score exclusively to the other player
        photonView.RPC("ReceiveOpponentScore_RPC", RpcTarget.Others, myTotalScore);
    }

    [PunRPC]
    private void ReceiveOpponentScore_RPC(int opponentScore)
    {
        if (opponentScoreText != null) 
        {
            opponentScoreText.text = $"Opponent Score: {opponentScore}";
        }
    }

    public void SyncMyStamina(float myCurrentStamina)
    {
        if (PhotonNetwork.OfflineMode || !PhotonNetwork.IsConnected) return;
        photonView.RPC("ReceiveOpponentStamina_RPC", RpcTarget.Others, myCurrentStamina);
    }

    [PunRPC]
    private void ReceiveOpponentStamina_RPC(float opponentStamina)
    {
        if (opponentStaminaText != null) 
        {
            // You can replace this with a UI slider update later!
            opponentStaminaText.text = $"Opponent Stamina: {opponentStamina}";
        }
    }

    // ==========================================
    // 2. SENDING & RECEIVING ATTACKS
    // ==========================================

    public void SendAttackToOpponent(string attackName)
    {
        if (PhotonNetwork.OfflineMode || !PhotonNetwork.IsConnected) return;
        
        Debug.Log($"Sending attack '{attackName}' to opponent!");
        photonView.RPC("ReceiveAttack_RPC", RpcTarget.Others, attackName);
    }

    [PunRPC]
    private void ReceiveAttack_RPC(string attackName)
    {
        Debug.Log($"I was hit by an attack from the opponent: {attackName}");

        // Here is where you trigger the local punishment based on the string name
        switch (attackName)
        {
            case "SpeedUp":
                // e.g., WordGenerator.Instance.ApplyTemporarySpeedBoost();
                break;
            case "HideLetters":
                // e.g., trigger a UI panel that covers the screen
                break;
            case "StealStamina":
                // e.g., PlayerManager.Instance.LoseStamina(10);
                break;
        }
    }
}