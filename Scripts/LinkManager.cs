using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkManager : MonoBehaviour {
    
    /*
     * Spirit Link Notes:
     * - hold data of attack 
     * - must be activated on command(public function)
     * - let playermanager know when the attack is done
     *
     * General Notes:
     * - have preinstantiated objects for the current amount of links possible
     *     - hide and show these links when necessary
     * - figure out how to create the animation for the spirit links
     */

    public Link[] links;
    protected DungeonManager dungeon;
    
    //
    public void CreateLink(Vector3 linkPosition, Attack linkAttack, LinkUser linkUser) {
        
    }

    public void ActivateNextLink() {
        
    }

    public bool IsLinkActive() {
        return !links[0].IsCompleted();
    }

    public void Testy(Attack a, Vector3 linkPosition, float direction)
    {
        if (!links[0].IsCompleted()) return;
        links[0].transform.localPosition = linkPosition;
        links[0].transform.localScale = new Vector3(direction, 1, 1);
        links[0].SetAttack(a, LinkUser.Sword);
        links[0].ActivateLink();
    }

    private void Start() {
        dungeon = FindObjectOfType<DungeonManager>();
    }

    private void Update() {
        /*
        // TESTING PURPOSES
        if (Input.GetKeyDown(KeyCode.H)) {
            links[0].ActivateLink();
        }
        */
        //links[0].UpdateLink(dungeon.GetActiveEnemies());
    }
}
