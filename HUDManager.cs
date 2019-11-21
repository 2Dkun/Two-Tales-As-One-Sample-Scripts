using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDManager : MonoBehaviour {

	[System.Serializable]
	public class CharIcons {
		public Sprite bar;

		public Sprite idle;
		public Sprite idleS;
	}

	[System.Serializable]
	public class StatBars {
		public GameObject original, shadow;
		public Vector3 empty, full;
		FrameCounter timer;
	}

	public StatBars[] statBars;
	public GameObject bar, activeChar, swapChar;
	public CharIcons sword, shield;
	private CharIcons curChar;

	// Use this for initialization
	void Start () {
		curChar = sword;
		UpdateDisplay();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	// Updates the overall status HUD
	private void UpdateDisplay() {
		bar.GetComponent<SpriteRenderer>().sprite = curChar.bar;

		// Allow for an arg in function to change face later
		activeChar.GetComponent<SpriteRenderer>().sprite = curChar.idle;
		swapChar.GetComponent<SpriteRenderer>().sprite = curChar.idleS;
	}

	// Updates a given status bar
	private void UpdateStatBar(int bar, float percent) {
		// Get position of new Vector3 based on given percent
		Vector3 newPos = (statBars[bar].full - statBars[bar].empty) * percent;
		newPos += statBars[bar].empty;

		// Set the given status bar to that position
		statBars[bar].original.transform.localPosition = newPos;
	}

	/* ----- Functions to be called by other classes ----- */

	// Update corresponding status bar
	public void UpdateHP(float percent) { UpdateStatBar(0, percent); }
	public void UpdateMP(float percent) { UpdateStatBar(1, percent); }
	public void UpdateSP(float percent) { UpdateStatBar(2, percent); }

	// Changes the current character in status HUD
	public void SwapChar() {
		if(curChar == sword)	curChar = shield;
		else 					curChar = sword;

		UpdateDisplay();
	}

}
