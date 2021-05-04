using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerManager))]
public class DungeonManager : MonoBehaviour {

    public ConstantConfig constants;
    public MaterialConfig materials;

    public PauseManager pause;
    public CameraManager cam;
    public PlayerManager player;
    public BaseEnemy[] enemies;
    public List<BaseEntity> obstacles;

    private void Awake() 
    {
        // TODO: Move this line to title screen manager
        DataManager.Load();
        Application.targetFrameRate = 60;

        if (!cam) cam = FindObjectOfType<CameraManager>();
        if (!player) player = GetComponent<PlayerManager>();
        if (!pause) pause = FindObjectOfType<PauseManager>();
        if (pause) pause.UpdateDungeon(this);
        
        obstacles = new List<BaseEntity>();
        enemies = FindObjectsOfType<BaseEnemy>();
        var breakables = FindObjectsOfType<BreakableEntity>();
        for (int i = 0; i < enemies.Length; i++) obstacles.Add(enemies[i]);
        for (int i = 0; i < breakables.Length; i++) obstacles.Add(breakables[i]);
    }

    private void Update()
    {
        // TODO: Determine when to actually save later
        if(Input.GetKeyDown(KeyCode.Return)) DataManager.Save();
        if(Input.GetKeyDown(KeyCode.Backspace)) DataManager.WipeSave();
        
        // TODO: Remove in final build
        if(Input.GetKeyDown(KeyCode.R)) ExitDungeon(SceneLoader.Scene.Rooftops);

        if (paused) return;
        // Pause the game
        if (PlayerInput.escapeMenu.isPressed) TogglePause(enabled, true);

        cam.UpdateCamera();
        player.UpdatePlayers();
        Vector2 playerPos = GetActivePlayer().transform.position;
        for (int i = 0; i < enemies.Length; i++)
        {
            if (!obstacles.Contains(enemies[i])) continue;
            Vector2 enemyPos = enemies[i].transform.position;
            float dist = Vector2.Distance(playerPos, enemyPos);
            if (dist <= constants.enemyActionRange) {
                enemies[i].UpdateEntity();
            }
        }

    }


    #region Public Access Functions

    public BasePlayer GetActivePlayer() { return player.GetActivePlayer(); }

    public BaseEnemy[] GetActiveEnemies() { return enemies; }
    public List<BaseEntity> GetObstacles() { return obstacles; }
    
    #endregion

    #region Public General Functions

    public void UpdateCam(BasePlayer target)
    {
        cam.UpdatePlayer(target);
    }

    public void RemoveObstacle(BaseEntity obstacle) { obstacles.Remove(obstacle); }

    private bool paused;
    public void TogglePause(bool enable, bool isEscape = false)
    {
        if (!pause) return;
        // TODO: Complete this
        if (enable && isEscape) pause.EscapeMenu();

        paused = enable;
        Time.timeScale = paused ? 0 : 1;
    } 

    #endregion
    
    #region Player Warping Functions

    // Exit dungeon here
    public void ExitDungeon(SceneLoader.Scene scene) { SceneLoader.Load(scene); }

    // TODO: Wake the player up at the last save
    public void RespawnPlayer()
    {
        // just resets level for now
        ExitDungeon(SceneLoader.Scene.Rooftops);
    }

    private bool plsWarp;
    private bool hasWarped;
    public void TransportCharacter(Vector2 newPosition)
    {
        if (hasWarped) return;
        GetActivePlayer().MoveObject(newPosition);
        hasWarped = true;
        StartCoroutine(Reset());
    }
    
    IEnumerator Reset()
    {
        yield return new WaitForEndOfFrame();
        hasWarped = false;
    }

    #endregion

}
