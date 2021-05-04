using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : BasePlayer {

    private new void Start() {
        base.Start();
        if (!other) other = FindObjectOfType<Sword>();
    }

    public override void UpdateEntity()
    {
        base.UpdateEntity();

        // TODO: REMOVE ME
        if (isSpirit) { _animator.SetBool("inAir", true); return; }
        _animator.SetBool("isRun", PlayerInput.isMoving);
        _animator.SetBool("inAir", !isGrounded);

        // TODO: REMOVE ME
        // TEST
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Attacked(1, Vector3.back, false);
        }
        
        // Allow Shield to spotdodge cancel attacks
        if (curState == States.Special && PlayerInput.confirm.isPressed)
        {
            curState = prevState;
            shader.RevertMaterial();
            DoAttack();
        }

        // Allow Shield to cancel attacks with spotdodge
        /*
        if (curState == States.Attack && InEndlag && PlayerInput.cancel.isPressed && isGrounded)
        {
            EndAttack();
            StartSpecial();
        }
        */
    }
    
    // Override to update data for airdodge
    protected override void Jump(float jumpHeight, float airSpeed)
    {
        base.Jump(jumpHeight, airSpeed);
        airdodgeAmt = 1;
    }
    
    // Override to adjust data for spotdodge mechanic
    public override void Attacked(float damage, Vector3 knockback, bool wasTipper) {
        // Reward player for spotdodging
        if (curState == States.Special && !isAirdodge)
        {
            canCounter = iframes > 0;
            if (canCounter) return;
        }

        // Handle attacked logic
        base.Attacked(damage, knockback, wasTipper);
    }

    private bool wasAirborne;
    protected override void HandleAttack()
    {
        // Let player move while move while in air
        if (!isGrounded && PlayerInput.isMoving)
        {
            wasAirborne = true;
            var dir = (int) Mathf.Sign(PlayerInput.moveInput.x) * playerData.airSpeed;
            airSpeed = dir >= 0 ? 
                Mathf.Min(dir, airSpeed + playerData.airAcceleration * Mathf.Sign(dir)): 
                Mathf.Max(dir, airSpeed + playerData.airAcceleration * Mathf.Sign(dir));
            Move(Vector2.right * (airSpeed * Time.deltaTime));
        }
        HandleJumpFloat();
        
        // End attack if landed on the ground
        if (wasAirborne && isGrounded)
        {
            wasAirborne = false;
            EndAttack();
            return;
        }

        ApplyGravity(playerData.gravity, playerData.maxFallSpeed);
        DecelerateX(100);
        
        base.HandleAttack();
    }

    /*
     * Notes:
     * - Needs a cooldown so you can't just spam spotdodge
     *     - Doesn't need endlag because of cooldown and positioning
     *     - Doesn't need cooldown if enemies are design to literally walk into you
     */

    #region Airdodge and Spotdodge Mechanic
    
    private int airdodgeAmt = 1;
    private bool isAirdodge, canCounter;
    protected override void StartSpecial()
    {
        // Don't use the special move if player is out of airdodges
        if (!isGrounded && airdodgeAmt <= 0) return;
        
        ChangeState(States.Special);
        timer.ResetTimer();

        wasActive = true;
        canCounter = false;
        velocity = Vector3.zero;
        isAirdodge = !isGrounded;
        AddIframes(playerData.special.startup + playerData.special.linger);

        shader.InvincibleShader();
    }

    private bool wasActive;

    protected override void HandleSpecial()
    {
        base.HandleSpecial();
        return;
        if (isAirdodge)
        {
            HandleAirdodge();
            /*
            if (PlayerInput.isMoving)
            {
                var dir = (int) Mathf.Sign(PlayerInput.moveInput.x) * playerData.airSpeed/3;
                var temp  = dir >= 0 ? 
                    Mathf.Min(dir, velocity.x + dir * Time.deltaTime * 30): 
                    Mathf.Max(dir, velocity.x + dir * Time.deltaTime * 30);

                if (Mathf.Abs(temp) > Mathf.Abs(velocity.x)) velocity.x = temp;
            }
			
            ApplyGravity(playerData.gravity, 1);
            DecelerateX(100);
            /*
            // Update the airdodge amount if still airborne
            if (curState == States.Grounded) return;
            airdodgeAmt -= 1;
            ChangeState(States.Airborne);
            */
        }

        // Allow the player to walk during the dodge
        else
        {
            if (PlayerInput.isMoving)
            {
                var dir = (int) Mathf.Sign(PlayerInput.moveInput.x);
                Move(Vector2.right * (dir * playerData.walkSpeed / 3 * Time.deltaTime));
            }
            ApplyGravity(playerData.gravity);

            // Check state of dodge
            if (timer.WaitForXFrames(playerData.special.GetTotalFrames()))
            {
                airdodgeAmt -= 1;
                ChangeState(States.Idle);
                if (canCounter) GiveSoul(100);
                // TODO: Play super swag counter particle animation here 
            }
            else if (wasActive && iframes <= 0)
            {
                wasActive = false;
                shader.RevertMaterial();
            }
        }
    }

    private void HandleAirdodge()
    {
        //ApplyForce(playerData.special.knockback);
        if (PlayerInput.isMoving)
        {
            var dir = (int) Mathf.Sign(PlayerInput.moveInput.x);
            Move(Vector2.right * (dir * playerData.walkSpeed / 3 * Time.deltaTime));
            ApplyGravity(playerData.gravity/4);
        }
        
        // End the airdodge if landed or timed out
        if (isGrounded || timer.WaitForXFrames(playerData.special.startup + playerData.special.linger))
        {
            timer.ResetTimer();
            shader.RevertMaterial();
            
            // Adjust new velocity
            /*
            velocity.x = Mathf.Sign(velocity.x) * playerData.airSpeed;
            if (velocity.y < 0) velocity.y = -playerData.airSpeed;
*/
            
            // Update the airdodge amount if still airborne
            if (!isGrounded) airdodgeAmt -= 1;
        }
    }

    #endregion


}
