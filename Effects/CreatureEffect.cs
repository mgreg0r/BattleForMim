using UnityEngine;
using System.Collections;

public class CreatureEffect : ContinuousEffect {

    public virtual void onSpawn(Creature c)
    {
        //do nothing
    }

    public virtual void onDestroy(Creature c)
    {
        //do nothing
    }

    public virtual void onMove(Creature c, Field beg, Field end)
    {
        //do nothing
    }

    public virtual void onStartTurn(Creature c)
    {
        //do nothing
    }

    public virtual void onEndTurn(Creature c)
    {
        //do nothing
    }

    public virtual void onAttack(Creature c, SpawnableObject target)
    {
        //do nothing
    }

    public virtual void onBeingAttacked(Creature c, SpawnableObject target)
    {
        //do nothing
    }
}
