using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxePhantom : Enemy {

	// Enemy child specific variables
	public float strikeDist;
	private HitBox strikeBox = new HitBox();
 
	// Use this for initialization
	new void Start () {
		base.Start();
		strikeBox = new HitBox(strikeDist, 1, 0, -1);
	}
	
	// Update is called once per frame
	void Update () {
	//	ActFree();
		Debug.Log(curState + " " + prevState);
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
		if(enemy.transform.localPosition.x > origin.x + foe.walkDist/2){
			if(timer.WaitForXFrames(Random.Range(55,75))){
				enemy.transform.localScale = new Vector2(flipScale, flipScale);
				enemy.transform.localPosition = 
					new Vector2(origin.x + foe.walkDist/2, enemy.transform.localPosition.y);
			}
		}
		else if(enemy.transform.localPosition.x < -(origin.x + foe.walkDist/2)) {
			if(timer.WaitForXFrames(Random.Range(55,75))){
				enemy.transform.localScale = new Vector2(-flipScale, flipScale);
				enemy.transform.localPosition = 
					new Vector2(-(origin.x + foe.walkDist/2), enemy.transform.localPosition.y);
			}
		}

		// Walk to left
		else if(enemy.transform.localScale.x > 0){
			enemy.transform.Translate(-foe.walkSpd * Time.deltaTime, 0, 0);
		}
		// Walk to right
		else {
			enemy.transform.Translate(foe.walkSpd * Time.deltaTime, 0, 0);
		}

		// Try to detect player
		base.DetectPlayer();
	}

	// Handles AI for enemy when player is detected
	override public void Agro() {
		
		// Back away from player slowly if enemy is in cooldown
		if(cooldown > 0){
			if(timer.WaitForXFrames(cooldown))
				cooldown = 0;
			else {
				// PLAY WALK ANIM
				enemy.GetComponent<SpriteRenderer>().sprite = foe.foeAnims.walk[0];

				if(enemy.transform.localPosition.x > player.transform.localPosition.x){
					enemy.transform.localScale = new Vector2(flipScale, flipScale);
					enemy.transform.Translate(foe.walkSpd * Time.deltaTime, 0, 0);
				}
				else {
					enemy.transform.localScale = new Vector2(-flipScale, flipScale);
					enemy.transform.Translate(-foe.walkSpd * Time.deltaTime, 0, 0);
				}
			}
		}
		// Otherwise go on the offense
		else{
			// See if player is close enough to swing
			HitBox hurt = player.GetComponent<Player>().hurtBox;
			bool isStrike = base.IsHitTarget(strikeBox, enemy, hurt, player);
			if(isStrike){
				curAtk = foe.atk[0];
				ChangeState(States.Attack);
			}
			// Dash towards player if not close enough
			else {
				// PLAY DASH ANIMATION
				enemy.GetComponent<SpriteRenderer>().sprite = foe.foeAnims.dash[0];

				if(enemy.transform.localPosition.x > player.transform.localPosition.x){
					enemy.transform.localScale = new Vector2(flipScale, flipScale);
					enemy.transform.Translate(-foe.dashSpd * Time.deltaTime, 0, 0);
				}
				else {
					enemy.transform.localScale = new Vector2(-flipScale, flipScale);
					enemy.transform.Translate(foe.dashSpd * Time.deltaTime, 0, 0);
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
