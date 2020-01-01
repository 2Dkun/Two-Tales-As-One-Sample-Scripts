using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonManager : MonoBehaviour {

	public GameObject player;
	public GameObject[] enemies;
	private FrameCounter frameCounter;

	public GameObject ground;
	private GameObject[] groundPoints;
	private Vector3 prevPlayPos;
	private Vector3[] prevFoePos;

	void Awake() {
		Application.targetFrameRate = 60;
	}

	// Use this for initialization
	void Start () {
		prevFoePos = new Vector3[enemies.Length];
		for(int i = 0; i < enemies.Length; i++){
			enemies[i].SendMessage("Start");
			prevFoePos[i] = enemies[i].transform.localPosition;
		}

		// Treat all children of ground as ground points
		groundPoints = new GameObject[ground.transform.childCount];
		for(int i = 0; i < groundPoints.Length; i++) {
			groundPoints[i] = ground.transform.GetChild(i).gameObject;
		}
		// Sort children
		System.Array.Sort(groundPoints, GroundCompare);
	}
	
	// Update is called once per frame
	void Update () {
		// Allow all entities to act
		for(int i = 0; i < enemies.Length; i++){
			if(enemies[i]){
				Vector3 playerPos = player.transform.localPosition;
				Vector3 enemyPos = enemies[i].transform.localPosition;
				float dist = Vector3.Distance(playerPos, enemyPos);
				if(dist <= Constants.AWAKE_DIST) {
					enemies[i].SendMessage("ActFree");
					KeepInBounds(enemies[i], ref prevFoePos[i]);
				}
			}
		}
		player.GetComponent<Player>().ControlPlayer();
		KeepInBounds(player, ref prevPlayPos);
		


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

	// Make sure object is in bounds
	private void KeepInBounds(GameObject obj, ref Vector3 prevPos) {
		Vector3 curPos = obj.transform.position;
		// Find the two points the object is between
		for(int i = 0; i < groundPoints.Length-1; i++){
			Vector3 pointA = groundPoints[i].transform.position;
			Vector3 pointB = groundPoints[i+1].transform.position;
			if(pointA.x < curPos.x){
				if(pointB.x >= curPos.x){
					prevPos.y = obj.transform.position.y;
					// Moved left
					if(prevPos.x > curPos.x && pointA.y > curPos.y){
						obj.transform.position = prevPos;
					}
					// Moved right
					else if(prevPos.x < curPos.x && pointB.y > curPos.y){
						obj.transform.position = prevPos;
					}
					else if(obj == player){
						float newGround = Mathf.Max(pointA.y, pointB.y);
						player.GetComponent<Player>().UpdateGround(newGround);
					}
				}
			}
		}
		prevPos = obj.transform.position;
	}

}
