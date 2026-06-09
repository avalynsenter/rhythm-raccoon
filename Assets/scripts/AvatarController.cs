using UnityEngine;
using Photon.Pun; // --- NEW: We need this to check our Host status ---

public class AvatarController : MonoBehaviour
{
    public static AvatarController Instance { get; private set; }

    [Header("Avatars")]
    public SpriteRenderer localPlayer;
    public SpriteRenderer opponent;

    // --- NEW: Variables to remember our assigned colors ---
    private Color myBaseColor;
    private Color opponentBaseColor;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        // 1. Determine who we are on the network
        // (If playing Single Player Offline Mode, IsMasterClient is automatically true)
        if (PhotonNetwork.IsMasterClient)
        {
            myBaseColor = Color.green;         // Host is Green
            opponentBaseColor = Color.yellow;  // Guest is Yellow
        }
        else
        {
            myBaseColor = Color.yellow;        // Guest is Yellow
            opponentBaseColor = Color.green;   // Host is Green
        }

        // 2. Apply the colors immediately 
        if (localPlayer != null) localPlayer.color = myBaseColor;
        if (opponent != null) opponent.color = opponentBaseColor;
    }

    public void PlayLocalDamageEffect()
    {
        if (localPlayer == null) return;
        
        localPlayer.color = Color.red;
        Invoke(nameof(ResetLocalColor), 0.5f);
    }

    public void PlayOpponentDamageEffect()
    {
        if (opponent == null) return;

        opponent.color = Color.red;
        Invoke(nameof(ResetOpponentColor), 0.5f);
    }

    // --- UPDATED: Revert back to our specific identity colors, not just White ---
    private void ResetLocalColor() => localPlayer.color = myBaseColor;
    private void ResetOpponentColor() => opponent.color = opponentBaseColor;
}