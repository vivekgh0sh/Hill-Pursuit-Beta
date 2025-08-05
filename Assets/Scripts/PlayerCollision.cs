using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlayerCollision : MonoBehaviour
{
    public System.Action onCrash;
    public System.Action<int> onDiamond;

    [Header("FX")]
    public AudioSource audioSource;
    public AudioClip diamondClip;
    public AudioClip crashClip;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Obstacle"))
        {
            audioSource?.PlayOneShot(crashClip);
            onCrash?.Invoke();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Diamond"))
        {
            audioSource?.PlayOneShot(diamondClip);
            onDiamond?.Invoke(1);
            Destroy(other.gameObject);
        }
    }
}