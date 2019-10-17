using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpriteAnimator {

	// ---Fields---
	public Sprite[] anim; 	// Sprites for the animation
	public float FPS; 		// Holds how fast the animation should be shown
    public bool isLoop; 	// Determines if the animation loops 
    private float curFrame; // Used to keep track of how far the animation has gone


	// ---Constructor---
    public SpriteAnimator(Sprite[] anim, float FPS, bool isLoop){
        // Set animation variables
        this.anim = new Sprite[anim.Length];
        for (int i = 0; i < anim.Length; i++) {
            this.anim[i] = anim[i];
        }
        this.isLoop = isLoop;
        this.FPS = FPS;
        return;
    }


	// ---Access Functions---
    public int getLength() 		{ return this.anim.Length; }
    public float getFrame() 	{ return this.curFrame; }
    public Sprite[] getSprite() { return this.anim; }
    public float getFPS()		{ return this.FPS; }
	public bool isDone()		{ return this.curFrame == 0; }


	// ---Manipulation Procedures---

	// Returns the sprite that should be playing in the animation
    public Sprite PlayAnim(){
        // Find which frame should be shown based on real time
        this.curFrame += Time.deltaTime;
        int index = (int)(this.curFrame * this.FPS);
        // If the animation is still going, play it
        if (index < this.anim.Length) {
            return this.anim[index];
        }
        // Else, attack is done being displayed
        else {
            //ResetAnim();
            if (this.isLoop) {
                ResetAnim();
                return this.anim[index % this.anim.Length];
            }
            return this.anim[anim.Length - 1];
        }
    }

	// Reset the current animation to its original state
    public void ResetAnim(){
        this.curFrame = 0;
        return;
    }


}
