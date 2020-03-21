using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasePlayer : Entity {

	[System.Serializable]
	public class Anims {
		public SpriteAnimator idle, crouch, walk, dash, jump, swap, control;
		public GameObject ctrlParticles;
	}

	[System.Serializable]
	public class Block {
		public int parryFrames, maxBlock;
		public SpriteAnimator block, blocked, stop, parry;
		public Attack rollF, rollB;
		public bool parried { get; set; }
    }

	[System.Serializable]
	public class PlayerClass {
		public bool isShield;
		public Anims charAnims;
		public float walkSpd, dashSpd, airSpd;
		public float jumpHeight, gravity, fallSpd;
		public float airAccel, swapRate;
		public Attack atk, cAtk;
		public Attack[] skills;
		public Block block;
	}

	[Header("Player Data")]
	public PlayerClass sword;
    public PlayerClass shield;

	// Other character data
	protected PlayerClass curClass;
	protected float curSP;

    #region Abstract Classes
    public abstract override void ActFreely();

	#endregion

	#region Virtual Classes
	protected virtual void StartSwapChanges() { }
	#endregion

	#region Private Helper Functions
	// Initiate jump
	private void Jump() {
		ChangeState(State.AIRBORNE);

		yVel = Mathf.Abs(curClass.jumpHeight) * 0.2f;
		if (xVel > 0) xVel = curClass.airSpd;
		else if (xVel < 0) xVel = -curClass.airSpd;
	}
	#endregion

	#region Protected Helper Functions
	// Attempt to swap to other character
	protected void AttemptSwap() {
		if (curSP >= 100) {
			ChangeState(State.SWAP);
			timer.ResetWait();
			StartSwapChanges();

			// Stop enemies from moving
			dungeon.PauseEnemies(true);
		}
		else {
			// IMPLEMENT STRUGGLE ANIMATION LATER

			dungeon.PauseEnemies(false);
		}
	}
	#endregion

	#region Core Portected Functions
    // Handles the player's grounded options
    protected void MoveGrounded() {
		// SWAP
		if (Input.GetKeyDown(KeyCode.W)) {
			AttemptSwap();
		}

		// ATTACK OR BLOCK
		else if (Input.GetKeyDown(KeyCode.J)) {
			if (!Input.GetKey(KeyCode.S) && curClass.isShield) {
				ChangeState(State.BLOCK);
			}
			else {
				ChangeState(State.ATTACK);

				// Decide if the attack was a normal attack or low
				if (Input.GetKey(KeyCode.S)) curAttack = curClass.cAtk;
				else curAttack = curClass.atk;

				// Change direction of attack based on input
				if (Input.GetKey(KeyCode.A)) {
					transform.localScale = new Vector2(-flipScale, flipScale);
				}
				else if (Input.GetKey(KeyCode.D)) {
					transform.localScale = new Vector2(flipScale, flipScale);
				}
			}

			timer.ResetWait();
		}

		// MOVE LEFT
		else if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.LeftShift)) {
			GetComponent<SpriteRenderer>().sprite = curClass.charAnims.walk.PlayAnim();
			transform.localScale = new Vector2(-flipScale, flipScale);
			transform.Translate(-1 * curClass.walkSpd * Time.deltaTime, 0, 0);
		}
		else if (Input.GetKey(KeyCode.A)) {
			GetComponent<SpriteRenderer>().sprite = curClass.charAnims.dash.PlayAnim();
			transform.localScale = new Vector2(-flipScale, flipScale);
			transform.Translate(-1 * curClass.dashSpd * Time.deltaTime, 0, 0);
		}

		// MOVE RIGHT
		else if (Input.GetKey(KeyCode.D) && Input.GetKey(KeyCode.LeftShift)) {
			GetComponent<SpriteRenderer>().sprite = curClass.charAnims.walk.PlayAnim();
			transform.localScale = new Vector2(flipScale, flipScale);
			transform.Translate(curClass.walkSpd * Time.deltaTime, 0, 0);
		}
		else if (Input.GetKey(KeyCode.D)) {
			GetComponent<SpriteRenderer>().sprite = curClass.charAnims.dash.PlayAnim();
			transform.localScale = new Vector2(flipScale, flipScale);
			transform.Translate(curClass.dashSpd * Time.deltaTime, 0, 0);
		}

		// DOWN
		else if (Input.GetKey(KeyCode.S)) {
			GetComponent<SpriteRenderer>().sprite = curClass.charAnims.crouch.PlayAnim();
			xVel = 0;
		}

		// IDLE
		else {
			GetComponent<SpriteRenderer>().sprite = curClass.charAnims.idle.PlayAnim();
			xVel = 0;
		}

		// Make player jump
		if (Input.GetKeyDown("space")) {
			Jump();
		}
	}

	// Handles the player's air movement
	protected void MoveAirborne() {
		GetComponent<SpriteRenderer>().sprite = curClass.charAnims.jump.PlayAnim();

		// SWAP
		if (Input.GetKeyDown(KeyCode.W)) {
			AttemptSwap();
		}

		// MOVE LEFT
		if (Input.GetKey(KeyCode.A)) {
			xVel += -curClass.airAccel * Time.deltaTime * 30;
			if (xVel < -curClass.airSpd)
				xVel = -curClass.airSpd;
		}
		// MOVE RIGHT
		else if (Input.GetKey(KeyCode.D)) {
			this.xVel += curClass.airAccel * Time.deltaTime * 30;
			if (xVel > curClass.airSpd)
				xVel = curClass.airSpd;
		}

		// ATTACK
		if (Input.GetKeyDown(KeyCode.J)) {
			ChangeState(State.ATTACK);
			curAttack = curClass.atk;
			yVel = 0;
		}
		ApplyGravity();

		// Check if player has landed
		if (transform.localPosition.y <= minHeight) {
			transform.localPosition = new Vector2(transform.localPosition.x, minHeight);
			ChangeState(State.GROUNDED);
			xVel = 0;
			yVel = 0;
		}
	}
	#endregion

	#region Shield Specific Functions
    // Handles the block state
	protected void BlockSkill(bool canRoll) {
		Block b = curClass.block;

		if (b.parried) {
			GetComponent<SpriteRenderer>().sprite = b.parry.PlayAnim();
			// Allow Sword to attack here


			if (b.parry.isDone()) {
				timer.ResetWait();
				b.parried = false;
				ChangeState(State.GROUNDED);
			}
		}
		// End the block
		else if (timer.WaitForXFrames(b.maxBlock)) {// || !Input.GetKey(KeyCode.J)) {
			timer.ResetWait();
			b.parried = false;
			ChangeState(State.GROUNDED);
		}
		else {
			// ROLL LEFT
			if (Input.GetKeyDown(KeyCode.A) && canRoll) {
				timer.ResetWait();
				b.parried = false;
				curState = State.GROUNDED;
				ChangeState(State.ATTACK);
				// Decide which roll to use
				if (transform.localScale.x > 0) curAttack = b.rollB;
				else curAttack = b.rollF;
			}
			// ROLL RIGHT
			else if (Input.GetKeyDown(KeyCode.D) && canRoll) {
				timer.ResetWait();
				b.parried = false;
				curState = State.GROUNDED;
				ChangeState(State.ATTACK);
				// Decide which roll to use
				if (transform.localScale.x > 0) curAttack = b.rollF;
				else curAttack = b.rollB;
			}
			// BLOCK
			else {
				// Check if an attack was parried
				if (timer.CurFrame() <= b.parryFrames) {
					List<Entity> parried = CheckParry(hurtbox, dungeon.GetActiveEnemies());
					for (int i = 0; i < parried.Count; i++) {
						parried[i].Parried();
					}
					if (parried.Count > 0) b.parried = true;
				}
				
				// Play block anims
				if (timer.CurFrame() > b.maxBlock - b.stop.getDuration()) 
					GetComponent<SpriteRenderer>().sprite = b.stop.PlayAnim();
				else 
				    GetComponent<SpriteRenderer>().sprite = b.block.PlayAnim();
			}
		}
    }
    #endregion
}
