using UnityEngine;
public class FinishLine : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Level Complete!");
            // Tell the GameManager the level is done
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LevelCompleted();
            }
        }
    }
}