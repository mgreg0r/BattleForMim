using UnityEngine;
using System.Collections;

public class enemyCreatureValidator : FieldValidator {

    public override bool validate(Field f)
    {
        return f.content != null && f.content is Creature && f.content.owner == GameEngine.getOpPlayer();
    }
}
