using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
 
	/*
		CURRUNT TASK:
		Change display base on current hp, mp, and swap

		TO DO LIST:
		- Setup up enemy scripts using the child thing
			- Think about how it can work when you need to call its method
			- https://en.wikipedia.org/wiki/Template_method_pattern
			- https://answers.unity.com/questions/1266855/access-subclass-through-base.html
			- YO: https://answers.unity.com/questions/471551/how-can-i-access-a-function-without-knowing-the-sc.html 
		- Improve parry
		- Improve on enemy AI
		- Work on animations

		- FIGURE OUT HOW YOU'RE GONNA ALLOW SKILLS TO BE USED
			- What you have so far:
				- Enemy will treat damage of -1 as parry kill
				- Skills are defined for each char. You must figure out how to organize them
				- Figure out how to let Shield use his first skill and give it an mp cost. 
		- Implement a block mechanic for shield
		- Make sword attacks a bit slower
	 */

	[System.Serializable]
    public class Anims {
		public Sprite[] idle, crouch, walk, dash, jump, swap;
	}

	[System.Serializable]
	public class PlayerClass {
		public Anims charAnims;
		public float walkSpd, dashSpd, airSpd, jumpHeight, gravity;
		public float airAccel, swapRate;
		public Attack atk, cAtk;
		public Attack[] skills;
		public Attack swapAtk;
	}

	// Base character data
    public int maxHP = 100, maxMP = 100;    // just a default value
	public PlayerClass sword, shield;
 
	// Character states
	private enum States { Grounded, Airborne, Swap, Attack, Parry, Hurt, Block };
	private States curState { get; set; }
	private States prevState { get; set; }

	// Other character data
	public GameObject player;
	private GameObject[] foes;
	public HitBox hurtBox;
	private PlayerClass curClass;
	private int curHP, curMP;
	private float curSP = 100;
	private float xVel, yVel;
	private float flipScale;
	private Attack curAttack;
	private FrameCounter timer, subTimer;
	private int iframes;

	// Player Objects
	public GameObject playerHUD;

	// TEMP VARS
	public float minHeight; // detect ground better in future
	 // place this info in dungeon manager and recieve it from there

	// Initialize variables
    public void Start() {
		timer = new FrameCounter();
		subTimer = new FrameCounter();
		foes = gameObject.GetComponent<DungeonManager>().enemies;

		curHP = this.maxHP;
		curMP = this.maxMP;
		curClass = this.sword;
		curState = States.Grounded;
		prevState = States.Grounded;
		flipScale = Mathf.Abs(player.transform.localScale.x);
	}

	// TESTING PURPOSES REMOVE ME LATER
	/*
	public void Update(){
		ControlPlayer();

		// Update SWAP meter
		if(curSP < 100) {
			curSP += Time.deltaTime * curClass.swapRate;
			if(curSP > 100) {
				curSP = 100;
			}
			playerHUD.GetComponent<HUDManager>().UpdateSP(curSP/100);		
		}
	}
	*/

	public void UpdateGround(float newHeight){
		minHeight = newHeight;
		if(minHeight != transform.position.y) {
			//Debug.Log("check");
			if(curState == States.Grounded) 	ChangeState(States.Airborne);
			if(prevState == States.Grounded) 	prevState = States.Airborne;
		}
	}

	// Allow the user to have full control over the player
	public void ControlPlayer(){
		//Debug.Log(curState);

		// Update SWAP meter
		if(curSP < 100) {
			curSP += Time.deltaTime * curClass.swapRate;
			if(curSP > 100) {
				curSP = 100;
			}
			playerHUD.GetComponent<HUDManager>().UpdateSP(curSP/100);		
		}
		// Update iframes
		if(iframes > 0) {
			// Handle iframe animation
			if(subTimer.WaitForXFrames(iframes)){ // 25 is arbitrary 
				subTimer.resetWait();
				iframes = 0;
			}
			else{
				if(subTimer.curFrame() % 3 == 1){
					player.GetComponent<SpriteRenderer>().color = new Color(1,1,1,0);
				} else{
					player.GetComponent<SpriteRenderer>().color = new Color(1,1,1,1);
				}
			}
		}

		// See if player made any skill inputs
		if(curClass == this.shield){
			if(Input.GetKey(KeyCode.Semicolon))		Block(shield.skills[0], KeyCode.Semicolon);
			else if(Input.GetKey(KeyCode.K))		ParryStrike(shield.skills[1]);
			else if(Input.GetKey(KeyCode.U))		BasicSkill(shield.skills[2]);
		}
		else{
			if(Input.GetKey(KeyCode.J))				AnotherSwing(sword.skills[0], sword.skills[1]);
			else if(Input.GetKey(KeyCode.U))		BasicSkill(sword.skills[2]);
		}

		// Run through player state machine
		switch(curState){
			case States.Grounded:	MoveGrounded(); 								break;
			case States.Airborne: 	MoveAirborne(); 								break;
			case States.Swap:		Swap(); 										break;
			case States.Attack:  	Attack();										break; 
			case States.Parry: 		Attack();										break;
			case States.Hurt:		ApplyHitstun(); 								break;
			case States.Block:		Block(shield.skills[0], KeyCode.Semicolon); 	break;
			default: 																break;
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
		// Summon a projectile (if any) after the move
		else if(timer.curFrame() == curAttack.getLastFrame() + 1){
			GameObject proj = curAttack.prefab;
			if(proj){
				proj.GetComponent<Projectile>().dungeonData = gameObject;
				Vector3 scale = proj.transform.localScale;
				scale.z = transform.localScale.x/flipScale;
				scale.x = transform.localScale.x/flipScale;
				scale.x *= Mathf.Abs(proj.transform.localScale.x);
				proj.transform.localScale = scale;
				Instantiate(proj, gameObject.transform.position, Quaternion.identity);
			}
		}
		// Otherwise check if the move connected during its active frames
		else if(timer.curFrame() >= curAttack.startup && timer.curFrame() <= curAttack.getLastFrame()){ 
			// Check if attack connected with any enemy if it has a hitbox
			if(!curAttack.hitBox.IsEqual(new HitBox())){
				for(int i = 0; i < foes.Length; i++){
					// Check if enemy exists
					if(foes[i] != null) {		
						HitBox foeHurt = new HitBox();
						foes[i].SendMessage("GetHurtBox", foeHurt);
						bool isHit = IsHitTarget(curAttack.hitBox, player, foeHurt, foes[i]);
						// Tell the enemy that it has been attacked
						if(isHit){
							foes[i].SendMessage("Attacked", curAttack.power);
						}
					}
				}
			}
				
			// See if an attack was blocked if the player's attack is a parry
			if(curAttack.isParry){
				for(int i = 0; i < foes.Length; i++){
					// Check if enemy exists
					if(foes[i] != null) {
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

			// Move the player based on attack velocity
			float playDir = transform.localScale.x/flipScale;
			player.transform.Translate(playDir * curAttack.xVel * Time.deltaTime, 
				curAttack.yVel * Time.deltaTime, 0);
		}
		else if(Input.GetKey(KeyCode.W) && curSP >= 100) {
			// Reset previous attack
			curAttack.anim.ResetAnim();
			timer.resetWait();

			// Set current attack as Skill1
			curAttack = curClass.swapAtk;
			curMP -= curClass.swapAtk.mpCost;
			playerHUD.GetComponent<HUDManager>().UpdateMP((float)curMP/maxMP);

			// Apply the changes for swapping character
			if(curClass == sword)	curClass = shield;
			else					curClass = sword;
			curSP = 0;

			playerHUD.GetComponent<HUDManager>().SwapChar();
		}
		/*
		else if(curState == States.Parry) {
			// curState = States.Attack;
		}
		*/
			
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
		if (Input.GetKeyDown(KeyCode.W)) {
			if(curSP >= 100) {
				ChangeState(States.Swap);
				player.GetComponent<SpriteRenderer>().sprite = curClass.charAnims.swap[0];
				timer.resetWait();
			}
			else {
				// IMPLEMENT STRUGGLE ANIMATION LATER
			}
		}

		// MOVE LEFT
		if(Input.GetKey(KeyCode.A)) {
			this.xVel += -curClass.airAccel * Time.deltaTime * 30;
			if(xVel < -curClass.airSpd)
				xVel = -curClass.airSpd;
			//player.transform.localScale = new Vector2(-flipScale, flipScale);
		}
		// MOVE RIGHT
		else if(Input.GetKey(KeyCode.D)) {
			this.xVel += curClass.airAccel * Time.deltaTime * 30;
			if(xVel > curClass.airSpd)
				xVel = curClass.airSpd;
			//player.transform.localScale = new Vector2(flipScale, flipScale);
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
			yVel = 0;
        }
	}

	// Handles the player's grounded movement
	private void MoveGrounded() {

		// SWAP
		if (Input.GetKeyDown(KeyCode.W)) {
			if(curSP >= 100) {
				ChangeState(States.Swap);
				player.GetComponent<SpriteRenderer>().sprite = curClass.charAnims.swap[0];
				timer.resetWait();
				// Stop enemies from moving
				gameObject.GetComponent<DungeonManager>().PauseGame();
			}
			else {
				// IMPLEMENT STRUGGLE ANIMATION LATER
			}
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
			curSP = 0;

			playerHUD.GetComponent<HUDManager>().SwapChar();		

			if(prevState == States.Airborne) {
				if(xVel > 0)		this.xVel = curClass.airSpd;
				else if(xVel < 0)	this.xVel = -curClass.airSpd;

				player.GetComponent<SpriteRenderer>().sprite = curClass.charAnims.jump[0];
			}
			ChangeState(prevState);
			gameObject.GetComponent<DungeonManager>().ContinueGame();
		}
		// Otherwise continue to fade the character
		else{
			float i = player.GetComponent<SpriteRenderer>().color.r - Time.deltaTime * 1.5f;
            if(i < 0.25f) {
				i = 0.25f;
			}
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


	/* --- Other methods that can be called by other scripts --- */ 
	
	// Apply hurt process if player got hit by an attack
	public void Attacked(int damage){
		if(curState != States.Parry && curState != States.Block 
			&& curState != States.Hurt && damage > 0 && iframes <= 0){

			curHP -= damage;
			if(curHP < 0)	curHP = 0;
			iframes = Constants.IFRAMES;

			if(curState == States.Attack || curState == States.Swap)
				curState = prevState;
			if(curState != States.Hurt)
				ChangeState(States.Hurt);
			playerHUD.GetComponent<HUDManager>().UpdateHP((float)curHP/maxHP);
			timer.resetWait();
		}
	}
	

	// Allows players to use a skill with no unique traits
	private void BasicSkill(Attack skill){
		if(curState == States.Grounded && skill.mpCost <= curMP){
			// Set current attack as Skill1
			curAttack = skill;
			curMP -= skill.mpCost;
			playerHUD.GetComponent<HUDManager>().UpdateMP((float)curMP/maxMP);

			ChangeState(States.Attack);
		}
	}

	/* -------------------------------- SWORD SKILLS -------------------------------- */
	// A skill that allows for multiple swings from Shida's basic attack
	private void AnotherSwing(Attack atk2, Attack atk3) {
		if(curAttack == sword.atk){
			if(timer.curFrame() >= curAttack.getLastFrame() && timer.curFrame() < curAttack.endlag){ 
				// Reset previous attack
				curAttack.anim.ResetAnim();
				timer.resetWait();

				// Set new attack to swing 2
				curAttack = atk2;
			}
		}
		else if(curAttack == atk2){
			if(timer.curFrame() >= atk2.getLastFrame() && timer.curFrame() < atk2.endlag){ 
				// Reset previous attack
				curAttack.anim.ResetAnim();
				timer.resetWait();

				// Set new attack to swing 2
				curAttack = atk3;
			}
		}
	}

	/* -------------------------------- SHIELD SKILLS -------------------------------- */
	// A skill that prevents the player from getting hit
	private void Block(Attack skill, KeyCode cmd){
		// Can only block on the ground
		if(curState == States.Grounded){
			ChangeState(States.Block);
		}
		else if(curState == States.Block){
			// Play attack animation
			player.GetComponent<SpriteRenderer>().sprite = skill.anim.PlayAnim();
		}
		// Handle inputs
		if(Input.GetKeyUp(cmd)){
			ChangeState(States.Grounded);
		}
		else if(Input.GetKey(KeyCode.A)) {
			player.transform.localScale = new Vector2(-flipScale, flipScale);
		}
		else if(Input.GetKey(KeyCode.D)) {
			player.transform.localScale = new Vector2(flipScale, flipScale);
		}
	}
	
	// A skill that switches the current player to Shida and strikes the enemy
	private void ParryStrike(Attack skill){
		// Skill can only be done during a parry
		if(curState == States.Parry && skill.mpCost <= curMP){
			// Reset previous attack
			curAttack.anim.ResetAnim();
			timer.resetWait();

			// Set current attack as Skill1
			curAttack = skill;
			curMP -= skill.mpCost;
			playerHUD.GetComponent<HUDManager>().UpdateMP((float)curMP/maxMP);

			// Apply the changes for swapping character
			if(curClass == sword)	curClass = shield;
			else					curClass = sword;
			curSP = 0;

			playerHUD.GetComponent<HUDManager>().SwapChar();	
		}
	}

}
