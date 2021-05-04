using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * NOTES:
 * - Attacks are done in the following order:
 * 1) [DONE] pick an attack at a choice spot
 * 2) based on the attack, walk to the starting position
 * 3) preform the attack [call dedicated attack function]
 * 4) walk to a choice spot that isn't the current spot where the attack ended
 *
 * - How attacks are chosen:
 * - Pick an option in one of a few predetermined sets
 * 
 * - Extras:
 *     - Have checkpoints in fight where the boss takes a break to indicate progress
 */

public class Rival : GroundedEnemy
{
    public RivalConfig config;

    private enum Option { Spin, Beam, Thrust }
    private int direction;
    
    // Start is called before the first frame update
    protected new void Start()
    {
        base.Start();
        InitData(config.detectionRadius, config.castDistance);

        curHP = config.maxHP;
        
        //TODO: remove me
        curSet[curOpt] = Option.Thrust;
        InitOption();
    }
    
    protected override void HandleIdle()
    {
        base.HandleIdle();
        ApplyGravity(config.gravity);
        
        if (!MoveBackward(config.dashSpeed))
        {
            FlipEnemy();
            _animator.SetTrigger("flip");
        }

        return;
        
        // Preform the current attack
        switch (curPhase)
        {
            case AttackPhase.Start: HandleStart(); break;
            case AttackPhase.Attack: HandleAttack(); break;
            case AttackPhase.End: HandleEnd(); break;
        }
        print(curSet[curOpt] + ": " + curPhase);
    }
    
#region Deals With Enemy Movement

    private enum AttackPhase { Start, Attack, End }
    private AttackPhase curPhase;
    private float currentPositionTarget;
    
    // Possible target positions 
    private float quarter => config.choiceSpotPercent;
    private const float middle = 0.5f;
    private const float corner = 0.0f;

    // Sets the new target position of the boss
    private void SelectPosition(float positionPercent, bool isCloser)
    {
        var stage = config.rightCorner - config.leftCorner;
        var posA = (stage * positionPercent) + config.leftCorner;
        var posB = (stage * Mathf.Abs(1 - positionPercent)) + config.leftCorner;
        var curPos = transform.position;
        
        // Make sure you dont have a spot you're already on
        if (Mathf.Abs(curPos.x - posA) <= config.intervalSize) { currentPositionTarget = posB; return; } 
        if (Mathf.Abs(curPos.x - posB) <= config.intervalSize) { currentPositionTarget = posA; return; } 
        
        // Determine the closer position and choose accordingly
        bool isCloserA = Mathf.Abs(curPos.x - posA) <= Mathf.Abs(curPos.x - posB);
        if (isCloser) currentPositionTarget = isCloserA ? posA : posB;
        else currentPositionTarget = isCloserA ? posB : posA;
    }

    // Moves the boss closer to desired target
    private bool MoveToTarget(float moveSpeed)
    {
        var positionDifference = currentPositionTarget - transform.position.x;
        MoveHorizontally(moveSpeed, positionDifference);
        return Mathf.Abs(positionDifference) <= config.intervalSize;
    }

    // Walk to the starting position of the current attack
    private void HandleStart()
    {
        // Start the attack once boss has reached target position
        if (MoveToTarget(config.dashSpeed))
        {
            curPhase = AttackPhase.Attack;
            StartAttack();
        }
    }

    // Walk to the next choice spot and then pick a new option once there
    private void HandleEnd()
    {     
        if (MoveToTarget(config.moveSpeed))
        {
            GetNextOption();
            curPhase = AttackPhase.Start;
        }
    }

    // Flips boss to face the direction their headed
    private void FlipTowardsTarget()
    {
        FlipEnemy(currentPositionTarget - transform.position.x);
    }

    #endregion

#region Functions for Boss Attacks

    // Executes actions required to begin the current attack
    private void StartAttack()
    {
        switch (curSet[curOpt]) {
            case Option.Beam: StartBeam(); break;
            case Option.Spin: StartSpin(); break;
            case Option.Thrust: StartThrust(); break;
        }
    }

    protected override void HandleAttack()
    {
        switch (curSet[curOpt]) {
            case Option.Beam: HandleBeam(); break;
            case Option.Spin: HandleSpin(); break;
            case Option.Thrust: HandleThrust(); break;
        }
    }

    private void InitBeam() { SelectPosition(quarter, true); }
    private void InitSpin() { SelectPosition(quarter, true); }
    private void InitThrust() { SelectPosition(corner, true); }
    
    
    
#region Handle Spin

    private bool hasSpun;
    private void StartSpin()
    {
        hasSpun = false;
        SelectPosition(corner, false); 
    }

    private void HandleSpin()
    {
        if (hasSpun)
        {
            Attack(player);
            if (curState != States.Attack) { EndSpin(); }
        }
        if (!hasSpun)
        {
            var dir = Mathf.Sign(transform.localScale.x) * Vector2.right;
            if (RayCastPlayer(dir, config.spinDist))
            {
                hasSpun = true;
                BeginAttack(config.spin);
            }
            else if (MoveToTarget(config.dashSpeed)) { EndSpin(); }
        }
    }

    private void EndSpin()
    {
        curPhase = AttackPhase.End;
        SelectPosition(quarter, true);
    }

    #endregion

#region Handle Beam

    /*
    * 1) Backdash to the nearest corner
    *     - BeginAttack(beam) once there
    * 2) Execute beam attack (HandleProjectile)
    * 3) End attack
    */

    private bool atCorner;

    private void StartBeam()
    {
        atCorner = false;
        SelectPosition(corner, true); 
    }

    private void HandleBeam()
    {
        if (!atCorner)
        {
            if (MoveToTarget(config.beam.movement.x))
            {
                atCorner = true;
                BeginAttack(config.beam);
            }
        }

        else
        {
            HandleProjectile();
            
            // Walk to nearest choice spot once attack is done
            if (curState != States.Attack)
            {
                curPhase = AttackPhase.End;
                SelectPosition(quarter, true); 
            }
        }
    }
#endregion

#region Handle Thrust

    private void StartThrust()
    {
        SelectPosition(quarter, false); 
        FlipTowardsTarget();
        BeginAttack(config.thrust);
    }

    private void HandleThrust()
    {
        if (curState == States.Attack) Attack(player);
        if (MoveToTarget(config.thrust.movement.x))
        {
            EndAttack();
            SelectPosition(quarter, true);
            curPhase = AttackPhase.End;
        }
    }
    
#endregion
    
    
    
#endregion


#region Functions for Picking Options

    // Returns the current option that the Boss is using
    private Option CurrentOption() { return curSet[curOpt]; }

    // Select the next option, but pick a new pattern if previous one is done
    private void GetNextOption()
    {
        curOpt += 1;
        
        // Choose a new set
        if (curOpt >= maxOpt) {
            curOpt = 0;
            curPattern = (Pattern) Random.Range(1, System.Enum.GetValues(typeof(Pattern)).Length);
        }
        InitOption();
    }

    // Variables for holding options sets of the boss
    private enum Pattern { AGGRESSIVE, PATIENT, AVERAGE }
    private Pattern curPattern;

    private Option[] avg = { Option.Spin };
    private Option[] agg = { Option.Spin };
    private Option[] pat = { Option.Spin };
    
    // Variables for keeping track of boss options
    private Option[] curSet => CurSet();
    private int curOpt;
    private int maxOpt => curSet.Length;
    
    // Initialize variables of incoming option
    private void InitOption() {
        switch (curSet[curOpt]) {
            case Option.Beam: InitBeam(); break;
            case Option.Spin: InitSpin(); break;
            case Option.Thrust: InitThrust(); break;
        }
    }
    
    // Returns the current option set of the boss
    private Option[] CurSet() {
        switch (curPattern) {
            case Pattern.AVERAGE: 
                return avg;
            case Pattern.AGGRESSIVE: 
                return agg;
            case Pattern.PATIENT: 
                return pat;
            default: 
                return null;
        }
    }

#endregion
}