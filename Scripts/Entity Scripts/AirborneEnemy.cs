using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class AirborneEnemy : BaseEnemy
{
    private Path path;
    private Seeker seeker;
    private int currentWayPoint = 1;
    private bool reachedEndOfPath;
    private float nextWaypointDistance;
    private bool updatePath;

    protected Vector2 movement, prevMove;
    private Transform target => dungeon.GetActivePlayer().transform;
    //public Transform target;
    
    // Start is called before the first frame update
    protected new void Start() 
    {
        base.Start();

        seeker = GetComponent<Seeker>();
        InvokeRepeating("UpdatePath",0f, 0.5f);
    }
    
    // Allows inherited scripts to initialize data easier
    protected void InitData(float detectionRadius, float waypointDist = 0.25f)
    {
        detectRadius = detectionRadius;
        nextWaypointDistance = waypointDist;
    }
    
    #region A Star Functions
    
    // Move towards the player
    protected void TrailPlayer(float speed)
    {
        if (path == null) return;
        
        reachedEndOfPath = currentWayPoint >= path.vectorPath.Count;
        if (reachedEndOfPath) return;

        Vector2 pos = transform.position;
        Vector2 direction = ((Vector2) path.vectorPath[currentWayPoint] - pos).normalized;
        movement = direction * (speed * Time.deltaTime);

        if (updatingPath)
        {
            movement = prevMove;
            updatingPath = false;
        }
        
        Move(movement);

        float distance = Vector2.Distance(pos, path.vectorPath[currentWayPoint]);
        if (distance < nextWaypointDistance)
        {
            currentWayPoint++;
        }
    }

    // Allow inherited scripts to prevent path searching 
    protected void TogglePathUpdate(bool enable) { updatePath = enable; }
    
    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWayPoint = 1;
        }
    }

    private bool updatingPath;
    private void UpdatePath()
    {
        if (!updatePath) return;
        updatingPath = true;

        if (seeker.IsDone())
        {
            Vector2 targetPos = target.position;
            targetPos += constants.centerPlayerShift;
            seeker.StartPath(rb.position, targetPos, OnPathComplete);
        }
    }


    #endregion
}
