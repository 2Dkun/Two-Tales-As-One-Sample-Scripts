using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost : MonoBehaviour {

      /*
        CONTROL GHOST PLAN:
        - No hurtbox, can't get hit
        - *** YOU NEED TO REORGANIZE CODE SO THAT GHOST CAN CALL ATTACK FUNCTION FROM PLAYER OR BASE CLASS ***
        -States:
            - Grounded/float?
            - Attack (same)
            - Skills?
            - Return
    */

    [System.Serializable]
    public class GhostAnims {
		public Sprite[] idle, move, swap, hurt;
	}

    // Character states
	private enum States { IDLE, SWAP, ATTACK, MOVE };
	private States curState { get; set; }
	private States prevState { get; set; }

    // Main variables
    public GhostAnims sword, shield;
    public GameObject player;
	public float smoothTime = 0.3F;
	public float xShift, yShift;
    public float idleVel;

    // Other variables
    private GhostAnims curGhost;
	private Vector3 velocity = Vector3.zero;
    private float flipScale;
    private float baseAlpha;

    // Start is called before the first frame update
    void Start()
    {
        curGhost = shield;
        flipScale = Mathf.Abs(player.transform.localScale.x);
        baseAlpha = GetComponent<SpriteRenderer>().color.a;
    }

    // Allows the ghost to act
    public void ActFree() {
        if(Input.GetKeyDown(KeyCode.LeftShift))
            curState = States.MOVE;
        else if(Input.GetKeyUp(KeyCode.LeftShift))
            curState = States.IDLE;

        switch(curState){
            case States.SWAP:   ActSwap();      break;
            case States.IDLE:   ActIdle();      break;
            case States.MOVE:   MoveGhost();    break; 
            default:                            break;
        }

    }
    

    /* PRIVATE FUNCTIONS TO MAKE THINGS MORE READABLE */

    // Ghost will move towards player
    private void MoveToPlayer() {
        // Smoothly move the ghost towards the player
		Vector3 newPos = player.transform.TransformPoint(new Vector3(xShift, yShift, 0));
        transform.position = Vector3.SmoothDamp(transform.position, newPos, ref velocity, smoothTime);

        // Make sure ghost is facing player
        if(transform.localPosition.x > player.transform.localPosition.x) {
            transform.localScale = new Vector2(-flipScale, flipScale);
        }
        else {
            transform.localScale = new Vector2(flipScale, flipScale);
        }
    }

    // Determines how ghost acts in act state
    private void ActSwap() {
        MoveToPlayer();
        // Do the swap
        GetComponent<SpriteRenderer>().sprite = curGhost.swap[0];
        float i = GetComponent<SpriteRenderer>().color.a - Time.deltaTime * 1.5f;
        if(i < 0) i = 0;
        GetComponent<SpriteRenderer>().color = new Color(1,1,1,i);
    }
    // Determines how ghost acts in idle state
    private void ActIdle() {
        MoveToPlayer();
        // IDLE ANIMATION
        if(Vector3.Distance(velocity, Vector3.zero) <= idleVel) {
            GetComponent<SpriteRenderer>().sprite = curGhost.idle[0];
        }
        //MOVE ANIMATION
        else{
           GetComponent<SpriteRenderer>().sprite = curGhost.move[0];
        }
    }


    private void MoveGhost() {
        Player.PlayerClass curClass = player.GetComponent<Player>().GetGhost();
        // Move horizontally
        if(Input.GetKey(KeyCode.A)) {
			GetComponent<SpriteRenderer>().sprite = curGhost.move[0];
			transform.localScale = new Vector2(-flipScale, flipScale);
			transform.Translate(-1 * curClass.walkSpd * Time.deltaTime, 0, 0);
		}
        else if(Input.GetKey(KeyCode.D)) {
            GetComponent<SpriteRenderer>().sprite = curGhost.move[0];
			transform.localScale = new Vector2(flipScale, flipScale);
			transform.Translate(curClass.walkSpd * Time.deltaTime, 0, 0);
        }
        // Move vertically
        if(Input.GetKey(KeyCode.S)) {
			GetComponent<SpriteRenderer>().sprite = curGhost.move[0];
			transform.Translate(0, -1 * curClass.walkSpd * Time.deltaTime, 0);
		}
        else if(Input.GetKey(KeyCode.W)) {
            GetComponent<SpriteRenderer>().sprite = curGhost.move[0];
			transform.Translate(0, curClass.walkSpd * Time.deltaTime, 0);
        }
    }
    

    // Functions to be called by other scripts 
    public void Swap() {
        curState = States.SWAP;
    }
    public void SwapChar() {
        if(curGhost == shield)      curGhost = sword;
        else                        curGhost = shield;

        GetComponent<SpriteRenderer>().color = new Color(1,1,1,baseAlpha);
        curState = States.IDLE;
    }
    public void Hurt() {}

}
