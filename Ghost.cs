using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost : MonoBehaviour {

    [System.Serializable]
    public class GhostAnims {
		public Sprite[] idle, move, swap, hurt;
	}

    // Character states
	private enum States { IDLE, SWAP, ATTACK, HURT };
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

    // Update is called once per frame
    void Update()
    {
        // Smoothly move the ghost towards the player
		Vector3 newPos = player.transform.TransformPoint(new Vector3(xShift, yShift, 0));
        transform.position = Vector3.SmoothDamp(transform.position, newPos, ref velocity, smoothTime);
        
        // Play animations
        switch(curState){
            case States.SWAP:
                GetComponent<SpriteRenderer>().sprite = curGhost.swap[0];
                float i = GetComponent<SpriteRenderer>().color.a - Time.deltaTime * 1.5f;
                if(i < 0) i = 0;
                GetComponent<SpriteRenderer>().color = new Color(1,1,1,i);
                break;
            case States.IDLE:
                // IDLE ANIMATION
                if(Vector3.Distance(velocity, Vector3.zero) <= idleVel) {
                    GetComponent<SpriteRenderer>().sprite = curGhost.idle[0];
                }
                //MOVE ANIMATION
                else{
                    GetComponent<SpriteRenderer>().sprite = curGhost.move[0];
                }
                break;
            default:
                break;
        }

        // Make sure ghost is facing player
        if(transform.localPosition.x > player.transform.localPosition.x) {
            transform.localScale = new Vector2(-flipScale, flipScale);
        }
        else {
            transform.localScale = new Vector2(flipScale, flipScale);
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
