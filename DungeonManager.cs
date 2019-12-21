using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonManager : MonoBehaviour {

	public GameObject player;
	public GameObject[] enemies;
	private FrameCounter frameCounter;

	public GameObject[] groundPoints;
	private Vector3 playerPos;

	// Use this for initialization
	void Start () {
		for(int i = 0; i < enemies.Length; i++){
			enemies[i].SendMessage("Start");
		}

		System.Array.Sort(groundPoints, GroundCompare);
		for(int i = 0; i < 3; i++){
		//	Debug.Log(groundPoints[i].transform.position.x);
		}
	}
	
	// Update is called once per frame
	void Update () {
		// Allow all entities to act
		for(int i = 0; i < enemies.Length; i++){
			if(enemies[i])
				enemies[i].SendMessage("ActFree");
		}
		player.GetComponent<Player>().ControlPlayer();

		// Make sure player is in bounds
		Vector3 curPos = player.transform.position;
		if(playerPos != curPos){
			// Find the two points the player is between
			for(int i = 0; i < groundPoints.Length-1; i++){
				Vector3 pointA = groundPoints[i].transform.position;
				Vector3 pointB = groundPoints[i+1].transform.position;
				if(pointA.x < curPos.x){
					if(pointB.x >= curPos.x){
						playerPos.y = player.transform.position.y;
						// Moved left
						if(playerPos.x > curPos.x && pointA.y > curPos.y){
							player.transform.position = playerPos;
						}
						// Moved right
						else if(playerPos.x < curPos.x && pointB.y > curPos.y){
							player.transform.position = playerPos;
						}
						else{
							float newGround = Mathf.Max(pointA.y, pointB.y);
							player.GetComponent<Player>().UpdateGround(newGround);
						}
					}
				}
			}
			playerPos = player.transform.position;
		}



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

	// Function used to sort groundPoints by their x position
	private int GroundCompare(GameObject a, GameObject b) {
        if (a == null) return (b == null) ? 0 : -1;
        if (b == null) return 1;

		float xa = a.transform.position.x;
        float xb = b.transform.position.x;
        return xa.CompareTo(xb);
	}
}
