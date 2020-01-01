using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxePhantom : Enemy {

	// Enemy child specific variables
	public float strikeDist;
	private HitBox strikeBox = new HitBox();
	private bool isTurn;
 
	// Use this for initialization
	new void Start () {
		base.Start();
		strikeBox = new HitBox(strikeDist, 1, 0, -1);
	}
	
	// Update is called once per frame
	void Update () {
	//	ActFree();
	//	Debug.Log(curState + " " + prevState);
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
			case States.Airborne:    						break;
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
			if(timer.WaitForXFrames(Random.Range(55,75))){
				transform.localScale = new Vector2(flipScale, flipScale);
				isTurn = false;
			}
		}
		else if(transform.localPosition.x < origin.x - foe.walkDist/2 && transform.localScale.x > 0) {
			isTurn = true;
			if(timer.WaitForXFrames(Random.Range(55,75))){
				transform.localScale = new Vector2(-flipScale, flipScale);
				isTurn = false;
			}
		}


		// Walk to left
		if(transform.localScale.x > 0 && !isTurn){
			transform.Translate(-foe.walkSpd * Time.deltaTime, 0, 0);
		}
		// Walk to right
		else if (!isTurn){
			transform.Translate(foe.walkSpd * Time.deltaTime, 0, 0);
		}

		// Try to detect player
		base.DetectPlayer(false);
	}

	// Handles AI for enemy when player is detected
	override public void Agro() {
		
		// Stop being agro if player is too far away
		float dist = Vector2.Distance(player.transform.localPosition, transform.localPosition);
		if(dist > foe.detectRad * 2) {
			ChangeState(States.Undected);
			timer.resetWait();
		}

		// Back away from player slowly if enemy is in cooldown
		if(cooldown > 0){
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
		// Otherwise go on the offense
		else{
			// See if player is close enough to swing
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

				if(transform.localPosition.x > player.transform.localPosition.x){
					transform.localScale = new Vector2(flipScale, flipScale);
					transform.Translate(-foe.dashSpd * Time.deltaTime, 0, 0);
				}
				else {
					transform.localScale = new Vector2(-flipScale, flipScale);
					transform.Translate(foe.dashSpd * Time.deltaTime, 0, 0);
				}
			}
		}
		
	}
/* 
	public void GetHurtBox(HitBox hurt){
		base.GetHurtBox(hurt);
	}
*/
}
