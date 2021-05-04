using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    // Make the pause manager stay between scenes
    private static PauseManager pauseInstance;
    private DungeonManager dungeon;
    public void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (pauseInstance == null) pauseInstance = this;
        else Destroy(gameObject);

        escape.enabled = false;
    }

    public void UpdateDungeon(DungeonManager dungeonManager)
    { dungeon = dungeonManager; }

    #region Escape Menu

    public Canvas escape;

    // Enables the escape menu
    public void EscapeMenu()
    { escape.enabled = true; }

    public void Unpause()
    { 
        dungeon.TogglePause(false);
        escape.enabled = false;
    }

    public void ReturnToTitle()
    {
        
    }

    public void QuitGame()
    {
        DataManager.SaveOptions();
        Application.Quit();
    }

    #endregion
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
