using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonManager : MonoBehaviour {

	public GameObject player;
	public GameObject[] enemies;
	private FrameCounter frameCounter;

	// Use this for initialization
	void Start () {
		enemies[0].SendMessage("Start");
	}
	
	// Update is called once per frame
	void Update () {
		if(enemies[0])
			enemies[0].SendMessage("ActFree");

		// Check if enemy has detected player then check if attacked
			// If attacked: apply hitstun and knockback to player

		// If player comes into contact with enemy, they take minor damage and knockback



		// Check if player is attacking and then see if it hit any of the hurtboxes.
			// If landed: make opponent blink but no hitstun



		// Else let all object act freely
		
		/* 
		player.GetComponent<Player2>().FreeAct();

		if (Input.GetKeyDown(KeyCode.P))
			player.GetComponent<Player2>().Attacked(1);
			*/

	}
}
