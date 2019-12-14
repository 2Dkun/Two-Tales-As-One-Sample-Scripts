using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy: MonoBehaviour {
 
	[System.Serializable]
    public class Anims {
		public Sprite[] idle, walk, dash, stunned;
	}

    [System.Serializable]
	public class FoeClass {
		public Anims foeAnims;
		public float walkSpd, dashSpd, walkDist;
		public float airAccel, airSpd, jumpHeight, gravity;
		public float detectRad;
		public Attack[] atk;
	}

    // Base enemy data
    public int maxHP;
	public FoeClass foe;

    // Enemy states
	public enum States { Undected, Grounded, Airborne, Attack, Stunned, KO };
	public States curState 			{ get; set; }
	public States prevState 		{ get; set; }

    // Other data
    public GameObject enemy;
	public HitBox hurtbox;
    public int curHP				{ get; set; }
	public Vector2 origin			{ get; set; }
    public FrameCounter timer 		{ get; set; }
	public FrameCounter subTimer 	{ get; set; }
	public float xVel 				{ get; set; }
	public float yVel 				{ get; set; }
	public float flipScale 			{ get; set; }
	public int cooldown 			{ get; set; }
	public Attack curAtk			{ get; set; }
	public bool isHurt				{ get; set; }
	private HitBox fov = new HitBox();
	private HitBox activeHit;
	private bool hitPlayer;

	// Temp data?
	public GameObject player;

    // Initialize variables
    public void Start() {
		timer = new FrameCounter();

		curHP = this.maxHP;
		curState = States.Undected;
		prevState = States.Undected;
		flipScale = Mathf.Abs(enemy.transform.localScale.x);
		origin = enemy.transform.localPosition;
		timer = new FrameCounter();
		subTimer = new FrameCounter();

		// Set htibox for detection
		fov = new HitBox(foe.detectRad, 1, -foe.detectRad, -1);
		hitPlayer = false;
	}

	/* 
		Functions to be implemented by children 
	*/

    // Handles AI for enemy when player is not detected
    public abstract void ActIdle();
	// Handles AI for enemy when player is detected
	public abstract void Agro();

	/* 
		Functions to be used by all children 
	*/

    // Handles the enemy's state during hitstun
	public void ApplyHitstun(){
		// Return the previous state once out of hitstun
		if(subTimer.WaitForXFrames(25)){ // 25 is arbitrary 
			isHurt = false;
		}
		// Otherwise play hitstun animation
		else{
			if(subTimer.curFrame() % 3 == 1){
				enemy.GetComponent<SpriteRenderer>().color = new Color(1,0,0);
			} else{
				enemy.GetComponent<SpriteRenderer>().color = new Color(1,1,1);
			}
		}
	}

    // Handles the enemy's state during hitstun
	public void Stunned(){
		// Return the previous state once out of hitstun
		if(timer.WaitForXFrames(25)){ // 25 is arbitrary 
			ChangeState(prevState);
		}
		// Otherwise play stunned animation
		else{
			if(timer.curFrame() % 3 == 1){
				enemy.GetComponent<SpriteRenderer>().color = new Color(1,1,0);
			} else{
				enemy.GetComponent<SpriteRenderer>().color = new Color(1,1,1);
			}
		}
	}

    // Remove the enemy gameObj upon death
    public void DestroyFoe() {
        // Fade GameObject to nothing
		float curAlpha = enemy.GetComponent<SpriteRenderer>().color.a - Time.deltaTime;
		enemy.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, curAlpha);
        if (curAlpha < 0) {
            curAlpha = 0;
			Destroy(enemy);
        }
        
       
    }

	// See if the player is nearby 
	public void DetectPlayer() {
		bool isHit = IsHitTarget(fov, enemy, player.GetComponent<Player>().hurtBox, player);

		if(isHit){
			ChangeState(States.Grounded);
			timer.resetWait();
		}
	}

	// Check if given hitbox connected with player hurtbox with adjusted positions
	public bool IsHitTarget(HitBox userHit, GameObject user, HitBox targetHurt, GameObject target) {
		// Shift user hitbox to position
		int flipScale = (int) (user.transform.localScale.x / (Mathf.Abs(user.transform.localScale.x))); 
		userHit.flipBox(-flipScale);
		userHit.shiftBox(user.transform.localPosition.x, user.transform.localPosition.y);

		// Shift target hurtbox to position
		flipScale = (int) (target.transform.localScale.x / (Mathf.Abs(target.transform.localScale.x))); 
		targetHurt.flipBox(flipScale);
		targetHurt.shiftBox(target.transform.localPosition.x, target.transform.localPosition.y);

		// See if the hitbox came inot contact with hurtbox
		return userHit.checkHit(targetHurt);
	}

	// Perform the given attack
	public void Attack(Attack a) {
		// Play attack anim
		enemy.GetComponent<SpriteRenderer>().sprite = a.anim.PlayAnim();

		// See if the move has ended
		if(timer.WaitForXFrames(a.endlag)){ 
			ChangeState(States.Grounded);
			cooldown = a.cooldown;
			a.anim.ResetAnim();
			hitPlayer = false;
		}
		// Otherwise check if the move connected during its active frames
		else if(timer.curFrame() >= a.startup && timer.curFrame() <= a.getLastFrame() && !hitPlayer){
			bool isHit = IsHitTarget(a.hitBox, enemy, player.GetComponent<Player>().hurtBox, player);
			if(isHit){
				player.SendMessage("Attacked", a.power);
				hitPlayer = true;
			}
			activeHit = a.hitBox;
		}
		else if(timer.curFrame() == 0 || timer.curFrame() == a.getLastFrame()+1){
			activeHit = null;
		}

	}

    // Change enemy's current state and store it
	public void ChangeState(States s){
		prevState = curState;
		curState = s;
	}

	/* 
		Functions to be called by other scripts 
	*/

    // Apply hurt process if player got hit by an attack
    public void Attacked(int damage) {
        //if(curState != States.Hurt){
		if(!isHurt && curState != States.KO){
			if(damage == Constants.PARRY_KO) {
				if(curState == States.Stunned)
					damage = Constants.INSTANT_KO;
				else
					damage = 0;
			}

			if(curState == States.Stunned)
				damage *= 3;
			curHP -= damage;
			if(curHP < 0)	curHP = 0;

			isHurt = true;
			subTimer.resetWait();
		}
    }

	// Put enemy into parried state
	public void Parried() {
		curAtk.anim.ResetAnim();
		cooldown = curAtk.cooldown;
		activeHit = null;
		timer.resetWait();
		//if()
		curState = States.Grounded;
		ChangeState(States.Stunned);
	}

	// Copy the hitbox of the enemy's current attack
	public void GetCurAtk(HitBox h) {
		if(activeHit != null)
			h.Clone(activeHit);
	}

	// Copy the enemy's hurtbox
	public void GetHurtBox(HitBox foeHurt) {
		foeHurt.Clone(hurtbox);
	}


	/*
		Getter/Setter functions
	 */
}
