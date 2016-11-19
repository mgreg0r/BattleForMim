using UnityEngine;
using System.Collections.Generic;

public class ThunderstormCard : Card
{
    public ThunderstormCard()
    {
        id = 9;
        name = "Thunderstorm";
        description = "Deal 4 damage to every creature";
        manaCost = 4;
    }

    public override Card getInstance()
    {
        return new ThunderstormCard();
    }

    public override CAST_EFFECT beginCast()
    {
        return CAST_EFFECT.SUCCESS;
    }

    public override void endCast()
    {
        List<Field> fields = GameEngine.getValidFields();
        Field central = GameEngine.getField(8, 10).GetComponent<Field>();
        central.invokeParticle("thunderstorm");
        foreach (Field f in fields)
        {
            if (f.content != null && f.content is Creature)
                f.content.dealDamage(4, "Thunderstorm");
        }
    }

    public override string getCastNetworkMessage()
    {
        return "cast#" + id;
    }

    public override void opponentCast(ParametersQueue args)
    {
        endCast();
    }
}
