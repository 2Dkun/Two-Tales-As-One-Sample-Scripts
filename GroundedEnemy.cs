using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Holds general grounded movement logic
public abstract class GroundedEnemy : BaseEnemy
{
    private float castLength;
    protected Vector2 shift;

    // Allows inherited scripts to initialize data easier
    protected void InitData(float detectionRadius = 0, float castDistance = 0.5f)
    {
        castLength = castDistance;
        detectRadius = detectionRadius;

        var offset = box.offset;
        var size = box.size;
        shift.x = (offset + size * 0.5f).x;
        shift.y = (offset - size * 0.5f).y;
        shift *= transform.localScale;
    }

    // Move the entity horizontally
    protected bool MoveForward(float moveSpeed)  { return MoveHorizontally(moveSpeed, transform.localScale.x); }
    protected bool MoveBackward(float moveSpeed) { return MoveHorizontally(moveSpeed, -transform.localScale.x); }
    protected bool MoveHorizontally(float moveSpeed, float direction, float castLen = -1)
    {
        var dir = Mathf.Sign(direction);
        castLen = castLen < 0 ? castLength : castLen;
        if (!CheckPatrolDirection(castLen, dir)) return false;
        
        Move(Vector2.right * (dir * moveSpeed * Time.deltaTime));
        return true;
    }

    // Returns true if the enemy can continue moving towards the given direction
    protected bool CheckPatrolDirection(float castDistance, float currentDirection)
    {
        Vector2 direction = Vector2.down;
        direction.x = Mathf.Sign(currentDirection);

        Vector2 position = transform.position;
        shift.x = Mathf.Abs(shift.x) * Mathf.Sign(currentDirection);
        position += shift;

        var checkGround = Physics2D.Raycast(position, direction, castDistance, layerMask);
        var checkWall = Physics2D.Raycast(position, direction * Vector2.right, castDistance, layerMask);
        
        Debug.DrawLine(position, position + (castDistance * direction));
        //print(checkWall.collider == false);

        return checkGround.collider == true && checkWall.collider == false;
    }
}
