using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour {

    /*
     *  NOTES:
     *  - Try to minimize times where entities directly access data
     *  from other entities. Maybe use dungeon manager to do this?
     *  - Ghost should inherit from entity and pull player data once
     *  from player script
     *      - Ghost calls functions in player script to change variables
     */

    // State Variables
    protected enum State {
        GROUNDED, AIRBORNE, ATTACK, HURT, STUNNED, KO,
        PATROL, SWAP, BLOCK, PARRY, CONTROL
    }
    protected State curState, prevState;

    // Public Data Variables
    [Header("Base Entity Data")]
    public DungeonHandler dungeon;
    public float maxHP, maxMP;
    public float mpRate;
    public HitBox hurtbox;
    public float gravity, fallSpd;

    // Hidden Data Variables
    protected float curHP, curMP;
    protected Attack curAttack;
    protected HitBox activeHit;
    protected FrameCounter timer, subTimer;
    protected int iframes;
    protected float xVel, yVel;
    protected float flipScale;
    protected float minHeight;

    // Initialize data
    protected void Start() {
        curHP = maxHP;
        curMP = maxMP;

        timer = new FrameCounter();
        subTimer = new FrameCounter();
        flipScale = Mathf.Abs(transform.localScale.x);

        if (!dungeon) dungeon = FindObjectOfType<DungeonHandler>();
    }

    #region Abstract Functions
    // Allow the entity to act freely
    public abstract void ActFreely();
    #endregion

    #region Virtual Functions
    // Apply additional changes to entity when they're attacked
    protected virtual void AttackedChanges() { }
    // Apply additional changes to entity when they're stunned
    protected virtual void StunnedChanges() { }
    protected virtual void StunnedComplete() { }
    // Add more functionality when attacks are done in the air
    protected virtual void AttackInAir() { }
    #endregion

    #region Protected Core Functions
    // TODO: CONFIRM THAT ENEMIES HAVE THEIR SPRITE FLIPPED IN INSPECTOR
    // Check if given hitbox connected with target hurtbox with adjusted positions
    protected bool IsHitTarget(HitBox userHit, GameObject user, HitBox targetHurt, GameObject target) {
        // Shift user hitbox to position
        int flipScale = (int)(user.transform.localScale.x / (Mathf.Abs(user.transform.localScale.x)));
        userHit.flipBox(flipScale);
        userHit.shiftBox(user.transform.localPosition.x, user.transform.localPosition.y);

        // Shift target hurtbox to position
        flipScale = (int)(target.transform.localScale.x / (Mathf.Abs(target.transform.localScale.x)));
        targetHurt.flipBox(flipScale);
        targetHurt.shiftBox(target.transform.localPosition.x, target.transform.localPosition.y);

        // See if the hitbox came inot contact with hurtbox
        return userHit.checkHit(targetHurt);
    }

    // TODO: CHECK TO MAKE SURE EVERYTHING WORKS
    private List<Entity> foesHit;
    protected bool Attack(Attack a, Entity[] targets) {
        // Play attack anim
        gameObject.GetComponent<SpriteRenderer>().sprite = a.anim.PlayAnim();

        // See if the move has ended
        FrameCounter timer = a.timer;
        if (timer.WaitForXFrames(a.endlag)) { //endlag
            EndAttack();
            ChangeState(prevState);
            return true; //Attack is completed
        }
        // Summon a projectile (if any) after the move
        else if (timer.CurFrame() == a.GetLastFrame() + 1) {
            GameObject proj = a.prefab;
            if (proj) {
                proj.GetComponent<Projectile>().dungeonData = gameObject;
                Vector3 scale = proj.transform.localScale;
                scale.z = transform.localScale.x / flipScale;
                scale.x = transform.localScale.x / flipScale;
                scale.x *= Mathf.Abs(proj.transform.localScale.x);
                proj.transform.localScale = scale;
                Instantiate(proj, gameObject.transform.position, Quaternion.identity);
            }
        }
        // Otherwise check if the move connected during its active frames
        // NEW IMPL: DAMAGE IS DONE AT THE LAST FRAME OF MOVE TO ALLOW FOR PARRIES TO NOT HIT GHOST
        else if (timer.CurFrame() >= a.startup && timer.CurFrame() <= a.GetLastFrame()) {
            // Change states to signal parry duration and  update active hitbox
            if (timer.CurFrame() == a.startup) {
                activeHit = a.hitBox;
                iframes = a.iframes;
                if (a.isParry) curState = State.PARRY;
                foesHit = new List<Entity>();
            }
            else if (timer.CurFrame() == a.GetLastFrame()) {
                activeHit = null;
                if (a.isParry) curState = State.ATTACK;
                for (int i = 0; i < foesHit.Count; i++)
                    foesHit[i].Attacked(curAttack.power);
            }

            // Check if attack connected with any target if it has a hitbox
            if (!a.hitBox.IsEqual(new HitBox()) && !a.isParry) {
                for (int i = 0; i < targets.Length; i++) {
                    // Check if target exists
                    if (targets[i] != null) {
                        HitBox tarHurt = targets[i].GetHurtBox();
                        bool isHit = IsHitTarget(a.hitBox, gameObject,
                            tarHurt, targets[i].gameObject);
                        // Tell the target that it has been attacked
                        if (isHit) {
                            /*
                            targets[i].Attacked(curAttack.power);
                            Debug.Log("hit");
                            */
                            foesHit.Add(targets[i]);
                        }
                    }
                }
            }

            // See if an attack was blocked if the player's attack
            else if (a.isParry) {
                List<Entity> parried = CheckParry(a.hitBox, targets);
                for (int i = 0; i < parried.Count; i++) {
                    parried[i].Parried();
                }
            }

            // Move the player based on attack velocity
            float playDir = transform.localScale.x / flipScale;
            transform.Translate(playDir * curAttack.xVel * Time.deltaTime,
                curAttack.yVel * Time.deltaTime, 0);
        }

        // Allow for movement if the player is airborne
        if (prevState == State.AIRBORNE) {
            AttackInAir();
        }
        return false; //Attack is still going on
    }
    #endregion

    #region Protected Helper Functions
    // Remove given health from entity
    protected void TakeDamage(float damage) {
        curHP -= damage;
        if (curHP < 0) curHP = 0;
    }

    // Change current state and store it
    protected void ChangeState(State s) {
        prevState = curState;
        curState = s;
    }

    // Apply gravity to the player
    protected void ApplyGravity() {
        yVel -= gravity * Time.deltaTime;
        if (yVel < 0) yVel *= fallSpd;
        transform.Translate(xVel * Time.deltaTime, yVel, 0);
    }

    // Apply changes to data when an attack ends
    protected void EndAttack() {
        timer.ResetWait();
        subTimer.ResetWait();
        if (curAttack != null) {
            curAttack.timer.ResetWait();
            curAttack.BeginCooldown();
            curAttack.anim.ResetAnim();
        }
        activeHit = null;
    }

    // Check if the player parried an attack
    protected List<Entity> CheckParry(HitBox hb, Entity[] targets) {
        List<Entity> ents = new List<Entity>();
        for (int i = 0; i < targets.Length; i++) {
            // Check if target exists
            if (targets[i] != null) {
                HitBox tarHit = new HitBox();
                targets[i].GetCurAtk(tarHit);
                if (!tarHit.IsEqual(new HitBox())) {
                    bool parried = IsHitTarget(hb, gameObject,
                        tarHit, targets[i].gameObject);
                    if (parried) {
                        //targets[i].Parried();
                        ents.Add(targets[i]);
                    }
                }
            }
        }
        return ents;
    }
    #endregion

    #region Public Functions
    public HitBox GetHurtBox() { return hurtbox; }

    // TODO: Check states to make sure entity can be attacked
    // Try to attack the entity
    public void Attacked(float damage) {
        if (curState != State.PARRY && curState != State.BLOCK
            && curState != State.HURT && damage > 0 && iframes <= 0) {

            if (curState == State.STUNNED)
                damage *= 3;
            TakeDamage(damage);

            if (curState == State.ATTACK || curState == State.SWAP || curState == State.STUNNED)
                curState = prevState;
            if (curState != State.HURT)
                ChangeState(State.HURT);
            Debug.Log(curState);
            EndAttack();
            AttackedChanges();
        }
    }

    // Set the entity into parried state
    public void Parried() {
        Debug.Log("Called");
        if (curAttack != null) {
            EndAttack();
        }
        curState = State.GROUNDED;
        ChangeState(State.STUNNED);
    }

    // Copy the hitbox of the user's current attack
    public void GetCurAtk(HitBox h) {
        if (activeHit != null) h.Clone(activeHit);
    }

    // Handles the entity's state during hitstun
    public void Stunned() {
        StunnedChanges();
        // Return the previous state once out of stun
        if (timer.WaitForXFrames(Constants.STUN_TIME)) {
            ChangeState(prevState);
            GetComponent<SpriteRenderer>().color = new Color(1, 1, 1);
            StunnedComplete();
        }
        // Otherwise play stunned animation
        else {
            if (timer.CurFrame() % 3 == 1) {
                GetComponent<SpriteRenderer>().color = new Color(1, 1, 0);
            }
            else {
                GetComponent<SpriteRenderer>().color = new Color(1, 1, 1);
            }
        }    }
    #endregion


}
