using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSpirit : AirborneEnemy
{
    /*
     * TODO: Stays idle until player is detected. Plays an alerted animation then starts trailing
     * Activate idle/trailing state based on player distance
     */
    public LightSpiritConfig config;
    private bool isFacingLeft;
    
    protected new void Start() 
    {
        base.Start();
        curHP = config.maxHP;
        
        TogglePathUpdate(true);
        InitData(config.detectionRadius, config.waypointDistance);
        isFacingLeft = Mathf.Sign(transform.localScale.x) > 0;
    }

    private bool hasWaited;
    protected override void HandleIdle()
    {
        if (playerDetected)
        {
            if (!hasWaited)
            {
                if (!timer.WaitForXFrames(config.reactionTime)) return;
                hasWaited = true;
            }

            TrailPlayer(config.moveSpeed);
        } 
    
        var wasDetected = playerDetected;
        playerDetected = DetectPlayer();
        if (wasDetected != playerDetected)
        {
            hasWaited = false;
            timer.ResetTimer();
            _animator.SetBool("isFollow", playerDetected);
        }
    }

    private bool DetectPlayer()
    {
        var playerPos = dungeon.GetActivePlayer().transform.position;
        var userPos = transform.position;
        
        var distance = Vector2.Distance(userPos, playerPos);
        var radius = playerDetected ? config.loseDetect : config.detectionRadius;
        
        if (distance < radius)
        {
            var xDifference = userPos.x - playerPos.x;
            if ((!isFacingLeft && xDifference > config.flipTolerance) ||
                (isFacingLeft && xDifference < -config.flipTolerance))
            {
                isFacingLeft = !isFacingLeft;
                _animator.SetTrigger("flip");
                FlipEnemy(xDifference);
            }
        }

        //return Mathf.Abs(distance) < radius;
        return distance < radius;
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
}
