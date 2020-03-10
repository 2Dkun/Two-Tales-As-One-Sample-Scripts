using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonHandler : MonoBehaviour {

    /*
     *  CHANGE player SCRIPT TO Player LATER
     *  YOU HAVE TO MAKE THIS CHANGE OR IT'LL LOOK BAD
     */

    // Display variables
    public CameraManager curCam;
    public HUDManager playerHUD;

    // Entity variables
    public MainPlayer player;
    public Spirit spirit;
    private BaseEnemy[] enemies;

    private bool pauseFoe, pausePlayer;

    // Start is called before the first frame update
    void Awake() {
        if (!player) player = FindObjectOfType<MainPlayer>();
        if (!spirit) spirit = FindObjectOfType<Spirit>();
        enemies = FindObjectsOfType<BaseEnemy>();

        if (!curCam) curCam = FindObjectOfType<CameraManager>();
        if (!playerHUD) playerHUD = FindObjectOfType<HUDManager>();

        curCam.player = player.gameObject;
    }

    // Update is called once per frame
    void Update() {
        for (int i = 0; i < enemies.Length && !pauseFoe; i++) {
            if(enemies[i]) enemies[i].ActFreely();
        }

        if (!pausePlayer) {
            player.ActFreely();
            spirit.ActFreely();
        }
    }

    // Access functions for players
    public MainPlayer GetMainPlayer()   { return player; }
    public Spirit GetSpiritPlayer()     { return spirit; }
    public Entity[] GetActiveEnemies()  { return enemies; }
    public HUDManager GetHUDManager()   { return playerHUD; }     

    // Access functions for enemies
    public BasePlayer GetPlayer() {
        return player.GetComponent<BasePlayer>();
    }
    public Entity[] GetActivePlayers() {
        Entity[] e = { player, spirit };
        return e;
    }

    // Pause functions
    public void PausePlayers(bool isPause) { pausePlayer = isPause; }
    public void PauseEnemies(bool isPause) { pauseFoe = isPause; }

    // Update camera position
    public void UpdateCamera(GameObject player) {
        curCam.player = player;
    }
}
