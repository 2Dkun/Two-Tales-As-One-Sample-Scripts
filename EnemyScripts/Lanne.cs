using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
	PLAN:
	-if orbs are attacked and not in a middle of a move, then go for LUNGE
	-else
		-stand and walk around a certain range from the player
		-once player enters strike zone
			-1) back off until player gets closer?
			-2) move forward a bit? and go for poke attack
			-3) RARE: go for LUNGE
*/

public class Lanne : Enemy {

    // Enemy child specific variables
	private bool orbHurt;
	public float strikeDist, closeDist;
	public int backoff, approach;

	private enum pattern { APPROACH, FADE, POKE, LUNGE, IDLE };
	private pattern curPatt;

    // Use this for initialization
	new void Start () {
		base.Start();

		curPatt = pattern.IDLE;
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
		// Walk away from player if in cooldown
		if(cooldown > 0) {
			if(timer.WaitForXFrames(cooldown))
				cooldown = 0;
			else {
				// PLAY WALK ANIM
				gameObject.GetComponent<SpriteRenderer>().sprite = foe.foeAnims.walk[0];

				if(transform.localPosition.x > player.transform.localPosition.x){
					transform.localScale = new Vector2(flipScale, flipScale);
					transform.Translate(foe.walkSpd * Time.deltaTime, 0, 0);
				}
				else {
					transform.localScale = new Vector2(-flipScale, flipScale);
					transform.Translate(-foe.walkSpd * Time.deltaTime, 0, 0);
				}
			}
		}
		// If orbs are attacked and not in a middle of a move, then go for LUNGE
		else if(orbHurt && curState != States.Attack) {
			orbHurt = false;
			curAtk = foe.atk[0];
			ChangeState(States.Attack);
		}
		// Otherwise ...
		else {
			// See if player is close enough to attack
			float dist = Vector2.Distance(player.transform.localPosition, transform.localPosition);
			if(strikeDist >= dist){
				// Randomly pick an action
				int action = Random.Range(0, 100);
				if(action < 30) 			curPatt = pattern.APPROACH;
				else if(action < 60) 		curPatt = pattern.FADE;
				else if(action < 90) 		curPatt = pattern.POKE;
				else 				 		curPatt = pattern.LUNGE;

				ChangeState(States.Grounded);
				timer.resetWait();
			}
			// Walk to a certain range from the player
			else {
				// PLAY WALK ANIMATION
				gameObject.GetComponent<SpriteRenderer>().sprite = foe.foeAnims.walk[0];

				if(transform.localPosition.x > player.transform.localPosition.x){
					transform.localScale = new Vector2(flipScale, flipScale);
					transform.Translate(-foe.walkSpd * Time.deltaTime, 0, 0);
				}
				else {
					transform.localScale = new Vector2(-flipScale, flipScale);
					transform.Translate(foe.walkSpd * Time.deltaTime, 0, 0);
				}
			}
		}	     
	}

	// Handles AI for enemy when player is detected
	override public void Agro() {
		Debug.Log(curPatt);
		switch(curPatt) {
			case pattern.APPROACH:
				MoveThenPoke(approach, 1);
				break;
			case pattern.FADE:
				MoveThenPoke(backoff, -1);
				break;
			case pattern.POKE:
				curAtk = foe.atk[1];
				ChangeState(States.Attack);
				curPatt = pattern.IDLE;
				break;
			case pattern.LUNGE:
				curAtk = foe.atk[0];
				ChangeState(States.Attack);
				curPatt = pattern.IDLE;
				break;
			default:
				ChangeState(States.Undected);
				timer.resetWait();
				break;
		}
	}

	// Moves the foe in a given direction from player for a given amount of frames then attacks
	private void MoveThenPoke(int moveFrames, int moveDir) {
		// Attack if player is too close
		float dist = Vector2.Distance(player.transform.localPosition, transform.localPosition);
		if(closeDist >= dist){
			timer.resetWait();
			curPatt = pattern.POKE;
		}
		// Otherwise backoff
		else{
			if(timer.WaitForXFrames(moveFrames)){
				curPatt = pattern.POKE;
			}
			else{
				// PLAY WALK ANIM
				gameObject.GetComponent<SpriteRenderer>().sprite = foe.foeAnims.walk[0];

				if(transform.localPosition.x > player.transform.localPosition.x){
					transform.localScale = new Vector2(flipScale, flipScale);
					transform.Translate(foe.walkSpd * Time.deltaTime * moveDir, 0, 0);
				}
				else {
					transform.localScale = new Vector2(-flipScale, flipScale);
					transform.Translate(-foe.walkSpd * Time.deltaTime * moveDir, 0, 0);
				}
			}
		}
	}

    /* 
		Functions to be called by other scripts 
	*/
    public void OrbAttacked() {
        // Get lanne to go for LUNGE attack
		orbHurt = true;
    }
}
