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


	void UpdateDisplay() {
		bar.GetComponent<SpriteRenderer>().sprite = curChar.bar;

		// Allow for an arg in function to change face later
		activeChar.GetComponent<SpriteRenderer>().sprite = curChar.idle;
		swapChar.GetComponent<SpriteRenderer>().sprite = curChar.idleS;
	}


	// Can be called by other classes
	void UpdateHP(/*give hp percent*/) {

	}

	public void SwapChar() {
		if(curChar == sword)	curChar = shield;
		else 					curChar = sword;

		UpdateDisplay();
	}

}
