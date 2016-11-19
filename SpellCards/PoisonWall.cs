using UnityEngine;
using System.Collections.Generic;

public class PoisonWall : Card
{
    private Field target;
    private int[] modX = { 0, 2, 4, 6, -2, -4, -6 };
    private int[] modY = { 0, 0, 0, 0, 0, 0, 0, };

    public PoisonWall()
    {
        id = 10;
        name = "PoisonWall";
        description = "Summons a wall of poison fields. Whenever a creature enters a poison field, it stops immediately and becomes poisoned. Poisoned creature takes 2 damage each turn, for 3 turns.";
        manaCost = 4;
    }

    public override Card getInstance()
    {
        return new PoisonWall();
    }

    public override CAST_EFFECT beginCast()
    {

        GameEngine.requestSetOfFieldsTarget(modX, modY, new EmptyFieldValidator(), "Select target");
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
        for (int i = 0; i < modX.Length; i++)
        {
            int mx = modX[i];
            int my = modY[i];
            GameObject go = GameEngine.getField(target.x + mx, target.y + my);
            if (go != null)
            {
                fields.Add(go.GetComponent<Field>());
            }
        }
        foreach (Field f in fields)
        {
            f.spawnEffect(new PoisonFieldEffect());
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