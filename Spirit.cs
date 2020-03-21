using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spirit : BasePlayer {

    // Spirit State variables
    private enum SpiritState { IDLE, SWAP, HURT };
    private SpiritState curStateS;

    // Main variables
    public float smoothTime = 0.3F;
    public float xShift, yShift;
    public float leniency, summonDist;

    // Other variables
    private MainPlayer player;
    private float baseAlpha;
    private Vector3 velocity = Vector3.zero;
    private bool inControl, controlRdy;
    private bool inForcedAction => inAttack || inBlock;
    private bool inAttack =>
        curState == State.ATTACK || curState == State.PARRY;
    private bool inBlock =>
        curState == State.BLOCK;

    // Initialize spirit data
    private new void Start() {
        base.Start();

        maxHP = 100; // Needs to be not 0 so player can take damage
        player = dungeon.GetMainPlayer();
        baseAlpha = GetComponent<SpriteRenderer>().color.a;
    }

    // Allow the spirit to act freely
    public override void ActFreely() {
        Entity[] foes = dungeon.GetActiveEnemies();

        // Handles spirit state when not being controlled
        if (!inControl) {
            if (inForcedAction) {
                if (inAttack)       Attack(curAttack, foes);
                else if (inBlock)   BlockSkill(false);
            }
            else { 
                switch (curStateS) {
                    case SpiritState.IDLE: ActIdle(); break;
                    case SpiritState.SWAP: ActSwap(); break;
                    case SpiritState.HURT: ActHurt(); break;
                    default: break;
                }
            }
        }
        // Summon the spirit by moving them in front of the main player
        else if (!controlRdy) {
            // Smoothly move the ghost towards the player
            Vector3 newPos = player.transform.TransformPoint(new Vector3(summonDist, 0, 0));
            transform.position = Vector3.SmoothDamp(transform.position, newPos, ref velocity, smoothTime);

            // Start moving if same ground level as player
            float heightDif = transform.localPosition.y - player.transform.localPosition.y;
            if (Mathf.Abs(heightDif) <= leniency) {
                controlRdy = true;
                dungeon.UpdateCamera(gameObject);
                Vector3 tempPos = transform.position;
                tempPos.y = player.transform.position.y;
                transform.position = tempPos;
                SummonSpirit();
            }
        }
        // Allow the spirit to freely act
        else {
            switch (curState) {
                case State.GROUNDED:    MoveGrounded(); break;
                case State.AIRBORNE:    MoveAirborne(); break;
                case State.SWAP:        SwapPlayers(); break;
                case State.ATTACK:      Attack(curAttack, foes); break;
                case State.BLOCK:       BlockSkill(true); break;
                case State.PARRY:       Attack(curAttack, foes); break;
                default:                Debug.Log("Bad State: " + curState); break;
            }
        }
    }

    #region Virtual Function Implmentation
    // Break control if ghost is attacked
    protected override void AttackedChanges() {
        if (inControl || inForcedAction) {
            player.HurtMainPlayer(maxHP - curHP);

            player.UndoControl();
            curStateS = SpiritState.HURT;
        }
        curHP = maxHP;
    }
    #endregion

    #region Private Helper Functions
    // Spirit will move towards player
    private void MoveToPlayer() {
        // Smoothly move the ghost towards the player
        Vector3 newPos = player.transform.TransformPoint(new Vector3(xShift, yShift, 0));
        transform.position = Vector3.SmoothDamp(transform.position, newPos, ref velocity, smoothTime);

        // Make sure ghost is facing player
        if (transform.localPosition.x > player.transform.localPosition.x) {
            transform.localScale = new Vector2(-flipScale, flipScale);
        }
        else {
            transform.localScale = new Vector2(flipScale, flipScale);
        }
    }

    // Init all variables for the spirit to start moving
    private void SummonSpirit() {
        if (!inForcedAction) { ChangeState(State.GROUNDED); }
        // Set class stats for air movement
        gravity = curClass.gravity;
        fallSpd = curClass.fallSpd;
        curSP = 100;
    }

    // Try to swap with the main player
    private void SwapPlayers() {
        player.UndoControl();
        player.SpiritSwap();
    }
    #endregion

    #region Core Functions
    // Determines how ghost acts in idle state
    private void ActIdle() {
        MoveToPlayer();

        Vector3 newPos = player.transform.TransformPoint(new Vector3(xShift, yShift, 0));
        // IDLE ANIMATION
        if (Vector3.Distance(transform.position, newPos) <= leniency) {
            GetComponent<SpriteRenderer>().sprite = curClass.charAnims.idle.PlayAnim();
        }
        //MOVE ANIMATION
        else {
            GetComponent<SpriteRenderer>().sprite = curClass.charAnims.walk.PlayAnim();
        }
    }

    // Determines how ghost acts in act state
    private void ActSwap() {
        MoveToPlayer();
        // Do the swap
        GetComponent<SpriteRenderer>().sprite = curClass.charAnims.swap.PlayAnim();
        float i = GetComponent<SpriteRenderer>().color.a - Time.deltaTime * 1.5f;
        if (i < 0) i = 0;
        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, i);
    }

    // Display that the spirit is hurt
    private void ActHurt() {
        // Return the previous state once out of hitstun
        if (timer.WaitForXFrames(Constants.HURT_TIME)) {
            curStateS = SpiritState.IDLE;
        }
        // Otherwise play hitstun animation
        else {
            if (timer.CurFrame() % 3 == 1) {
                GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 1);
            }
            else {
                GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, baseAlpha);
            }
        }
    }
    #endregion

    #region Public Functions
    // Functions to handle swapping
    public void Swap() { curStateS = SpiritState.SWAP; }
    public void SwapChar() {
        curClass = player.GetSpiritClass();

        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, baseAlpha);
        curStateS = SpiritState.IDLE;
    }

    // Toggle control of spirit
    public void SetControl(bool isCtrl) {
        inControl = isCtrl;
        if (!isCtrl) controlRdy = false;
    }

    // Set spirit class
    public void SetSpiritClass(PlayerClass p) {
        curClass = p;
    }
    #endregion
}
