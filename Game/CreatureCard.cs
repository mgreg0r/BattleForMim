using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CreatureCard : Card {
    public CreatureStats stats = new CreatureStats(1, 1, 1, 1);
    public Sprite CreatureImg;
    public Field spawnTarget;

    public int maxAttacks = 1;
    public int maxMoves = 1;
    public int maxSpecials = 1;
    public bool haste = false;
    public bool swiftness = false;
    public bool flying = false;
    public bool hasSpecial = false;
    public int specialRange = 0;
    public int summonRange = 2;
    public FieldValidator specialValidator = new FieldValidator();
    public int[] specialModX = { 0 };
    public int[] specialModY = { 0 };
    public string specialDescription = "";

    public void setAttack(int _attack)
    {
        stats.attack = _attack;
    }

    public void setHealth(int _health)
    {
        stats.health = _health;
    }

    public void setSpeed(int _speed)
    {
        stats.speed = _speed;
    }

    public void setRange(int _range)
    {
        stats.range = _range;
    }

    public void setStats(CreatureStats _stats)
    {
        stats = _stats;
    }

    public override CAST_EFFECT beginCast()
    {
        GameEngine.requestFieldTarget(new SummonFieldValidator(summonRange), "Select summon target");
        return CAST_EFFECT.SUSPEND;
    }

    public override CAST_EFFECT continueCast(Field target)
    {
        spawnTarget = target;
        return CAST_EFFECT.SUCCESS;
    }

    public override void endCast()
    {
        spawnTarget.spawn(new Creature(name, this, spawnTarget, GameEngine.getLocalPlayer()));
    }

    public override void cancelCast()
    {
        spawnTarget = null;
    }


    public override string getCastNetworkMessage()
    {
        return "cast#" +id +"#" +spawnTarget.x +"#" +spawnTarget.y;
    }

    public override void opponentCast(ParametersQueue args)
    {
        spawnTarget = args.getField();
        spawnTarget.spawn(new Creature(name, this, spawnTarget, GameEngine.getActivePlayer()));
    }

    public virtual void special(Creature invoker, Field target)
    {

    }

    public virtual bool validateSpecialPath(Field beg, Field end)
    {
        return true;
    }

}
