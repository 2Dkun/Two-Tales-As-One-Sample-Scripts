using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Attack {

    // Public Data
	public SpriteAnimator anim;
	public HitBox hitBox;
	public int startup, linger, endlag;
	public int power, cooldown;
	public float xVel, yVel;
	public bool isParry;
	public int mpCost;
	public GameObject prefab; // change to projectile later
    [HideInInspector]
	public FrameCounter timer;
	// Other Data
	private int curCooldown = 0;

    // Functions to be called by other scripts
	public int GetLastFrame() { return startup + linger; }
	public void BeginCooldown() { curCooldown = Time.frameCount; }
	public bool InCooldown() { return Time.frameCount - curCooldown < cooldown; }
}
