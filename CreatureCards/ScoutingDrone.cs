using UnityEngine;
using System.Collections;

public class ScoutingDrone : CreatureCard {

    public ScoutingDrone()
    {
        id = 5;
        name = "ScoutingDrone";
        description = "";
        manaCost = 2;
        CreatureStats _stats = new CreatureStats(2, 1, 8, 3);
        setStats(_stats);
        maxAttacks = 2;
        flying = true;
    }

    public override Card getInstance()
    {
        return new ScoutingDrone();
    }
}
