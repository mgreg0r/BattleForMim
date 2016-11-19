using UnityEngine;
using System.Collections;

public class FieldEffect : ContinuousEffect {

    public string particle = null;
    public GameObject particleObject = null;

    public virtual bool onStep(Creature c, Field f)
    {
        return false;
    }
}
