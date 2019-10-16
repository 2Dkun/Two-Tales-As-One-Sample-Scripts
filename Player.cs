using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
 
	/*
		TO DO LIST:
		- Setup up enemy scripts using the child thing
			- Think about how it can work when you need to call its method
			- https://en.wikipedia.org/wiki/Template_method_pattern
			- https://answers.unity.com/questions/1266855/access-subclass-through-base.html
			- YO: https://answers.unity.com/questions/471551/how-can-i-access-a-function-without-knowing-the-sc.html 
		- Improve parry
		- Improve on enemy AI
		- Work on animations

		- IMPLEMENT AN MP SYSTEM FOR SKILLS.
		- FIGURE OUT HOW YOU'RE GONNA ALLOW SKILLS TO BE USED
			- What you have so far:
				- Enemy will treat damage of -1 as parry kill
				- Skills are defined for each char. You must figure out how to organize them
				- Figure out how to let Duke use his first skill and give it an mp cost.
	 */

	[System.Serializable]
    public class Anims {
		public Sprite[] idle, crouch, walk, dash, jump, swap;
	}

	[System.Serializable]
	public class PlayerClass {
		public Anims charAnims;
		public float walkSpd, dashSpd, airSpd, jumpHeight, gravity;
		public float airAccel;
		public Attack atk, cAtk;
		public Attack[] skills;
	}

	// Base character data
    public int maxHP = 100;    // just a default value
	public PlayerClass sword, shield;
 
	// Character states
	private enum States { Grounded, Airborne, Swap, Attack, Parry, Hurt };
	private States curState { get; set; }
	private States prevState { get; set; }

	// Other character data
	public GameObject player;
	public HitBox hurtBox;
	private PlayerClass curClass;
	private int curHP;
	private float xVel, yVel;
	private float flipScale;
	private Attack curAttack;
	private FrameCounter timer;

	// TEMP VARS
	public float minHeight; // detect ground better in future
	public GameObject[] foes; // place this info in dungeon manager and recieve it from there

	// Initialize variables
    public void Start() {
		timer = new FrameCounter();

		curHP = this.maxHP;
		curClass = this.sword;
		curState = States.Grounded;
		prevState = States.Grounded;
		flipScale = Mathf.Abs(player.transform.localScale.x);
	}

	// TESTING PURPOSES REMOVE ME LATER
	public void Update(){
		ControlPlayer();
	}

	// Allow the user to have full control over the player
	public void ControlPlayer(){
		switch(curState){
			case States.Grounded:	MoveGrounded(); break;
			case States.Airborne: 	MoveAirborne(); break;
			case States.Swap:		Swap(); 		break;
			case States.Attack:  	Attack();		break; // Play animation in this actual method please
			case States.Parry: 		Attack();		break;
			case States.Hurt:		ApplyHitstun(); break;
			default: 								break;
		}
	}

	// Perform current attack
	private bool Attack() {
		// Play attack animation
		player.GetComponent<SpriteRenderer>().sprite = curAttack.anim.PlayAnim();
		
		// See if the move has ended
		if(timer.WaitForXFrames(curAttack.endlag)){ //endlag
			if(prevState == States.Airborne)
				player.GetComponent<SpriteRenderer>().sprite = curClass.charAnims.jump[0];

			curAttack.anim.ResetAnim();
			ChangeState(prevState);
			return true; //Attack is completed
		}
		// Otherwise check if the move connected during its active frames
		else if(timer.curFrame() >= curAttack.startup && timer.curFrame() <= curAttack.getLastFrame()){ 
			// Check if attack connected with any enemy
			for(int i = 0; i < foes.Length; i++){
				HitBox foeHurt = new HitBox();
				foes[i].SendMessage("GetHurtBox", foeHurt);
				bool isHit = IsHitTarget(curAttack.hitBox, player, foeHurt, foes[i]);

				// Tell the enemy that it has been attacked
				if(isHit){
					foes[i].SendMessage("Attacked", curAttack.power);
				}
			}
				
			// See if an attack was blocked if the player's attack is a parry
			if(curAttack.isParry){
				for(int i = 0; i < foes.Length; i++){
					HitBox foeHit = new HitBox(); 
					foes[i].SendMessage("GetCurAtk", foeHit);
					if(!foeHit.IsEqual(new HitBox())){
						bool isParry = IsHitTarget(curAttack.hitBox, player, foeHit, foes[i]);

						if(isParry){
							curState = States.Parry;
							foes[i].SendMessage("Parried");
							Debug.Log("yote");
						}
					}
				}
			}

		}
		else if(curState == States.Parry) {
			curState = States.Attack;
		}

			
		// Allow for movement if the player is airborne
		if(prevState == States.Airborne){
			if(Input.GetKey(KeyCode.A)) {
				this.xVel += -curClass.airAccel * Time.deltaTime * 30;
				if(xVel < -curClass.airSpd)
					xVel = -curClass.airSpd;
			}
			else if(Input.GetKey(KeyCode.D)) {
				this.xVel += curClass.airAccel * Time.deltaTime * 30;
				if(xVel > curClass.airSpd)
					xVel = curClass.airSpd;
			}
			ApplyGravity();

			// Change to grounded state if player lands
			if(player.transform.localPosition.y <= minHeight){
				player.transform.localPosition = new Vector2(player.transform.localPosition.x, minHeight);
				ChangeState(States.Grounded);
				timer.resetWait();
				xVel = 0;
			}
		}
		return false; //Attack is still going on
	}

	// Handles the player's air movement
	private void MoveAirborne() {
		// SWAP
		if (Input.GetKeyDown(KeyCode.W)){
			ChangeState(States.Swap);
			player.GetComponent<SpriteRenderer>().sprite = curClass.charAnims.swap[0];
			timer.resetWait();
		}

		// MOVE LEFT
		if(Input.GetKey(KeyCode.A)) {
			this.xVel += -curClass.airAccel * Time.deltaTime * 30;
			if(xVel < -curClass.airSpd)
				xVel = -curClass.airSpd;
			player.transform.localScale = new Vector2(-flipScale, flipScale);
		}
		// MOVE RIGHT
		else if(Input.GetKey(KeyCode.D)) {
			this.xVel += curClass.airAccel * Time.deltaTime * 30;
			if(xVel > curClass.airSpd)
				xVel = curClass.airSpd;
			player.transform.localScale = new Vector2(flipScale, flipScale);
		}

		// ATTACK
		if(Input.GetKeyDown(KeyCode.J)) {
			ChangeState(States.Attack);
			curAttack = curClass.atk;
		}
		ApplyGravity();

		// Check if player has landed
		if(player.transform.localPosition.y <= minHeight){
            player.transform.localPosition = new Vector2(player.transform.localPosition.x, minHeight);
			ChangeState(States.Grounded);
			xVel = 0;
        }
	}

	// Handles the player's grounded movement
	private void MoveGrounded() {

		// SWAP
		if (Input.GetKeyDown(KeyCode.W)){
			ChangeState(States.Swap);
			player.GetComponent<SpriteRenderer>().sprite = curClass.charAnims.swap[0];
			timer.resetWait();
		}

		// ATTACK
		else if(Input.GetKeyDown(KeyCode.J)) {
			ChangeState(States.Attack);

			// Decide if the attack was a normal attack or low
			if(Input.GetKey(KeyCode.S)) 	curAttack = curClass.cAtk;
			else 							curAttack = curClass.atk;

			// Change direction of attack based on input
			if(Input.GetKey(KeyCode.A)){
				player.transform.localScale = new Vector2(-flipScale, flipScale);
			}
			else if(Input.GetKey(KeyCode.D)){
				player.transform.localScale = new Vector2(flipScale, flipScale);
			}

			timer.resetWait();
		}

		// MOVE LEFT
		else if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.LeftShift)) {
			player.GetComponent<SpriteRenderer>().sprite = curClass.charAnims.walk[0];
			player.transform.localScale = new Vector2(-flipScale, flipScale);
			player.transform.Translate(-1 * curClass.walkSpd * Time.deltaTime, 0, 0);
		}
		else if(Input.GetKey(KeyCode.A)) {
			player.GetComponent<SpriteRenderer>().sprite = curClass.charAnims.dash[0];
			player.transform.localScale = new Vector2(-flipScale, flipScale);
			player.transform.Translate(-1 * curClass.dashSpd * Time.deltaTime, 0, 0);
		}

		// MOVE RIGHT
		else if (Input.GetKey(KeyCode.D) && Input.GetKey(KeyCode.LeftShift)) {
			player.GetComponent<SpriteRenderer>().sprite = curClass.charAnims.walk[0];
			player.transform.localScale = new Vector2(flipScale, flipScale);
			player.transform.Translate(curClass.walkSpd * Time.deltaTime, 0, 0);
		}
		else if(Input.GetKey(KeyCode.D)) {
			player.GetComponent<SpriteRenderer>().sprite = curClass.charAnims.dash[0];
			player.transform.localScale = new Vector2(flipScale, flipScale);
			player.transform.Translate(curClass.dashSpd * Time.deltaTime, 0, 0);
		}

		// DOWN
		else if(Input.GetKey(KeyCode.S)) {
			player.GetComponent<SpriteRenderer>().sprite = curClass.charAnims.crouch[0];
			xVel = 0;
		}

		// IDLE
		else {
			player.GetComponent<SpriteRenderer>().sprite = curClass.charAnims.idle[0];
			xVel = 0;
		}

		// Make player jump
		if (Input.GetKeyDown("space")) {
			Jump();
		}

	}

	// Handles the player's state during hitstun
	private void ApplyHitstun(){
		// Return the previous state once out of hitstun
		if(timer.WaitForXFrames(25)){ // 25 is arbitrary 
			ChangeState(prevState);
		}
		// Otherwise play hitstun animation
		else{
			if(timer.curFrame() % 3 == 1){
				player.GetComponent<SpriteRenderer>().color = new Color(1,0,0);
			} else{
				player.GetComponent<SpriteRenderer>().color = new Color(1,1,1);
			}
		}
	}

	// Initiate jump
	private void Jump() {
		ChangeState(States.Airborne);

		this.yVel = Mathf.Abs(curClass.jumpHeight) * 0.2f;
		if(xVel > 0)		this.xVel = curClass.airSpd;
		else if(xVel < 0)	this.xVel = -curClass.airSpd;

		player.GetComponent<SpriteRenderer>().sprite = curClass.charAnims.jump[0];
	}

	// Swap the current class of the player
	private void Swap() {
		// Apply swap once character is faded enough
		if(player.GetComponent<SpriteRenderer>().color.r == 0.25f){
			player.GetComponent<SpriteRenderer>().color = new Color(1,1,1);

			// Apply the changes for swapping character
			if(curClass == sword)	curClass = shield;
			else					curClass = sword;

			if(prevState == States.Airborne) {
				if(xVel > 0)		this.xVel = curClass.airSpd;
				else if(xVel < 0)	this.xVel = -curClass.airSpd;

				player.GetComponent<SpriteRenderer>().sprite = curClass.charAnims.jump[0];
			}
			ChangeState(prevState);
		}
		// Otherwise continue to fade the character
		else{
			float i = player.GetComponent<SpriteRenderer>().color.r - Time.deltaTime * 1.5f;
            if(i < 0.25f)	i = 0.25f;
                player.GetComponent<SpriteRenderer>().color = new Color(i,i,i);
		}
	}

	// Apply gravity to the player
    private void ApplyGravity(){
        this.yVel -= curClass.gravity * Time.deltaTime;
        this.player.transform.Translate(this.xVel * Time.deltaTime, this.yVel, 0);
    }

	// Change player's current state and store it
	private void ChangeState(States s){
		prevState = curState;
		curState = s;
	}

	// Check if given hitbox connected with player hurtbox with adjusted positions
	private bool IsHitTarget(HitBox userHit, GameObject user, HitBox targetHurt, GameObject target) {
		// Shift user hitbox to position
		int flipScale = (int) (user.transform.localScale.x / (Mathf.Abs(user.transform.localScale.x))); 
		userHit.flipBox(flipScale);
		userHit.shiftBox(user.transform.localPosition.x, user.transform.localPosition.y);

		// Shift target hurtbox to position
		flipScale = (int) (target.transform.localScale.x / (Mathf.Abs(target.transform.localScale.x))); 
		targetHurt.flipBox(-flipScale);
		targetHurt.shiftBox(target.transform.localPosition.x, target.transform.localPosition.y);

		// See if the hitbox came inot contact with hurtbox
		return userHit.checkHit(targetHurt);
	}


	/* 
		Other methods that can be called by other scripts
	*/ 
	
	// Apply hurt process if player got hit by an attack
	public void Attacked(int damage){
		if(curState != States.Hurt && curState != States.Parry){
			curHP -= damage;
			if(curHP < 0)	curHP = 0;

			if(curState == States.Attack || curState == States.Swap)
				curState = prevState;
			ChangeState(States.Hurt);
			timer.resetWait();
		}
	}
	
}
