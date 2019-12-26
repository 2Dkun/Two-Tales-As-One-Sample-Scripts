using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarkOrb : Enemy {
    // Enemy child specific variables
	public float strikeDist;
	private HitBox strikeBox = new HitBox();
    public int damage;

    // Maybe remove
    private Vector3 oldSpot, newSpot;
	public float amp;
 
	// Use this for initialization
	new void Start () {
		base.Start();

		strikeBox = new HitBox(strikeDist, 0.1f, 0, -0.1f);
        // Orb is constantly attacking
        /*
        Attack a = new Attack();
        a.hitBox = strikeBox;
		curAtk = a;
        Debug.Log(curAtk.hitBox.xPos1);
*/
        base.SetActiveHit(strikeBox);
        curAtk = null;

        // MAYBE REMOVE
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
			case States.Attack:  	Agro(); 	            break;
            case States.Stunned:	base.Stunned();      	break;
			case States.KO:			base.DestroyFoe();		break;
			default: 										break;
		}
	}

	// Handles AI for enemy when player is not detected
	override public void ActIdle() {
		// Player is always detected since it's a boss battle
		ChangeState(States.Grounded);
		timer.resetWait();
	}

	// Handles AI for enemy when player is detected
	override public void Agro() {
        // See if we have come into contact with the player
		HitBox hurt = player.GetComponent<Player>().hurtBox;
		bool isStrike = base.IsHitTarget(strikeBox, gameObject, hurt, player);
		if(isStrike){
            player.SendMessage("Attacked", damage);
		}

		/* ------------------------------ Follow the player ------------------------------ */
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