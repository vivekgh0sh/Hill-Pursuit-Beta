// --- CREATE NEW FILE: FuelCollectible.cs ---

using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FuelCollectible : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The amount of fuel to add to the car upon collection.")]
    public float fuelAmount = 30f; // Add 30 fuel units by default

    [Header("Effects")]
    [Tooltip("The particle effect to spawn when collected. (Optional)")]
    public GameObject pickupEffectPrefab;
    [Tooltip("The sound to play when collected. (Optional)")]
    public AudioClip pickupSound;

    private void Awake()
    {
        // Ensure the collider is a trigger so it doesn't block the car
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that entered has the "Player" tag
        if (other.CompareTag("Player"))
        {
            // Find the CarController on the player object
            CarController car = other.GetComponentInParent<CarController>();
            if (car != null)
            {
                Collect(car);
            }
        }
    }

    private void Collect(CarController car)
    {
        // Call the public method on the car to add fuel
        car.AddFuel(fuelAmount);

        // Play the pickup particle effect, if assigned
        if (pickupEffectPrefab != null)
        {
            Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
        }

        // Play the pickup sound, if assigned
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }

        // Destroy the fuel can object
        Destroy(gameObject);
    }
}