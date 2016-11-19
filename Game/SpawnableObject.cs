using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnableObject : Spawnable {


    public string spriteName;
    public bool isMoving = false;

    public virtual void OnClick()
    {
       
    }

    public virtual void dealDamage(int amount, Creature attacker)
    {

    }

    public virtual void dealDamage(int amount, string effectName)
    {

    }

    public virtual bool isDestroyed()
    {
        return false;
    }

    public virtual string getDescription()
    {
        return "";
    }

    public virtual void onBeingAttacked(Creature target)
    {

    }

    public virtual IEnumerator MovementAnimation(List<Field> path, GameObject go)
    {
        isMoving = true;
        foreach(var f in path)
        {
            while (new Vector2(go.transform.position.x, go.transform.position.y) != new Vector2(f.transform.position.x, f.transform.position.y))
            {
                go.transform.position = Vector3.MoveTowards(go.transform.position, f.transform.position, Time.deltaTime * 10);
                yield return 0;
            }
        }
        isMoving = false;
    }

}
