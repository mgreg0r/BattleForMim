using UnityEngine;
using System.Collections;

public class Soldier2 : CreatureCard
{
    public Soldier2()
    {
        id = 3;
        name = "Soldier2";
        description = "";
        manaCost = 3;
        CreatureStats _stats = new CreatureStats(5, 5, 2, 2);
        setStats(_stats);
        haste = true;
    }

    public override Card getInstance()
    {
        return new Soldier2();
    }

}