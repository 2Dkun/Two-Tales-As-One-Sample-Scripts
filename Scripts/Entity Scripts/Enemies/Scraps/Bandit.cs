using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * NOTES:
 * - Separate actions based on positioning
 * - Special stun state after being shield broken
 * - Maybe move the actual gameobject forward during animation instead of code? (make sure it cant go thru walls)
 *
 * Required Data:
 * - hurtbox
 * - positions to detect when player is cornered
 * - walk speed
 *
 * States:
 * - (hurt?) More of a passive than an actual state
 * - Attack
 * - Grounded
 * - Defend? (Block stun during grounded phase)
 * - Guard Break
 *
 * TODO:
 * 1) Put the character into blockstun when blocking an attack
 * 2) Flip the character when they are attacked behind them (you can prob avoid depending on AI)
 * 3) Put the character into shield break when guard is broken (same logic as step 1)
 * V --- Current Progress --- V
 * 4) Implement the dagger attack. Bandit will stand still for some time before moving forward to strike.
 * 5) Move Bandit closer to player until he's just at the right spacing for the attack.
 * *Refer to the AI behind Hornet for this. Figure out why her AI is different from this if it is*
 * 6) Handle AI movement when the opponent is cornered. Bandit should be backing off, but not too far.
 * 7) Program the absolute block state where a non-tipper will result in counterhit.
 * 
 */


public class Bandit : BaseEnemy
{
    public BanditConfig config;

    private bool inBlockstun;
    
    protected new void Start() 
    {
        base.Start();
        
        hurtbox = config.hurtbox;
        ChangeState(States.Idle);
    }

    private void Update()
    {
        CheckHurt();
        switch(curState)
        {
            case States.Idle:	HandleGrounded(); break;
            //case States.Hurt:       ApplyHitstun(); break;
            default: 				print("fuck"); break;
        }

        //UpdateHurtbox();
        //hurtbox.DrawBox(Color.cyan);
        
        print(curState);
    }

    protected void HandleGrounded()
    {
        if (CheckBlockstun()) return;
    }

    public override void Attacked(float damage, Vector3 knockback, bool wasTipper) 
    {
        if (curState == States.Idle)//curState == States.Block || curState == States.Grounded) 
        {
            if (wasTipper) 
            {
                // TODO: Defense break state
            }
            else
            {
                inBlockstun = true;
                // TODO: Play block stun anim
                _animator.SetTrigger("block");
                print("called");
                return;
            }
        }
        
        base.Attacked(damage, knockback, wasTipper);
    }
    
    private bool CheckBlockstun()
    {
        if (!inBlockstun) return false;
        if (timer.WaitForXFrames(config.blockstun)) inBlockstun = false;
        return true;
    }
}

[CreateAssetMenu(fileName = "BanditData", menuName = "ScriptableObjects/BanditScriptableObject", order = 1)]
public class BanditConfig : ScriptableObject {

    public float maxHP;
    public Hitbox hurtbox;

    public int shieldBreakDuration, blockstun;
}

