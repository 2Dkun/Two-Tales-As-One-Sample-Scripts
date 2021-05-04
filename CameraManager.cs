using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    
    /*
     * TODO: https://www.youtube.com/watch?v=w4xM9EWKs3I
     * - [DONE] idle position with slight bias in front of player (done in TSF)
     * - [DONE???] upwards lerp and downwards snap
     * - [DONE] horizontal strong and weak lerps
     * - [DONE] camera bounds
     *     - done via clamps and box colliders
     */

    public CameraConfig config;
    public DungeonManager dungeon;
    private BasePlayer player;

    private List<Boundary> bounds = new List<Boundary>();
    private float initZPos;

    private void Start()
    {
        if (!dungeon) dungeon = FindObjectOfType<DungeonManager>();
        initZPos = transform.position.z;
    }

    public void UpdateCamera()
    {
        //transform.position = Vector3.Lerp(transform.position, GetTargetPosition(), curLerpSpeed * Time.deltaTime);
        var curPos = transform.position;
        var targetPos = GetTargetPosition();
        var xLerpSpeed = player.IsMoving() ? config.fastLerp : config.slowLerp;
        var yLerpSpeed = targetPos.y - curPos.y > 0 ? config.riseLerp : config.fallLerp;
        curPos.x = Mathf.Lerp(curPos.x, targetPos.x, xLerpSpeed * Time.deltaTime);
        curPos.y = Mathf.Lerp(curPos.y, targetPos.y, yLerpSpeed * Time.deltaTime);
        //if (targetPos.y - curPos.y <= 0) curPos.y = targetPos.y;
        transform.position = curPos;
    }

    // Returns the target position of the camera
    private Vector3 GetTargetPosition()
    {
        var playerTransform = player.transform;
        Vector3 playerPos = config.playerBias;
        playerPos.x *= Mathf.Sign(playerTransform.localScale.x); 
        playerPos += playerTransform.position;
        return BoundCamera(playerPos);
    }

    // Bounds the camera based on level design
    private Vector3 BoundCamera(Vector3 curTarget)
    {
        var minX = curTarget.x;
        var maxX = curTarget.x;
        var minY = curTarget.y;
        var maxY = curTarget.y;

        for (int i = 0; i < bounds.Count; i++)
        {
            if (bounds[i].isXBound)
            {
                if (bounds[i].isMaxBound) maxX = Mathf.Min(bounds[i].boundValue, maxX);
                else minX = Mathf.Max(bounds[i].boundValue, minX);
            }
            else
            {
                if (bounds[i].isMaxBound) maxY = Mathf.Min(bounds[i].boundValue, maxY);
                else minY = Mathf.Max(bounds[i].boundValue, minY);
            }
        }

        curTarget.x = Mathf.Clamp(curTarget.x, minX, maxX);
        curTarget.y = Mathf.Clamp(curTarget.y, minY, maxY);
        curTarget.z = initZPos;
        return curTarget;
    }
    
    // Add new camera bounds
    public void PushBounds (Boundary[] b)
    { for (int i = 0; i < b.Length; i++) bounds.Add(b[i]); }

    // Remove from camera bounds
    public void PopBounds(Boundary[] b)
    { for (int i = 0; i < b.Length; i++) bounds.Remove(b[i]); }

    // Update the current player
    public void UpdatePlayer(BasePlayer basePlayer)
    { player = basePlayer; }
}