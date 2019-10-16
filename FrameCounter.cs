using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FrameCounter {

	// A small function used to delay actions for a certain amount of time
    private int initialFrame;

	public FrameCounter() {
		this.initialFrame = 0;
	}

    public bool WaitForXFrames(int x){
        if (initialFrame == 0) {
            initialFrame = Time.frameCount;
        }
            
        if (Time.frameCount - this.initialFrame >= x) {
            this.initialFrame = 0;
            return true;
        }
        return false;
    }
    public void resetWait() { this.initialFrame = 0; return; }
    public int curFrame() {return Time.frameCount - this.initialFrame; }
}
