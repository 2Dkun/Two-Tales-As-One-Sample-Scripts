using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class Sword : BasePlayer {
    
    private new void Start() {
        base.Start();
        if (!other) other = FindObjectOfType<Shield>();
    }

    public override void UpdateEntity()
    {
        base.UpdateEntity();
        
        if (isSpirit) { _animator.SetBool("isRun", false); return; }
       // _animator.SetBool("isTrail", PlayerInput.left.isHeld || PlayerInput.right.isHeld);
        _animator.SetBool("isRun", isGrounded && PlayerInput.isMoving);
        
        // Allow Sword to dodge cancel attacks
        if (curState == States.Special && PlayerInput.confirm.isPressed)
        {
            EndDash();
            DoAttack();
        }
        
        // Allow Sword to cancel attacks with dash
        /*
        if (curState == States.Attack && InEndlag && PlayerInput.cancel.isPressed && isGrounded)
        {
            EndAttack();
            StartSpecial();
        }
        */
    }

    protected override void StartSpecial()
    {
        if (!CheckDash(false)) return;
        curState = States.Special;
    }

    protected override void HandleSpecial()
    {
        if (!CheckDash(false))
        {
            ChangeState(States.Idle);
        }
    }
    
    protected override void HandleAttack()
    {
        if (isGrounded && !InEndlag) {
            var dir = (int) Mathf.Sign(transform.localScale.x) * playerData.basicAttack.movement.y;
            Move(Vector2.right * (dir * playerData.basicAttack.movement.x * Time.deltaTime));
        }
        // Movement
        else if (PlayerInput.isMoving) {
            var dir = (int) Mathf.Sign(PlayerInput.moveInput.x);
            Move(Vector2.right * (dir * playerData.walkSpeed * Time.deltaTime));
        }
        ApplyGravity(playerData.gravity, playerData.maxFallSpeed);
        
        base.HandleAttack();
    }

    /*
    private bool hasDashed;
    protected override void HandleAttack() 
    {
        if (prevState == States.Airborne) {
            if (PlayerInput.isMoving)
            {
                var dir = (int) Mathf.Sign(PlayerInput.moveInput.x) * playerData.airSpeed;
                var temp  = dir >= 0 ? 
                    Mathf.Min(dir, velocity.x + dir * Time.deltaTime * 30): 
                    Mathf.Max(dir, velocity.x + dir * Time.deltaTime * 30);

                if (Mathf.Abs(temp) > Mathf.Abs(velocity.x)) velocity.x = temp;
            }
			
            HandleJumpFloat();
            ApplyGravity(playerData.gravity, 1);
            DecelerateX(100);
        }
        else
        {
            if (PlayerInput.isMoving)
            {
                hasDashed = true;
            }
        }
        
        Attack(dungeon.GetActiveEnemies());
        if (curState != States.Attack)
        {
            hasDashed = false;
        }
    }
    */
    
    /*
     * NOTES:
     * - dash attack that bounces back on hit
     * - alternative option to thrust downwards later on
     */
    /*
    #region Thrust Mechanic

    private bool isCharging;
    
    protected override void StartSpecial()
    {
        CheckDash(false);
        timer.ResetTimer();

        isCharging = true;
        velocity = new Vector3(Mathf.Sign(transform.localScale.x) * playerData.dashSpeedF, 0, 0);
        BeginAttack(playerData.special); 
        curState = States.Special;
    }
    protected override void HandleSpecial()
    {
        if (isCharging && !InEndlag && HasStarted)
        {
            ApplyForce(new Vector2(0, 0));
        }
        else if(HasStarted)
        {
            
            if (!isCharging && PlayerInput.isMoving)
            {
                var dir = (int) Mathf.Sign(PlayerInput.moveInput.x) * playerData.airSpeed;
                var temp  = dir >= 0 ? 
                    Mathf.Min(dir, velocity.x + dir * Time.deltaTime * 30): 
                    Mathf.Max(dir, velocity.x + dir * Time.deltaTime * 30);

                if (Mathf.Abs(temp) > Mathf.Abs(velocity.x)) velocity.x = temp;
            }
			
            ApplyGravity(playerData.gravity, 1);
            DecelerateX(100);
            
            //print(velocity);
            if(!isCharging) ApplyForce(new Vector2(100, 0));
        }

        if (Attack(dungeon.GetActiveEnemies()))
        {
            isCharging = false;
            velocity = playerData.special.pushback;
        }
        
        if(isCharging && curState != States.Special) velocity = Vector3.zero;
    }
    
    #endregion
    */
}
