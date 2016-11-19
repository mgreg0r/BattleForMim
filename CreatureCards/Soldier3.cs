using UnityEngine;
using System.Collections;

public class Soldier3 : CreatureCard
{
    public Soldier3()
    {
        id = 4;
        name = "Soldier3";
        description = "";
        manaCost = 5;
        CreatureStats _stats = new CreatureStats(8, 5, 3, 3);
        setStats(_stats);
    }

    public override Card getInstance()
    {
        return new Soldier3();
    }

}
