using UnityEngine;
using System.Collections;

public class Sniper1 : CreatureCard {

    public Sniper1()
    {
        id = 8;
        name = "Sniper1";
        description = "";
        specialDescription = "Sleepdart - puts an enemy creature to sleep for 2 turns.\nOne use per turn (range 8)";
        manaCost = 3;
        CreatureStats _stats = new CreatureStats(3, 5, 2, 8);
        setStats(_stats);
        hasSpecial = true;
        specialRange = 8;
        specialValidator = new enemyCreatureValidator();
    }

    public override Card getInstance()
    {
        return new Sniper1();
    }

    public override void special(Creature invoker, Field target)
    {
        if(target.content != null)
        {
            if(target.content is Creature)
            {
                Creature c = (Creature)target.content;
                c.stun(2);
            }
        }
    }
}
