using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkerSpirit : GroundedEnemy
{

    /*
     * - Walks idle until it detects player
     *     - Check set distance forward each frame
     * - Slow startup charge "attack" until it hits a wall
     *     - Not an attack, just make it move forward after a set interval, then wait again after detecting wall
     *     - Turns around by the end of the attack and sees if player is still there, if not then idle again
     */
    
    public WalkerSpiritConfig config;
    
    protected new void Start()
    {
        base.Start();

        curHP = config.maxHP;
        InitData(config.detectionRadius, config.castDistance);
    }

    // Continuously patrol until the player is detected
    protected override void HandleIdle()
    {
        base.HandleIdle();
        
        ApplyGravity(config.gravity);
        if (!isGrounded) return;
        
        if (!MoveBackward(config.moveSpeed))
        {
            FlipEnemy();
            _animator.SetTrigger("flip");
        }
        
        // Check for player
        if (CheckPlayer())
        {
            inStartup = true;
            inEndlag = false;
            ChangeState(States.Attack);
            _animator.SetTrigger("charge");
        }
    }

    // Handles logic for charging attack
    private bool inStartup, inEndlag;
    protected override void HandleAttack()
    {
        ApplyGravity(config.gravity);
        
        if (inStartup)
        {
            inStartup = !timer.WaitForXFrames(config.startup);
        }
        else if (!inEndlag)
        {
            //inEndlag = !MoveBackward(config.chargeSpeed);
            inEndlag = !MoveHorizontally(config.chargeSpeed, -transform.localScale.x, config.chargeCastLength); 
            if (inEndlag) _animator.SetTrigger("break");
        }
        else if (timer.WaitForXFrames(config.endlag))
        {
            FlipEnemy();
            _animator.SetTrigger("flip");
            ChangeState(States.Idle);
        }
    }

    private bool initKO;
    protected override void HandleKo()
    {
        //ApplyGravity(config.gravity);
        if (initKO) return;
        initKO = true;
        box.enabled = false;
        _animator.SetTrigger("die");
    }

    // Checks if the player is in front of the enemy
    private bool CheckPlayer()
    {
        Vector2 pos = transform.position;
        pos += config.detectionShift;
        pos.x += shift.x;
        var dir = Mathf.Sign(-transform.localScale.x) * Vector2.right;
        var lm = Physics2D.GetLayerCollisionMask(LayerMask.NameToLayer("Enemy"));
        var checkPlayer = Physics2D.Raycast(pos, dir, config.detectionRadius, lm);
        
        Debug.DrawLine(pos, pos + (config.detectionRadius * dir));
        return checkPlayer == true && checkPlayer.collider.CompareTag("Player");
    }

    public override void Attacked(float damage, Vector3 knockback, bool wasTipper)
    {
        base.Attacked(damage, knockback, wasTipper);
        // Flip enemy in direction of attack
        if (curState == States.Idle) FlipEnemy(knockback.x);
    }
}
