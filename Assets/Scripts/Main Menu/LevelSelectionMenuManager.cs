using UnityEngine;

public class LevelSelectionMenuManager : MonoBehaviour
{
    public LevelObjects[] levelObjects;
    public static int UnlockedLevels;

    public void OnClickBack()
    {
        this.gameObject.SetActive(false);
    }

    private void Start()
    {
        UnlockedLevels = PlayerPrefs.GetInt("UnlockedLevels", 1);
        for (int i = 0; i < levelObjects.Length; i++)
        {
          if (UnlockedLevels >= i)
          {
              levelObjects[i].button.interactable = true;
          }
          else
          {
              levelObjects[i].button.interactable = false;
          }
        }
    }
}
