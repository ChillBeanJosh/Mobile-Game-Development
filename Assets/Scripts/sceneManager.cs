using UnityEngine;
using UnityEngine.SceneManagement;

public class sceneManager : MonoBehaviour
{
    public static sceneManager Instance;
    private const string UnlockedLevelsKey = "UnlockedLevels";

    private void Awake()
    {
        Instance = this;
    }

    //List of Scenes in the build Index, make sure they are in ORDER.
    public enum Scene
    {
        MainMenu,
        Gameplay_1,
        Gameplay_2,
        GameOver,
        ClearLevel,
    }

    //List of Scene Functions.

    //Used to refrence any specific scene on the build index.
    public void LoadScene(Scene scene)
    {
        SceneManager.LoadScene(scene.ToString());
    }


    //Used for the Start Game Button.
    public void LoadNewGame1()
    {
        SceneManager.LoadScene(Scene.Gameplay_1.ToString());
    }

    public void LoadNewGame2()
    {
        SceneManager.LoadScene(Scene.Gameplay_2.ToString());
    }

    public void LoadLevelClear()
    {
        SceneManager.LoadScene(Scene.ClearLevel.ToString());
    }

    //Loads next Scene in build index.
    public void LoadNextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }


    //Loads the Main Menu Scene.
    public void LoadMainMenu()
    {
        SceneManager.LoadScene(Scene.MainMenu.ToString());
    }


    public void LoadGameOver()
    {
        SceneManager.LoadScene(Scene.GameOver.ToString());
    }

    //Restarts the current Scene.
    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void CompleteLevel(int completedLevelNumber)
    {
        int currentlyUnlocked = PlayerPrefs.GetInt(UnlockedLevelsKey, 1);

        // Prevent completing a level that hasn't been unlocked.
        if (completedLevelNumber > currentlyUnlocked)
        {
            Debug.LogWarning("Attempted to complete a locked level.");
        }
        else
        {
            // Calculate the next level to unlock.
            int nextLevel = completedLevelNumber + 1;

            // Only update if this completion unlocks something new.
            if (nextLevel > currentlyUnlocked)
            {
                PlayerPrefs.SetInt(UnlockedLevelsKey, nextLevel);
                PlayerPrefs.Save();

                Debug.Log($"Level {nextLevel} Unlocked!");
            }
        }

        LoadLevelClear();
    }
}
