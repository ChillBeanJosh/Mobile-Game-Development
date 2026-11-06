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
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}
