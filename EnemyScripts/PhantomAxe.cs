using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhantomAxe : BaseEnemy {

	// Enemy child specific variables
	[Header("Enemy Child Data")]
	public float strikeDist;
	private HitBox strikeBox = new HitBox();
	private bool isTurn;

	// Initialize data
	new void Start() {
		base.Start();
		strikeBox = new HitBox(strikeDist, 1, 0, -1);
	}

	// Allow the enemy to act freely
	public override void ActFreely() {
		Entity[] targets = dungeon.GetActivePlayers();
        switch (curState) {
            case State.PATROL:      Patrol(); break;
            case State.GROUNDED:    Agro(); break;
            case State.ATTACK:      Attack(curAttack, targets); break;
            case State.STUNNED:     Stunned(); break;
            case State.KO:          DestroyFoe(); break;
            default:                DestroyFoe(); break;
        }
    }

	// Handles AI for enemy when player is not detected
	protected override void Patrol() {
		// Wait a moment before turning around
		if (transform.localPosition.x > origin.x + moveDist / 2 && transform.localScale.x > 0) {
			isTurn = true;
			if (timer.WaitForXFrames(Random.Range(55, 75))) {
				transform.localScale = new Vector2(-flipScale, flipScale);
				isTurn = false;
			}
			else {
				GetComponent<SpriteRenderer>().sprite = anims.idle.PlayAnim();
			}
		}
		else if (transform.localPosition.x < origin.x - moveDist / 2 && transform.localScale.x < 0) {
			isTurn = true;
			if (timer.WaitForXFrames(Random.Range(55, 75))) {
				transform.localScale = new Vector2(flipScale, flipScale);
				isTurn = false;
			}
			else {
				GetComponent<SpriteRenderer>().sprite = anims.idle.PlayAnim();
			}
		}
		else {
			GetComponent<SpriteRenderer>().sprite = anims.walk.PlayAnim();
		}

		// Walk to left
		if (transform.localScale.x < 0 && !isTurn) {
			transform.Translate(-moveSpd * Time.deltaTime, 0, 0);
		}
		// Walk to right
		else if (!isTurn) {
			transform.Translate(moveSpd * Time.deltaTime, 0, 0);
		}

		// Try to detect player
	    DetectPlayer(false);
	}

	// Handles AI for enemy when player is detected
	protected override void Agro() {
		// Stop being agro if player is too far away
		float dist = Vector2.Distance(player.transform.localPosition, transform.localPosition);
		if (dist > detectRad * 2) {
			ChangeState(State.PATROL);
			timer.ResetWait();
		}

		// Back away from player slowly if enemy is in cooldown
		if (attacks[0].InCooldown()) {
			// PLAY RETREAT ANIM
			gameObject.GetComponent<SpriteRenderer>().sprite = anims.retreat.PlayAnim();

			if (transform.localPosition.x > player.transform.localPosition.x) {
				transform.localScale = new Vector2(-flipScale, flipScale);
				transform.Translate(moveSpd * Time.deltaTime, 0, 0);
			}
			else {
				transform.localScale = new Vector2(flipScale, flipScale);
				transform.Translate(-moveSpd * Time.deltaTime, 0, 0);
			}
			
		}
		// Otherwise go on the offense
		else {
			// See if player is close enough to swing
			HitBox hurt = player.GetHurtBox();
			bool isStrike = IsHitTarget(strikeBox, gameObject, hurt, player.gameObject);
			if (isStrike) {
				curAttack = attacks[0];
				ChangeState(State.ATTACK);
			}
			// Move towards player if not close enough
			else {
				// PLAY WALK ANIMATION
				gameObject.GetComponent<SpriteRenderer>().sprite = anims.walk.PlayAnim();

				if (transform.localPosition.x > player.transform.localPosition.x) {
					transform.localScale = new Vector2(-flipScale, flipScale);
					transform.Translate(-moveSpd * Time.deltaTime, 0, 0);
				}
				else {
					transform.localScale = new Vector2(flipScale, flipScale);
					transform.Translate(moveSpd * Time.deltaTime, 0, 0);
				}
			}
		}
	}


}
