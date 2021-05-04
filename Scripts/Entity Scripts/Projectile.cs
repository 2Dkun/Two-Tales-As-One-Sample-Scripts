using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : PhysicsEntity
{
    public float damage, moveSpeed;
    public Vector2 knockback, direction, shift;

    protected void Start()
    {
        direction.x *= Mathf.Sign(transform.localScale.x);
    }

    void Update()
    {
        Move(direction * (moveSpeed * Time.deltaTime));
    }

    // Hurt the player if they came into contact with the projectile
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<BasePlayer>().Attacked(damage, knockback, false);
        }
        
        // TODO: Play an animation before dying
        Destroy(gameObject);
    }
}

/*
[CreateAssetMenu(fileName = "ProjectileData", menuName = "ScriptableObjects/Projectile", order = 1)]
public class ProjectileConfig : ScriptableObject
{
    public Vector2 direction;
}
*/
