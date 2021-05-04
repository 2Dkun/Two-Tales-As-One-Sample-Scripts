using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Timer {
    
    // A small function used to delay actions for a certain amount of time
    private float _curTime;
    
    public Timer() {
        _curTime = 0;
    }

    public void AdvanceTime()
    {
        // Prevent frames from being skipped
        const float maxTime = 1/60f;
        if(Time.deltaTime > maxTime)    _curTime += maxTime;
        else                            _curTime += Time.deltaTime;
    }

    public bool WaitForXFrames(int x){
        AdvanceTime();

        if (!(_curTime * 60 >= x)) return false;
        _curTime = 0; return true;
    }
    
    public void ResetTimer() { _curTime = 0; }
    public int CurFrame() { return (int)(_curTime * 60); }
    
    /*
    private float _curTime;
    private float _elapsedTime => Time.time - _curTime;
    
    public Timer() {
        _curTime = 0;
    }

    public bool WaitForXFrames(int x)
    {
        if (_curTime == 0) _curTime = Time.time;
        if (CurFrame() < x) return false;
        ResetTimer();
        return true;
    }
    
    public void ResetTimer() { _curTime = 0; }
    public int CurFrame() { return (int)(_elapsedTime * 60); }
    */
}
