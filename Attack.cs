using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Attack {

	public SpriteAnimator anim;
	public HitBox hitBox;
	public int startup, linger, endlag;
	public int power, cooldown;
	public bool isParry;
	public int mpCost;

	public int getLastFrame() { return startup + linger; }

}
