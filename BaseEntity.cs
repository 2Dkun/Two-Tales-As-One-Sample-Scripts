using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class BaseEntity : PhysicsEntity {
    
    // Character states
    protected enum States {
        Idle, Swap, Attack, Parry, Hurt, KO, Special,
    };
    protected States curState, prevState;

    // Game components
    protected DungeonManager dungeon;
    protected ShaderManager shader;
    protected ConstantConfig constants;
    protected Animator _animator;

    // Basic character data
    protected float flipScale;
    protected float curHP, curMP;
    
    // Attack variables
    private Attack curAttack;
    protected Attack CurAttack => curAttack;

    protected int iframes;
    
    // Other data
    protected readonly Timer timer = new Timer();
    protected Hitbox hurtbox; // TODO: MAKE THIS ORGANIZED IN DATA SOMEHOW AND ADJUSTABLE WITH ANIAMTIONS?
    protected bool InEndlag => curState == States.Attack && curAttack.InEndlag(atkTimer.CurFrame());
    protected bool HasStarted => curState == States.Attack && (curAttack.IsActive(atkTimer.CurFrame()) || InEndlag);
    protected bool IsActive => curState == States.Attack && curAttack.IsActive(atkTimer.CurFrame());

    /* Abstract Functions */
    public abstract void Attacked(float damage, Vector3 knockback, bool wasTipper);

    public virtual void UpdateEntity() { }

    protected void Start() {
        if (!dungeon) dungeon = FindObjectOfType<DungeonManager>();
        shader = gameObject.AddComponent <ShaderManager>();
        constants = dungeon.constants;
        _animator = GetComponent<Animator>();
        if (!_animator) _animator = GetComponentInChildren<Animator>();

        var localScale = transform.localScale;
        flipScale = Mathf.Abs(localScale.x);
        hurtbox = new Hitbox(box, localScale);
    }

    #region Protected Core Functions

    // Check if given hitbox connected with target hurtbox with adjusted positions
    protected static bool IsHitTarget(Hitbox userHit, float userScaleX, Vector2 userPos, 
        Hitbox targetHurt, float targetScaleX, Vector2 targetPos) {
        // Shift user hitbox to position
        var userFlip = (int) Mathf.Sign(userScaleX);
        userHit.UpdateBox(userPos, userFlip);
        // Shift target hurtbox to position
        var tarFlip = (int) Mathf.Sign(targetScaleX);
        targetHurt.UpdateBox(targetPos, tarFlip);

        // See if the hitbox came into contact with hurtbox
        return userHit.CheckHit(targetHurt);
    }
    
    protected static bool IsHitTarget(Hitbox userHit, GameObject user, Hitbox targetHurt, GameObject target) {
        float userScale = user.transform.localScale.x;
        Vector2 userPos = user.transform.position;
        float targetScale = target.transform.localScale.x;
        Vector2 targetPos = target.transform.position;

        // See if the hitbox came into contact with hurtbox
        return IsHitTarget(userHit, userScale, userPos, targetHurt, targetScale, targetPos);
    }
    
    // Check if given hitbox connected with target hurtbox with adjusted positions
    public static bool IsHitTarget(Hitbox[] userHit, GameObject user, Hitbox targetHurt, GameObject target) {
        bool hasHitTarget = false;
        float userScale = user.transform.localScale.x;
        Vector2 userPos = user.transform.position;
        float targetScale = target.transform.localScale.x;
        Vector2 targetPos = target.transform.position;
        
        for (int i = 0; i < userHit.Length && !hasHitTarget; i++) {
            hasHitTarget = IsHitTarget(userHit[i], userScale, userPos, targetHurt, targetScale, targetPos);
        }
        return hasHitTarget;
    }

    // Check to see of the current attack's hitbox overlapped with a target
    private bool hasHitTarget;
    private HashSet<BaseEntity> hitList = new HashSet<BaseEntity>();
    private readonly Timer atkTimer = new Timer();
    protected void Attack(BaseEntity target, bool advanceTime = true)
    {
        if(advanceTime) atkTimer.AdvanceTime();
        
        // See if the move has ended
        if (atkTimer.CurFrame() >= curAttack.GetTotalFrames()) {
            EndAttack();
            return; 
        }

        // Heal the user on the startup frame
        if (curAttack.isHeal)
        {
            if (!hasHitTarget && curAttack.IsActive(atkTimer.CurFrame()))
            {
                hasHitTarget = true;
                curHP += curAttack.power;
            }
        }

        // Check if the move connected during its active frames
        else if (curAttack.IsActive(atkTimer.CurFrame()) && !hitList.Contains(target)) {
            // Setup knockback and pushback data
            Vector2 userDir = new Vector2(Mathf.Sign(transform.localScale.x), 1);
            
            // Check if attack connected with any target if it has a hitbox
            var tarHurt = target.GetHurtBox();

            var wasTipper = false;
            var power = curAttack.power;
            var isHit = IsHitTarget(curAttack.basespots, gameObject, tarHurt, target.gameObject);
            if(!isHit) {
                isHit = IsHitTarget(curAttack.sweetspots, gameObject, tarHurt, target.gameObject);
                if (isHit) {
                    wasTipper = true;
                    power = (int) (power * constants.critMultiplier);
                }
            }
            else if(!hasHitTarget && curAttack.pushback != Vector2.zero)
            { 
                Vector2 push = curAttack.pushback * userDir;
                // if (Math.Abs(push.y) < Mathf.Epsilon && velocity.y <= 0) velocity = velocity + push;
                // else velocity = curAttack.pushback * playerDir;
                if (push.y > 0 && velocity.y < push.y) velocity = curAttack.pushback * userDir;
                else velocity = velocity + push/2;
            }

            // Tell the target that it has been attacked
            if (isHit)
            {
                hitList.Add(target);
                hasHitTarget = true;
                target.Attacked(power, curAttack.knockback * userDir, wasTipper);
            }
        }

        if (curAttack.projectile) HandleProjectile(false);
    }

    protected void Attack(List<BaseEntity> targets)
    {
        // End the attack if it's done
        if (atkTimer.WaitForXFrames(curAttack.GetTotalFrames())) {
            EndAttack();
            return; 
        }
        
        // Otherwise attack each enemy
        for (int i = 0; i < targets.Count; i++) { Attack(targets[i], false); }
    }

    // Performs a projectile attack and returns true when it's over
    private bool projectileSpawned;
    protected void HandleProjectile(bool advanceTime = true)
    {
        if (advanceTime && atkTimer.WaitForXFrames(curAttack.GetTotalFrames())) {
            EndAttack();
            return; 
        }

        // Instantiate the projectile
        if (!projectileSpawned && curAttack.IsActive(atkTimer.CurFrame()))
        {
            projectileSpawned = true;
            
            // Make the projectile
            Projectile projectile = curAttack.projectile;
            if (projectile)
            {
                var direction = Mathf.Sign(transform.localScale.x);
                Vector2 scale = projectile.transform.localScale;
                scale.x = Mathf.Abs(scale.x) * direction;
                projectile.transform.localScale = scale;
                Vector2 spawnPos = transform.position;
                var shift = projectile.shift;
                shift.x *= direction;
                print(shift);
                
                Instantiate(projectile, spawnPos + shift, Quaternion.identity);
            }
        }
    }

    // Apply changes to data when an attack ends
    protected void EndAttack() {
        atkTimer.ResetTimer();
        ChangeState(prevState);
        curAttack = null;
    }
    
    // Setup variables for attacking
    protected void BeginAttack(Attack desiredAttack) {
        atkTimer.ResetTimer();
        ChangeState(States.Attack);
        curAttack = desiredAttack;
        hitList.Clear();
        hasHitTarget = false;
        projectileSpawned = false;
        _animator.SetTrigger(curAttack.animationTrigger);
    }
    
    #endregion
    
    #region Public Functions
    
    // Return the entity's hurtbox
    public Hitbox GetHurtBox() { return hurtbox; }
    
    #endregion

    #region Small Helper Functions
    
    // Change player's current state and store it
    protected void ChangeState(States s) {
        prevState = curState;
        curState = s;
    }

    // Initiate jump
    protected virtual void Jump(float jumpHeight, float airSpeed)
    {
        isGrounded = false;
        velocity.y = Mathf.Abs(jumpHeight) * 0.2f;
        if (Mathf.Abs(velocity.x) < Mathf.Epsilon) return;
        velocity.x = Mathf.Sign(velocity.x) * airSpeed;
    }
    
    // Remove given health from entity
    protected void TakeDamage(float damage) {
        curHP -= damage;
        if (curHP < 0) curHP = 0;
    }
    
    // Adds iframes to the entity if it's more than the remaining iframe time
    protected void AddIframes(int amount)
    {
        if (iFrameTimer.CurFrame() >= amount) return;
        iframes = amount;
        iFrameTimer.ResetTimer();
    }

    // Update the iFrames of an entity
    protected Timer iFrameTimer = new Timer();
    protected void UpdateIFrames()
    {
        if (iframes <= 0) return;
        if (iFrameTimer.WaitForXFrames(iframes))
        {
            iframes = 0;
            shader.RevertMaterial();
        }
    }

    #endregion
}
