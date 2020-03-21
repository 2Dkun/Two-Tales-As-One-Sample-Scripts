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
    public int getDuration()    { return (int)(anim.Length * FPS); }
    public int getLength() 		{ return anim.Length; }
    public float getFrame() 	{ return curFrame; }
    public Sprite[] getSprite() { return anim; }
    public float getFPS()		{ return FPS; }
	public bool isDone()		{ return curFrame == 0; }


	// ---Manipulation Procedures---

	// Returns the sprite that should be playing in the animation
    public Sprite PlayAnim(){
        // Find which frame should be shown based on real time
        curFrame += Time.deltaTime;
        int index = (int)(curFrame * FPS);
        // If the animation is still going, play it
        if (index < anim.Length) {
            return anim[index];
        }
        // Else, attack is done being displayed
        else {
            //ResetAnim();
            if (isLoop) {
                ResetAnim();
                return anim[index % anim.Length];
            }
            return anim[anim.Length - 1];
        }
    }

	// Reset the current animation to its original state
    public void ResetAnim(){
        this.curFrame = 0;
        return;
    }


}
