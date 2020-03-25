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
    public Canvas canvas;

    // Entity variables
    public MainPlayer player;
    public Spirit spirit;
    private BaseEnemy[] enemies;

    private bool pauseFoe, pausePlayer;

    // Ground variables
    public GameObject ground;
    private GameObject[] groundPoints = new GameObject[0];

    // Start is called before the first frame update
    void Awake() {
        Application.targetFrameRate = 60;
        // Init entity variables
        if (!player) player = FindObjectOfType<MainPlayer>();
        if (!spirit) spirit = FindObjectOfType<Spirit>();
        enemies = FindObjectsOfType<BaseEnemy>();
        // Init player UI variables
        if (!curCam) curCam = FindObjectOfType<CameraManager>();
        if (!playerHUD) playerHUD = FindObjectOfType<HUDManager>();
        curCam.player = player.gameObject;
        if (!canvas) canvas = FindObjectOfType<Canvas>();

        /* Calculate the ground */
        // Treat all children of ground as ground points
        if (!ground) ground = gameObject;
        groundPoints = new GameObject[ground.transform.childCount];
        for (int i = 0; i < groundPoints.Length; i++) {
            groundPoints[i] = ground.transform.GetChild(i).gameObject;
        }
        // Sort children
        System.Array.Sort(groundPoints, GroundCompare);
    }

    // Update is called once per frame
    void Update() {
        for (int i = 0; i < enemies.Length && !pauseFoe; i++) {
            if (enemies[i]) {
                enemies[i].ActFreely();
                KeepInBounds(enemies[i], ref enemies[i].prevPos);
            }
        }

        if (!pausePlayer) {
            player.ActFreely();
            spirit.ActFreely();
            KeepInBounds(player, ref player.prevPos);
            KeepInBounds(spirit, ref spirit.prevPos);
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
    public Canvas GetCanvas() { return canvas; }

    // Pause functions
    public void PausePlayers(bool isPause) { pausePlayer = isPause; }
    public void PauseEnemies(bool isPause) { pauseFoe = isPause; }

    // Update camera position
    public void UpdateCamera(GameObject player) {
        curCam.player = player;
    }

    #region Ground Functions
    // Function used to sort groundPoints by their x position
    private int GroundCompare(GameObject a, GameObject b) {
        if (a == null) return (b == null) ? 0 : -1;
        if (b == null) return 1;

        float xa = a.transform.position.x;
        float xb = b.transform.position.x;
        return xa.CompareTo(xb);
    }

    // Make sure object is in bounds
    private void KeepInBounds(Entity obj, ref Vector3 prevPos) {
        Vector3 curPos = obj.transform.position;
        // Find the two points the object is between
        for (int i = 0; i < groundPoints.Length - 1; i++) {
            Vector3 pointA = groundPoints[i].transform.position;
            Vector3 pointB = groundPoints[i + 1].transform.position;
            if (pointA.x < curPos.x) {
                if (pointB.x >= curPos.x) {
                    prevPos.y = obj.transform.position.y;
                    // Moved left
                    if (prevPos.x > curPos.x && pointA.y > curPos.y) {
                        obj.transform.position = prevPos;
                    }
                    // Moved right
                    else if (prevPos.x < curPos.x && pointB.y > curPos.y) {
                        obj.transform.position = prevPos;
                    }
                    else {
                        float newGround = Mathf.Max(pointA.y, pointB.y);
                        obj.UpdateGround(newGround);
                    }
                }
            }
        }
        prevPos = obj.transform.position;
    }
    #endregion
}
