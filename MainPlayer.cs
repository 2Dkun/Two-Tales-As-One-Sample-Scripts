using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainPlayer : BasePlayer {

	public bool startShield;

	private HUDManager playerHUD;
	private Spirit spirit;
	private bool mpRecharge;

	private new void Start() {
		base.Start();

		spirit = dungeon.GetSpiritPlayer();
		playerHUD = dungeon.GetHUDManager();

		curSP = 100;
		if (startShield) {
			curClass = shield;
			spirit.SetSpiritClass(sword);
			playerHUD.SwapChar(); // Default is sword
		}
		else {
			curClass = sword;
			spirit.SetSpiritClass(shield);
		}
		gravity = curClass.gravity;
		fallSpd = curClass.fallSpd;
		
	}

    // Allow the player to act freely
	public override void ActFreely() {
		UpdateSP();
		UpdateMP();
		UpdateIFrames();

		// Control the spirit player
		if (Input.GetKeyDown(KeyCode.LeftShift) && curState == State.GROUNDED) {
			ChangeState(State.CONTROL);
			spirit.SetControl(true);
			curClass.charAnims.ctrlParticles.SetActive(true);
		}

		//Debug.Log("PLAYER: " + curState);
		Entity[] foes = dungeon.GetActiveEnemies();
		switch (curState) {
			case State.GROUNDED:    MoveGrounded(); break;
			case State.AIRBORNE:    MoveAirborne(); break;
			case State.SWAP:        Swap(); break;
			case State.ATTACK:      Attack(curAttack, foes); break;
			case State.BLOCK:       BlockSkill(true); break;
			case State.PARRY:       Attack(curAttack, foes); break;
			case State.HURT:        ApplyHitstun(); break;
			case State.CONTROL:     Control(); break;
			default:                Debug.Log("Bad State: " + curState); break;
		}
		
	}

    #region Virtual Function Implmentation
    protected override void AttackedChanges() {
        base.AttackedChanges();
		iframes = Constants.IFRAMES;
		playerHUD.UpdateHP(curHP / maxHP);
	}

    protected override void StartSwapChanges() {
        base.StartSwapChanges();
		spirit.Swap();
    }
    #endregion

    #region Private Helper Functions
    // Make the changes to the player when a swap occurs
    private void ApplySwapChanges() {
		if (curClass == sword) curClass = shield; 
		else curClass = sword;

		gravity = curClass.gravity;
		fallSpd = curClass.fallSpd;

		if (prevState == State.AIRBORNE) {
			if (xVel > 0) xVel = curClass.airSpd;
			else if (xVel < 0) xVel = -curClass.airSpd;
		}

		playerHUD.SwapChar();
	}

    // Update the SP meter
	private void UpdateSP() {
		if (curSP < 100) {
			curSP += Time.deltaTime * curClass.swapRate;
			if (curSP > 100) curSP = 100;
			playerHUD.UpdateSP(curSP / 100);
		}
	}

	// Update the MP meter
	private void UpdateMP() {
		if (mpRecharge) {
			curMP += Time.deltaTime * mpRate;
			if (curMP >= maxMP) { curMP = maxMP; mpRecharge = false; }
			playerHUD.RechargeMP(curMP / maxMP);
		}
	}

	// Update iframes
	private void UpdateIFrames() {
		if (iframes > 0) {
			// Handle iframe animation
			if (subTimer.WaitForXFrames(iframes)) { // 25 is arbitrary 
				subTimer.ResetWait();
				iframes = 0;
				GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
			}
			else {
				if (subTimer.CurFrame() % 3 == 1) {
					GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
				}
				else {
					GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
				}
			}
		}
	}

	// Handles the player's state during hitstun
	private void ApplyHitstun() {
		// Return the previous state once out of hitstun
		if (timer.WaitForXFrames(Constants.HURT_TIME)) {  
			ChangeState(prevState);
		}
		// Otherwise play hitstun animation
		else {
			if (timer.CurFrame() % 3 == 1) {
				GetComponent<SpriteRenderer>().color = new Color(1, 0, 0);
			}
			else {
				GetComponent<SpriteRenderer>().color = new Color(1, 1, 1);
			}
		}
	}
	#endregion

	#region Public Spirit Functions
    // Returns the class that isn't currently active
	public PlayerClass GetSpiritClass() {
		if (curClass == sword) return shield;
		return sword;
    }

    // Deal damage to the main player
	public void HurtMainPlayer(float damage) {
		TakeDamage(damage);
		playerHUD.UpdateHP(curHP / maxHP);
	}

	// Get the player to stop controlling the ghost
	public void UndoControl() {
		spirit.SetControl(false);
		if (curState == State.HURT) prevState = State.GROUNDED;
		else curState = State.GROUNDED;
		curClass.charAnims.ctrlParticles.SetActive(false);
		dungeon.UpdateCamera(gameObject);
	}

    // Attempt to swap with the main player
	public void SpiritSwap() { AttemptSwap(); }
	#endregion

	// Handles events in control state
	private void Control() {
		GetComponent<SpriteRenderer>().sprite = curClass.charAnims.control.PlayAnim();

		if (!Input.GetKey(KeyCode.LeftShift)) { UndoControl(); }
	}

    // Swap the current class of the player
    private void Swap() {
		GetComponent<SpriteRenderer>().sprite = curClass.charAnims.swap.PlayAnim();

		// Apply swap once character is faded enough
		if (GetComponent<SpriteRenderer>().color.r == 0.25f) {
			GetComponent<SpriteRenderer>().color = new Color(1, 1, 1);

			// Apply the changes for swapping character
			ApplySwapChanges();
			curSP = 0;

			//playerHUD.GetComponent<HUDManager>().SwapChar();
			spirit.SwapChar();

			ChangeState(prevState);
			dungeon.PauseEnemies(false);
		}
		// Otherwise continue to fade the character
		else {
			float i = GetComponent<SpriteRenderer>().color.r - Time.deltaTime * 1.5f;
			if (i < 0.25f) {
				i = 0.25f;
			}
			GetComponent<SpriteRenderer>().color = new Color(i, i, i);
		}
	}
}
