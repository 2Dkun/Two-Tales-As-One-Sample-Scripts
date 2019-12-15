using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Projectile : MonoBehaviour {

    // Game Data Reference
    public GameObject dungeonData;

    // Position variables
    public float xShift, yShift;
    public float xVel, yVel;
    //public HitBox[] hb;
    public HitBox hitBox;

    // Damage variables
    public int basePow, hitstun;
    // Animation variables
    public SpriteAnimator projAnim;
    // Other variables
    public int duration;

    public void Start(){
        transform.Translate(new Vector2(xShift * getDir(), yShift));
        Destroy(gameObject, duration);
    }

    // Update is called once per frame
    public void Update () {
        transform.Translate(new Vector2(xVel * getDir() * Time.deltaTime, yVel * Time.deltaTime));
        GetComponent<SpriteRenderer>().sprite = projAnim.PlayAnim();
    }

    // Return 1 for left, -1 for right
    private int getDir(){
        return (int) this.transform.localScale.z;
    }

    // Check if given hitbox connected with player hurtbox with adjusted positions
	public bool IsHitTarget(HitBox userHit, GameObject user, HitBox targetHurt, GameObject target) {
		// Shift user hitbox to position
		int flipScale = (int) (user.transform.localScale.x / (Mathf.Abs(user.transform.localScale.x))); 
		userHit.flipBox(flipScale);
		userHit.shiftBox(user.transform.localPosition.x, user.transform.localPosition.y);

		// Shift target hurtbox to position
		flipScale = (int) (target.transform.localScale.x / (Mathf.Abs(target.transform.localScale.x))); 
		targetHurt.flipBox(-flipScale);
		targetHurt.shiftBox(target.transform.localPosition.x, target.transform.localPosition.y);

		// See if the hitbox came inot contact with hurtbox
		return userHit.checkHit(targetHurt);
	}

}