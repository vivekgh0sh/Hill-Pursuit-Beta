// --- CREATE NEW FILE: LevelButton.cs ---

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelButton : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private GameObject lockIcon;
    [SerializeField] private Button button;

    private int levelIndex;
    private bool isUnlocked;

    // This method is called by the LevelSelectUI to configure the button
    public void Setup(int index, bool unlocked)
    {
        levelIndex = index;
        isUnlocked = unlocked;

        levelText.text = (levelIndex + 1).ToString(); // Display level 1, 2, 3 instead of 0, 1, 2

        if (isUnlocked)
        {
            lockIcon.SetActive(false);
            button.interactable = true;
        }
        else
        {
            lockIcon.SetActive(true);
            button.interactable = false;
        }
    }

    void Start()
    {
        // Add a listener to the button's onClick event
        button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        if (isUnlocked && GameManager.Instance != null)
        {
            // Tell the GameManager to load the level this button represents
            GameManager.Instance.LoadLevel(levelIndex);
        }
    }
}