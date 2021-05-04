using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SleeperSpirit : GroundedEnemy
{
    public GroundedEnemyConfig config;
    
    // Start is called before the first frame update
    protected new void Start()
    {
        base.Start();
        InitData(config.detectionRadius, config.castDistance);
        curHP = config.maxHP;
    }

    // Update is called once per frame
    protected override void HandleIdle()
    {
        base.HandleIdle();
        ApplyGravity(config.gravity);
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
