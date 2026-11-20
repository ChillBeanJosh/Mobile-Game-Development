using UnityEngine;
using UnityEngine.UI;


public class LevelSelectionMenuManager : MonoBehaviour
{
    private const string UnlockedLevelsKey = "UnlockedLevels";
    public LevelObjects[] levelObjects;
    public static int UnlockedLevels;

    private void OnEnable()
    {
        RefreshLevelSelection();
    }

    public void RefreshLevelSelection()
    {
        //Load the number of unlocked levels:
        UnlockedLevels = Mathf.Max(1, PlayerPrefs.GetInt(UnlockedLevelsKey, 1));

        //Update each level button:
        for (int i = 0; i < levelObjects.Length; i++)
        {
            if (levelObjects[i] == null || levelObjects[i].button == null) continue;

            // Index 0 = Level 1
            // Index 1 = Level 2
            // Index 2 = Level 3
            levelObjects[i].button.interactable = i < UnlockedLevels;
        }
    }

    public void OnClickBack()
    {
        this.gameObject.SetActive(false);
    }
}
