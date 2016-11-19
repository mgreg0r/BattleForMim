using UnityEngine;
using System.Collections;

public class Soldier1 : CreatureCard {
    public Soldier1()
    {
        id = 2;
        name = "Soldier1";
        description = "";
        manaCost = 2;
        CreatureStats _stats = new CreatureStats(3, 3, 2, 2);
        setStats(_stats);
        swiftness = true;
    }

    public override Card getInstance()
    {
        return new Soldier1();
    }

}
