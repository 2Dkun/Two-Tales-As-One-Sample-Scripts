using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonRoom : MonoBehaviour
{

    private DungeonManager dungeon;
    public bool isExit;
    public SceneLoader.Scene exitScene;
    public DungeonRoom exitRoom;
    
    // Start is called before the first frame update
    void Start()
    {
        dungeon = FindObjectOfType<DungeonManager>();
    }
    
    // Hurt the player if they came into contact with the enemy
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (isExit) dungeon.ExitDungeon(exitScene);
            else if (PlayerInput.up.isPressed && dungeon.GetActivePlayer().IsIdle())
            {
                dungeon.TransportCharacter(exitRoom.transform.position);
                print(gameObject.name);
            }
        }
    }

}
