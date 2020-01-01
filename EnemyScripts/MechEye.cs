using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechEye : Enemy {

    // Enemy child specific variables
	public float strikeDist;
	private HitBox strikeBox = new HitBox();
    private Vector3 oldSpot, newSpot;
	public float amp, wavelen;
	private bool isTurn;
 
	// Use this for initialization
	new void Start () {
		base.Start();

		strikeBox = new HitBox(strikeDist, 0.1f, 0, -0.1f);
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
		if(transform.localPosition.x > origin.x + foe.walkDist/2 && transform.localScale.x < 0){
			isTurn = true;
			transform.Translate(0, Mathf.Sin(Time.frameCount * wavelen) * Time.deltaTime * amp, 0);
			if(timer.WaitForXFrames(Random.Range(55,75))){
				transform.localScale = new Vector2(flipScale, flipScale);
				isTurn = false;
				// Find new spot
				newSpot.x = -(origin.x + foe.walkDist/2);
				newSpot.y = origin.y + Random.Range(-1, 1);	
			}
		}
		else if(transform.localPosition.x < origin.x - foe.walkDist/2 && transform.localScale.x > 0) {
			isTurn = true;
			transform.Translate(0, Mathf.Sin(Time.frameCount * wavelen) * Time.deltaTime * amp, 0);
			if(timer.WaitForXFrames(Random.Range(55,75))){
				transform.localScale = new Vector2(-flipScale, flipScale);
				isTurn = false;
				// Find new spot
				newSpot.x = origin.x + foe.walkDist/2;
				newSpot.y = origin.y + Random.Range(-1, 1);
			}
		}

		// Move to left
		if(transform.localScale.x > 0 && !isTurn){
			transform.Translate(-foe.walkSpd * Time.deltaTime, 
				Mathf.Sin(Time.frameCount * wavelen) * Time.deltaTime * amp, 0);
		}
		// Move to right
		else if(!isTurn){
			transform.Translate(foe.walkSpd * Time.deltaTime, 
				Mathf.Sin(Time.frameCount * wavelen) * Time.deltaTime * amp, 0);
		}

		// Try to detect player
		base.DetectPlayer(true);
	}

	// Handles AI for enemy when player is detected
	override public void Agro() {
		// Stop being agro if player is too far away
		float dist = Vector2.Distance(player.transform.localPosition, transform.localPosition);
		if(dist > foe.detectRad * 2) {
			ChangeState(States.Undected);
		}

		// Back away from player slowly if enemy is in cooldown
		if(cooldown > 0){
			if(timer.WaitForXFrames(cooldown))
				cooldown = 0;
			else {
				// PLAY WALK ANIM
				gameObject.GetComponent<SpriteRenderer>().sprite = foe.foeAnims.walk[0];

				// Move up and away from player
				if(transform.localPosition.x > player.transform.localPosition.x){
					transform.localScale = new Vector2(flipScale, flipScale);
					transform.Translate(foe.dashSpd * Time.deltaTime, foe.walkSpd * Time.deltaTime, 0);
				}
				else {
					transform.localScale = new Vector2(-flipScale, flipScale);
					transform.Translate(-foe.dashSpd * Time.deltaTime, foe.walkSpd * Time.deltaTime, 0);
				}
			}
		}
		// Otherwise go on the offense
		else{
			// See if player is close enough to shoot
			HitBox hurt = player.GetComponent<Player>().hurtBox;
			bool isStrike = base.IsHitTarget(strikeBox, gameObject, hurt, player);
			if(isStrike){
				curAtk = foe.atk[0];
				ChangeState(States.Attack);
			}
			// Dash towards player if not close enough
			else {
				// PLAY DASH ANIMATION
				gameObject.GetComponent<SpriteRenderer>().sprite = foe.foeAnims.dash[0];
				// MOVE HORIZONTALLY TOWARDS PLAYER
				if(transform.localPosition.x > player.transform.localPosition.x){
					transform.localScale = new Vector2(flipScale, flipScale);
					transform.Translate(-foe.dashSpd * Time.deltaTime, 0, 0);
				}
				else {
					transform.localScale = new Vector2(-flipScale, flipScale);
					transform.Translate(foe.dashSpd * Time.deltaTime, 0, 0);
				}
				// MOVE VERTICALLY TOWARDS PLAYER
				if(transform.localPosition.y > player.transform.localPosition.y){
					transform.Translate(0, -foe.dashSpd * Time.deltaTime,  0);
				}
				else if (transform.localPosition.y < player.transform.localPosition.y){
					transform.Translate(0, foe.dashSpd * Time.deltaTime, 0);
				}
			}
		}
		
	}
}
