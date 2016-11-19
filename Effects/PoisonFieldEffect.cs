using UnityEngine;
using System.Collections;

public class PoisonFieldEffect : FieldEffect
{
    public PoisonFieldEffect()
    {
        particle = "poisonField";
    }

    public override bool onStep(Creature c, Field f)
    {
        GameEngine.log(c.parentCard.name + " has been poisoned!");
        c.addEffect(new PoisonEffect(2, 3));
        f.removeEffect(this);
        GameObject.Destroy(particleObject);
        return true;
    }
}