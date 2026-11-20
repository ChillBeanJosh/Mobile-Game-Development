using UnityEngine;

public class LevelSelectionOpenManager : MonoBehaviour
{
    public GameObject OptionsMenu, LevelSelectMenu, SettingsMenu;
   public void OnClickPlay()
    {
        LevelSelectMenu.SetActive(true);
    }

    public void OnClickOptions()
    {
        OptionsMenu.SetActive(true);
    }

    public void OnClickSettings()
    {
        SettingsMenu.SetActive(true);
    }

    public void OnClickClearPlayerPrefs()
    {
        if (StatusUpgrade.Instance != null)
        {
            StatusUpgrade.Instance.ResetAllProgress();
        }
        else
        {
            Debug.LogError("StatusUpgrade instance not found!");
        }
    }
}
