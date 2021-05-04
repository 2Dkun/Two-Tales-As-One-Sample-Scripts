using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsEntity : MonoBehaviour
{
    protected int layerMask;
    protected Rigidbody2D rb;
    protected BoxCollider2D box;
    protected Vector2 velocity;
    protected bool isGrounded;
    
    // Collision Variables
    private ContactFilter2D contactFilter;
    private RaycastHit2D[] hitBuffer = new RaycastHit2D[16];
    private const float collisionTolerance = 0.01f;

    private void OnEnable()
    {
        rb = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
    }
    
    protected void Awake ()
    {
        isGrounded = false;
        contactFilter.useTriggers = false;
        layerMask = Physics2D.GetLayerCollisionMask(gameObject.layer);
        contactFilter.SetLayerMask(layerMask);
        contactFilter.useLayerMask = true;
    }

    // Update collision layer for the entity
    protected void UpdateLayerMask(int layer)
    {
        layerMask = Physics2D.GetLayerCollisionMask(layer);
        contactFilter.SetLayerMask(layerMask);
    }

    // Move the entity's position and check for collision
    protected void Move(Vector2 movement)
    {
        MoveEntity(movement * Vector2.right);
        if(Mathf.Abs(movement.y) > 0) isGrounded = false;
        MoveEntity(movement * Vector2.up);
    }
  
    private void MoveEntity (Vector2 movement)
    {
        float distance = movement.magnitude;
        
        if (distance > 0)
        {
            int count = rb.Cast (movement, contactFilter, hitBuffer, distance + collisionTolerance);
            
            for (int i = 0; i < count; i++) 
            {
                Vector2 currentNormal = hitBuffer[i].normal;
                if (currentNormal.y > 0) isGrounded = true;
                
                float projection = Vector2.Dot (velocity, currentNormal);
                if (projection < 0) velocity -= projection * currentNormal;
                
                float modifiedDistance = hitBuffer[i].distance - collisionTolerance;
                distance = modifiedDistance < distance ? modifiedDistance : distance;
            }
        }

        rb.position += movement.normalized * distance;
    }
    
    // Apply gravity to the entity
    protected void ApplyGravity(float gravity, float maxFallSpeed = -1)
    {
        velocity.y -= gravity * Time.deltaTime;
        if (maxFallSpeed > 0 && velocity.y < 0) velocity.y = Mathf.Max(velocity.y, -maxFallSpeed);
        Move(Vector2.up * velocity * Time.deltaTime);
    }
    
    // A specific physics function to move a character until they decelerate completely
    protected void ApplyForce(Vector2 deceleration) 
    {
        if (velocity == Vector2.zero) return;
        
        var prevVel = velocity;
        deceleration.x *= Mathf.Sign(velocity.x);
        deceleration.y *= Mathf.Sign(velocity.y);
        if (velocity.x != 0) velocity.x -= deceleration.x * Time.deltaTime;
        if (velocity.y != 0) velocity.y -= deceleration.y * Time.deltaTime;
        
        if (Mathf.Sign(velocity.x * prevVel.x) <= 0) velocity.x = 0;
        if (Mathf.Sign(velocity.y * prevVel.y) <= 0) velocity.y = 0;

        Move(velocity * Time.deltaTime);
    }
    
    // A specific physics function to reduce the horizontal velocity of the entity
    protected void DecelerateX(float deceleration) 
    {
        if (velocity.x == 0) return;
        
        var prevVel = velocity;
        deceleration *= Mathf.Sign(velocity.x);
        velocity.x -= deceleration * Time.deltaTime;
        if (Mathf.Sign(velocity.x * prevVel.x) <= 0) velocity.x = 0;
        Move(Vector2.right * (velocity.x * Time.deltaTime));
    }
    
    // Relocate the object to a given position
    public void MoveObject(Vector2 newPosition)
    {
        transform.localPosition = newPosition;
        rb.position = newPosition;
    }

    #region Backup Debug Functions
/*
    protected void Move(Vector2 movement) //ViaTransform
    {
        Vector3 temp = movement; //movement.normalized * movement.magnitude;
        transform.Translate(movement);
        if (transform.localPosition.y < -7.4f)
        {
            isGrounded = true;
            var transform1 = transform;
            transform1.localPosition = new Vector3(transform1.position.x, -7.4f, 0);
        }
    }
*/
    #endregion
    
}
