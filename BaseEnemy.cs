using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseEnemy : BaseEntity
{
    protected readonly Timer hurtTimer = new Timer();
    
    // Variables for pierce meter
    protected readonly Timer pierceTimer = new Timer();
    protected float pierceHP = 30;
    protected float soulDrop = 34;
    public bool IsPierced => pierceHP < 0;

    // Other Variables
    protected float detectRadius;
    protected bool isHurt, playerDetected;
    protected BasePlayer player => dungeon.GetActivePlayer();
    protected LayerMask baseLayerMask;

    #region Virtual Functions

    protected virtual void HandleIdle() { }
    protected virtual void HandleAttack() { Attack(dungeon.GetActivePlayer()); }

    protected virtual void HandleKo()
    {
        // play KO anim
        // TODO: placeholder ko animation
        float curAlpha = GetComponent<SpriteRenderer>().color.a - Time.deltaTime;
        if (curAlpha < 0)
        {
            curAlpha = 0;
            dungeon.RemoveObstacle(this);
            Destroy(gameObject);
        }
        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, curAlpha);
        
        box.enabled = false;
    }

    #endregion

    protected new void Start()
    {
        base.Start();
        baseLayerMask = layerMask;
        UpdateLayerMask(LayerMask.NameToLayer("EnemyMovement"));
    }

    public override void UpdateEntity()
    {
        CheckHurt();
        switch(curState){
            case States.Idle:      HandleIdle(); break;
            case States.Attack:    HandleAttack(); break;
            case States.KO:        HandleKo(); break;
            default: 			   print(gameObject.name + ": " + curState); break;
        }
        UpdatePierce();
        //print(curState);
    }

    protected void DetectPlayer(bool isCircle)
    {
        Hitbox fov;
        if (isCircle)   fov = new Hitbox(detectRadius, detectRadius, -detectRadius, -detectRadius);
        else            fov = new Hitbox(detectRadius, 1, -detectRadius, -1);
        playerDetected = IsHitTarget(fov, gameObject, player.GetHurtBox(), player.gameObject);
    }
    
    protected bool RayCastPlayer(Vector2 direction, float distance)
    {
        Vector2 position = transform.position;
        var offset = box.offset;
        offset.x = Mathf.Abs(offset.x) * Mathf.Sign(direction.x);
        position += offset;

        RaycastHit2D p = Physics2D.Raycast(position, direction, distance, baseLayerMask);
        Debug.DrawLine(position, position + (distance * direction), Color.magenta);
        
        return p && p.collider.CompareTag("Player");
    }


    public override void Attacked(float damage, Vector3 knockback, bool wasTipper)
    {
        if (!LinkAttacked(damage, knockback, wasTipper)) return;
        dungeon.player.GetActivePlayer().GiveSoul(soulDrop);
    }
    
    public bool LinkAttacked(float damage, Vector3 knockback, bool wasTipper) 
    {
        if (curState == States.KO || damage <= 0) return false;
        
        isHurt = true;
        hurtTimer.ResetTimer();
        
        TakeDamage(damage);
        if (wasTipper) pierceHP -= damage;
        // Apply knockback data
        shader.FlashWhite();
        velocity = knockback;
        if(curHP <= 0) ChangeState(States.KO);
        
        // TODO: dungeon.ApplyHitlag();
        return true;
    }
    
    #region Small Helper Functions

    private void UpdatePierce()
    {
        if (IsPierced)
        {
            // temp value maybe dependent on enemy or constant
            if (pierceTimer.WaitForXFrames(33)) 
            {
                // variable value for default pierce hp 
                pierceHP = 30; 
            }
        }
    }

    // Applies knockback to the enemy if they've been hit
    protected void CheckHurt()
    {
        if (!isHurt) return;
        isHurt = !hurtTimer.WaitForXFrames(constants.hurtTime);
        ApplyForce(new Vector2(100, 100));
    }    

    #endregion
    
    #region Simple Movement Functions

    protected void FlipEnemy()
    {
        var curScale = Vector3.one * flipScale;
        curScale.x *= Mathf.Sign(-transform.localScale.x);
        transform.localScale = curScale;
    }

    protected void FlipEnemy(float direction)
    {
        var curScale = Vector3.one * flipScale;
        curScale.x *= Mathf.Sign(direction);
        transform.localScale = curScale;
    }

    protected void MoveToPoint(Vector2 target, float speed)
    {
        Vector2 curPos = transform.position;
        Vector2 normal = (target - curPos).normalized;
        Move(normal * (speed * Time.deltaTime));
    }

    #endregion

    // Hurt the player if they came into contact with the enemy
    private void OnTriggerStay2D(Collider2D other)
    {
        if (curHP <= 0) return;
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<BasePlayer>().Attacked(1, constants.hurtKnockback, false);
        }
    }
}
