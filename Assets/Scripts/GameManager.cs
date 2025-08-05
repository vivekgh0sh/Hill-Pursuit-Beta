using UnityEngine;

public enum GameState { Playing, Paused, GameOver }

public class GameManager : MonoBehaviour
{
    public GameState state = GameState.Playing;
    public PlayerCollision playerCollision;
    public int diamonds;

    void Awake()
    {
        if (playerCollision != null)
        {
            playerCollision.onCrash += HandleCrash;
            playerCollision.onDiamond += HandleDiamond;
        }
    }

    void OnDestroy()
    {
        if (playerCollision != null)
        {
            playerCollision.onCrash -= HandleCrash;
            playerCollision.onDiamond -= HandleDiamond;
        }
    }

    void HandleCrash()
    {
        if (state != GameState.Playing) return;
        state = GameState.GameOver;
        Time.timeScale = 0.0f; // pause gameplay; replace with nicer flow later
        Debug.Log("Game Over");
    }

    void HandleDiamond(int amount)
    {
        diamonds += amount;
        // Update HUD here
        Debug.Log($"Diamonds: {diamonds}");
    }
}