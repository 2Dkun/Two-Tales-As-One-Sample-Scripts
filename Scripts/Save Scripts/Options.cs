using System.Collections;
using UnityEngine;

public enum key { UP, DOWN, LEFT, RIGHT, B1, B2, B3 };

[System.Serializable]
public class Options {

    // Volume Settings
    public int[] volume = new int[3]; 
    // Control Settings
    public KeyCode[] controls = new KeyCode[7]; 
    // Video Settings
    public int resolution = 2; 
    public bool fullscreen, particlesOn;

    // Constructor
    public Options() {
        volume[0] = 45;
        volume[1] = 25;
        volume[2] = 50;

        controls[0] = KeyCode.W;
        controls[1] = KeyCode.S;
        controls[2] = KeyCode.A;
        controls[3] = KeyCode.D;
        controls[4] = KeyCode.J;
        controls[5] = KeyCode.K;
        controls[6] = KeyCode.L;

        resolution = 1;
        particlesOn = true;
        fullscreen = true;
    }

    public void ChangeResolution(int change) {
        resolution += change + 4;
        resolution %= 4;
    }
    public string GetResolution() {
        switch (resolution) {
            case 0:  return "1600 x 900";
            case 1:  return "1280 x 720";
            case 2:  return "800 x 450";
            case 3:  return "640 x 360";
            default: return "";
        }
    }
    public void UpdateResolution() {
        switch (resolution) {
            case 0:     Screen.SetResolution(1600, 900, fullscreen);    break;
            case 1:     Screen.SetResolution(1280, 720, fullscreen);    break;
            case 2:     Screen.SetResolution(800, 450, fullscreen);     break;
            case 3:     Screen.SetResolution(640, 360, fullscreen);     break;
            default:                                                    break;
        }
        DataManager.SaveOptions();
    }
}