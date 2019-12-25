using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechEye : Enemy {

    // Enemy child specific variables
    private Vector3 oldSpot, newSpot;
	public float amp;
 
	// Use this for initialization
	new void Start () {
		base.Start();

		newSpot.x = -(origin.x + foe.walkDist/2);
		newSpot.y = origin.y + Random.Range(-1, 1);
	}

	// Allow the enemy to act freely
	private void ActFree() {
		if(curHP == 0) {
			curHP = -1; // Don't go into this if statement again
			timer.resetWait();
			ChangeState(States.KO);
		}
		if(isHurt)		base.ApplyHitstun();

		switch(curState){
            case States.Undected:	ActIdle();      		break;
			case States.Grounded:	Agro();        			break;
			case States.Airborne:   Agro();					break;
			case States.Attack:  	base.Attack(curAtk); 	break;
            case States.Stunned:	base.Stunned();      	break;
			case States.KO:			base.DestroyFoe();		break;
			default: 										break;
		}
	}

	// Handles AI for enemy when player is not detected
	override public void ActIdle() {
		// Wait a moment before turning around
		if(transform.localPosition.x > origin.x + foe.walkDist/2){
			transform.Translate(0, Mathf.Sin(transform.localPosition.x) * Time.deltaTime / amp, 0);
			if(timer.WaitForXFrames(Random.Range(55,75))){
				transform.localScale = new Vector2(flipScale, flipScale);
				transform.localPosition = 
					new Vector2(origin.x + foe.walkDist/2, transform.localPosition.y);
				// Find new spot
				newSpot.x = -(origin.x + foe.walkDist/2);
				newSpot.y = origin.y + Random.Range(-1, 1);	
			}
		}
		else if(transform.localPosition.x < -(origin.x + foe.walkDist/2)) {
			transform.Translate(0, Mathf.Sin(transform.localPosition.x) * Time.deltaTime / amp, 0);
			if(timer.WaitForXFrames(Random.Range(55,75))){
				transform.localScale = new Vector2(-flipScale, flipScale);
				transform.localPosition = 
					new Vector2(-(origin.x + foe.walkDist/2), transform.localPosition.y);
				// Find new spot
				newSpot.x = origin.x + foe.walkDist/2;
				newSpot.y = origin.y + Random.Range(-1, 1);
			}
		}

		// Move to left
		else if(transform.localScale.x > 0){
			transform.Translate(-foe.walkSpd * Time.deltaTime, 
				Mathf.Sin(transform.localPosition.x) * Time.deltaTime / amp, 0);
		}
		// Move to right
		else {
			transform.Translate(foe.walkSpd * Time.deltaTime, 
				Mathf.Sin(transform.localPosition.x) * Time.deltaTime / amp, 0);
		}

		// Try to detect player
		base.DetectPlayer(true);
	}

	// Handles AI for enemy when player is detected
	override public void Agro() {
		
		
	}
}
