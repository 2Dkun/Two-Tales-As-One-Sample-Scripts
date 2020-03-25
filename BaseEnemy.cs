using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseEnemy : Entity {

    [System.Serializable]
    public class Anims {
        public SpriteAnimator idle, walk, retreat, stunned;
    }

    // Base enemy data
    [Header("Enemy Data")]
    public string enemyName;
    public EnemyHUDManager hud;
    public Anims anims;
    public Attack[] attacks;
    public float moveSpd, moveDist;
    //public float airAccel, airSpd, jumpHeight, gravity;
    public float detectRad;

    // Other data
    protected Vector2 origin;
    protected BasePlayer player;

    // Initialize data
    protected new void Start() {
        base.Start();
        curState = State.PATROL;
        origin = transform.localPosition;
        player = dungeon.GetPlayer();
        if (hud) {
            hud.SetPosition(transform);
            hud.InitText(enemyName, curHP);
        }
    }

    // Apply any additional changes to when enemy is attacked
    protected override void AttackedChanges() {
        if (curHP <= 0) {
            timer.ResetWait();
            ChangeState(State.KO);
        }
        if (hud) { hud.UpdateHPBar(curHP, maxHP); }
    }

    // Apply any additional changes to when enemy is stunned
    protected override void StunnedComplete() {
        anims.stunned.ResetAnim();
    }
    protected override void StunnedChanges() {
        GetComponent<SpriteRenderer>().sprite = anims.stunned.PlayAnim();
    }

    #region Abstract Functions
    // Allow the entity to act freely
    public abstract override void ActFreely();
    // Handles AI for enemy when player is not detected
    protected abstract void Patrol();
    // Handles AI for enemy when player is detected
    protected abstract void Agro();
    #endregion

    #region Protected Core Functions
    // See if the player is nearby 
    protected void DetectPlayer(bool isCircle) {
        HitBox fov;
        // Set hitbox for detection
        if (isCircle)   fov = new HitBox(detectRad, detectRad, -detectRad, -detectRad);
        else            fov = new HitBox(detectRad, 1, -detectRad, -1);
        bool isHit = IsHitTarget(fov, gameObject, player.GetHurtBox(), player.gameObject);

        if (isHit) {
            ChangeState(State.GROUNDED);
            timer.ResetWait();
        }
    }

    // Remove the enemy gameObj upon death
    protected void DestroyFoe() {
        // Fade GameObject to nothing
        float curAlpha = GetComponent<SpriteRenderer>().color.a - Time.deltaTime;
        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, curAlpha);
        if (hud) hud.SetAlpha(curAlpha);
        if (curAlpha <= 0) {
            Destroy(gameObject);
            if (hud) Destroy(hud.gameObject);
        }
    }

    // Handles the enemy's state during hitstun
    protected void ApplyHitstun() {
        // Return the previous state once out of hitstun
        if (subTimer.WaitForXFrames(Constants.HURT_TIME)) { 
            curState = prevState;
        }
        // Otherwise play hitstun animation
        else {
            if (subTimer.CurFrame() % 3 == 1) {
                GetComponent<SpriteRenderer>().color = new Color(1, 0, 0);
            }
            else {
                GetComponent<SpriteRenderer>().color = new Color(1, 1, 1);
            }
        }
    }
    #endregion
}
