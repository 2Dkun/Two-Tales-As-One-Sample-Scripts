using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lanne : Enemy {

    // Enemy child specific variables

    // Use this for initialization
	new void Start () {
		base.Start();
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


        /* DO THIS ONCE DARK ORB DIES
		// Player is always detected since it's a boss battle
		ChangeState(States.Grounded);
		timer.resetWait();
        */
	}

	// Handles AI for enemy when player is detected
	override public void Agro() {
		
	}

    /* 
		Functions to be called by other scripts 
	*/
    public void OrbAttacked() {
        // Get lanne to go for thrust attack
    }
}
