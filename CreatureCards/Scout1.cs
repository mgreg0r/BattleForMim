using UnityEngine;
using System.Collections;

public class Scout1 : CreatureCard {

    private Field droneTarget = null;

    public Scout1()
    {
        id = 6;
        name = "Scout1";
        description = "Whenever you summon Scout1, summon a ScoutingDrone";
        manaCost = 4;
        CreatureStats _stats = new CreatureStats(4, 3, 3, 4);
        setStats(_stats);
    }

    public override Card getInstance()
    {
        return new Scout1();
    }

    public override CAST_EFFECT beginCast()
    {
        GameEngine.requestFieldTarget(new EmptyFieldValidator(), "Select summon target for Scout1");
        return CAST_EFFECT.SUSPEND;
    }

    public override CAST_EFFECT continueCast(Field target)
    {
        if (spawnTarget == null) {
            spawnTarget = target;
            target.invalidTarget = true;
            GameEngine.requestFieldTarget(new SummonFieldValidator(2), "Select summon target for a drone");
            return CAST_EFFECT.SUSPEND;
        }
        droneTarget = target;
        target.invalidTarget = false;
        return CAST_EFFECT.SUCCESS;
    }

    public override void endCast()
    {
        spawnTarget.invalidTarget = false;
        spawnTarget.spawn(new Creature(name, this, spawnTarget, GameEngine.getLocalPlayer()));
        CreatureCard c = new ScoutingDrone();
        droneTarget.spawn(new Creature(c.name, c, droneTarget, GameEngine.getLocalPlayer()));
    }

    public override void cancelCast()
    {
        if(spawnTarget != null)
        {
            spawnTarget.invalidTarget = false;
        }
        base.cancelCast();
        droneTarget = null;
    }

    public override string getCastNetworkMessage()
    {
        return "cast#" + id + "#" + spawnTarget.x + "#" + spawnTarget.y + "#" + droneTarget.x + "#" + droneTarget.y;
    }

    public override void opponentCast(ParametersQueue args)
    {
        spawnTarget = args.getField();
        spawnTarget.spawn(new Creature(name, this, spawnTarget, GameEngine.getActivePlayer()));
        droneTarget = args.getField();
        CreatureCard c = new ScoutingDrone();
        droneTarget.spawn(new Creature(c.name, c, droneTarget, GameEngine.getActivePlayer()));
    }

}
