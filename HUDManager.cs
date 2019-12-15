using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDManager : MonoBehaviour {

	private enum face {IDLE, HURT};

	[System.Serializable]
	public class CharIcons {
		public Sprite bar;

		public Sprite[] main;
		public Sprite[] mini;
	}

	[System.Serializable]
	public class StatBars {
		public GameObject original, shadow;
		public Vector3 empty, full;
		public bool barChanged;
		public FrameCounter timer;
	}

	public StatBars[] statBars;
	public GameObject bar, activeChar, swapChar;
	public CharIcons sword, shield;
	private CharIcons curChar;

	// Use this for initialization
	void Start () {
		curChar = sword;
		UpdateDisplay(face.IDLE);
	}
	
	// Update is called once per frame
	void Update () {
		UpdateShadows();
	}


	// Updates the position of trailing stat bars and player faces
	private void UpdateShadows() {
		for(int i = 0; i < statBars.Length; i++) {
			Vector3 shadowPos = statBars[i].shadow.transform.localPosition;
			Vector3 originalPos = statBars[i].original.transform.localPosition;

			if(statBars[i].barChanged) {
				// Wait for a moment before moving the shadow bar
				if(statBars[i].timer.WaitForXFrames(Constants.SHORT_DELAY)){
					statBars[i].barChanged = false;

					// Change face display back to normal if health was lost
					if(i == 0) { // i = 0 if it was HP bar
						UpdateDisplay(face.IDLE);
					}
				}
			}
			else if(!shadowPos.Equals(originalPos)) {
				// Gradually move shadow bar to original
				statBars[i].shadow.transform.localPosition = 
					Vector3.MoveTowards(shadowPos, originalPos, 1.2f * Time.deltaTime);
			}
		}
	}

	// Updates the overall status HUD
	private void UpdateDisplay(face newFace) {
		bar.GetComponent<SpriteRenderer>().sprite = curChar.bar;

		// Update face display based on given face
		activeChar.GetComponent<SpriteRenderer>().sprite = curChar.main[(int) newFace];
		swapChar.GetComponent<SpriteRenderer>().sprite = curChar.mini[(int) newFace];
	}

	// Updates a given status bar
	private void UpdateStatBar(int bar, float percent) {
		// Get position of new Vector3 based on given percent
		Vector3 newPos = (statBars[bar].full - statBars[bar].empty) * percent;
		newPos += statBars[bar].empty;

		// Move shadow bar to old position
		//statBars[bar].shadow.transform.localPosition = statBars[bar].original.transform.localPosition;
		// Set the given status bar to that position
		statBars[bar].original.transform.localPosition = newPos;
		statBars[bar].barChanged = true;
		statBars[bar].timer.resetWait();
	}

	/* ----- Functions to be called by other classes ----- */

	// Update corresponding status bar
	public void UpdateHP(float percent) { UpdateStatBar(0, percent); UpdateDisplay(face.HURT); }// FIX LATER SO THAT THIS DOESNT HAPPEN WHEN YOU HEAL}
	public void UpdateMP(float percent) { UpdateStatBar(1, percent); }
	public void UpdateSP(float percent) { UpdateStatBar(2, percent); }

	// Changes the current character in status HUD
	public void SwapChar() {
		if(curChar == sword)	curChar = shield;
		else 					curChar = sword;

		UpdateDisplay(face.IDLE);
	}

}
