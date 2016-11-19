using UnityEngine;
using System.Collections;

public class SummonFieldValidator : FieldValidator {

    public int range = 1;

    public SummonFieldValidator(int _range)
    {
        range = _range;
    }
        
    public override bool validate(Field f)
    {
        return f.isEmpty() && GameEngine.distanceFromTower(f) <= range;
    }
}
