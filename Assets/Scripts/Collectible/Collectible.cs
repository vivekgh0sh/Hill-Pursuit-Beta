// --- CREATE NEW FILE: Collectible.cs ---

using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Collectible : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The amount of coins to award the player upon collection.")]
    public int coinValue = 10;

    [Header("Effects")]
    [Tooltip("The particle effect to spawn when collected. (Optional)")]
    public GameObject pickupEffectPrefab;

    // In a real game, you would use an audio manager. For simplicity, we'll use AudioSource.
    [Tooltip("The sound to play when collected. (Optional)")]
    public AudioClip pickupSound;


    private void Awake()
    {
        // Ensure the collider is set to be a trigger so it doesn't physically block the car.
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that entered the trigger has the "Player" tag.
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }

    private void Collect()
    {
        // Award the coins to the player through the GameManager.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CollectCoin(coinValue);
        }

        // Play the pickup particle effect, if one is assigned.
        if (pickupEffectPrefab != null)
        {
            Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
        }

        // Play the pickup sound, if one is assigned.
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }

        // Destroy the star object itself.
        Destroy(gameObject);
    }
}