using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour {
  /*
    
    public static bool leftInput => Input.GetKey(KeyCode.A); //Input.GetAxis("Horizontal") < 0;
    public static bool rightInput => Input.GetKey(KeyCode.D);//Input.GetAxis("Horizontal") > 0;
    */
    //public static bool isMoving => PlayerInput.left.isHeld || PlayerInput.right.isHeld;

    public static bool isMoving => Input.GetAxisRaw("Horizontal") != 0;
    public static Vector2 moveInput => new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    
    // To deal with snapback
    //private static Vector2 CurrentInputValues => new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    //private static Vector3 PersistentInput; // this is the "accumulator"
    //public static Vector2 moveInput => PersistentInput = Vector2.Lerp(PersistentInput, CurrentInputValues, 25 * Time.deltaTime);

    public static ButtonInput up = new ButtonInput(KeyCode.W, KeyCode.UpArrow);
    public static ButtonInput down = new ButtonInput(KeyCode.J, KeyCode.DownArrow);
    public static ButtonInput left = new ButtonInput(KeyCode.A, KeyCode.LeftArrow);
    public static ButtonInput right = new ButtonInput(KeyCode.D, KeyCode.RightArrow);
    public static ButtonInput jump = new ButtonInput(KeyCode.Space, KeyCode.Joystick1Button0);
    public static ButtonInput keepDir = new ButtonInput(KeyCode.LeftShift, KeyCode.RightShift);
    
    public static ButtonInput confirm = new ButtonInput(KeyCode.Joystick1Button1, KeyCode.C);
    public static ButtonInput cancel = new ButtonInput(KeyCode.Joystick1Button2, KeyCode.V);
    public static ButtonInput dash = new ButtonInput(KeyCode.Joystick1Button3, KeyCode.Z);
    public static ButtonInput swap = new ButtonInput(KeyCode.Joystick1Button4, KeyCode.X);
    public static ButtonInput summon = new ButtonInput(KeyCode.LeftShift, KeyCode.RightShift);
    public static ButtonInput escapeMenu = new ButtonInput(KeyCode.Escape);

    public struct ButtonInput {
        private KeyCode key;
        private KeyCode key2; // Optional key for now
        private float startTime;
        private bool beganTimer;
        
        public ButtonInput(KeyCode k) {
            beganTimer = false;
            startTime = 0;
            key = k;
            key2 = k;
        }
        public ButtonInput(KeyCode k, KeyCode j) { 
            beganTimer = false;
            startTime = 0;
            key = k;
            key2 = j;
        }

        public void BeginTimer() {
            beganTimer = true;
            startTime = Time.time;
        }

        public void EndTimer() { beganTimer = false; }
        public float holdDuration => beganTimer ? Time.time - startTime : 0;
        

        public bool isHeld => Input.GetKey(key) || Input.GetKey(key2);
        public bool isPressed => Input.GetKeyDown(key) || Input.GetKeyDown(key2);
        public bool isReleased => Input.GetKeyUp(key) || Input.GetKeyUp(key2);
    }


    /*
    public Vector2 movementInput { get; private set; }
    public bool jumpInput { get; private set; }

    private void Update() {
        movementInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        jumpInput = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W);
    }
    */
    
}
