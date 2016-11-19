using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FireballCard : Card {

    private Field target;
    private int[] modX = { 0, 0, 1, 1, 0, -1, -1 };
    private int[] modY = { 0, 2, 1, -1, -2, -1, 1};

	public FireballCard()
    {
        id = 7;
        name = "Fireball";
        description = "Deal 6 damage to each creature in area of effect";
        manaCost = 4;
    }

    public override Card getInstance()
    {
        return new FireballCard();
    }

    public override CAST_EFFECT beginCast()
    {

        GameEngine.requestSetOfFieldsTarget(modX, modY, new FieldValidator(), "Select target");
        return CAST_EFFECT.SUSPEND;
    }

    public override CAST_EFFECT continueCast(Field _target)
    {
        target = _target;
        return CAST_EFFECT.SUCCESS;
    }

    public override void endCast()
    {
        List<Field> fields = new List<Field>();
        for(int i = 0; i < modX.Length; i++)
        {
            int mx = modX[i];
            int my = modY[i];
            GameObject go = GameEngine.getField(target.x + mx, target.y + my);
            if(go != null)
            {
                fields.Add(go.GetComponent<Field>());
            }
        }
        target.invokeParticle("fireball");
        foreach(Field f in fields)
        {
            if(f.content != null)
                f.content.dealDamage(6, "Fireball");
        }
    }

    public override void cancelCast()
    {
        target = null;
    }

    public override string getCastNetworkMessage()
    {
        return "cast#" + id + "#" + target.x + "#" + target.y;
    }

    public override void opponentCast(ParametersQueue args)
    {
        target = args.getField();
        endCast();
    }
}
