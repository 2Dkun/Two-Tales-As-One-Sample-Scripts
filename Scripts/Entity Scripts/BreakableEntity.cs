using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableEntity : BaseEntity
{
    private bool broken;
    private string flagName => SceneLoader.GetSceneName() + "_" + gameObject.name;
    
    public int hitsRequired = 1;
    public bool maintainCollider;
    
    new void Start()
    {
        base.Start();

        // Check save file if the object has already been broken
        if(DataManager.currentData.flags.Contains(flagName))
        {
            broken = true;
            box.isTrigger = !maintainCollider;
            dungeon.RemoveObstacle(this);
            
            // TODO: Change visual of object to be broken
        }
    }
    
    // Remove 1 hp from the object
    public override void Attacked(float damage, Vector3 knockback, bool wasTipper)
    {
        if (broken) return;
        
        hitsRequired -= 1;
        if (hitsRequired <= 0) Break();
    }
    
    // Break the object
    private void Break()
    {
        // TODO: Play break animation 
        
        // Write to data that object has been broken, but don't save
        DataManager.currentData.flags.Add(flagName);
        
        // Break the object
        broken = true;
        box.isTrigger = !maintainCollider;
        dungeon.RemoveObstacle(this);
        DropItem();
    }

    
    protected virtual void DropItem()
    {
        
    }

}
