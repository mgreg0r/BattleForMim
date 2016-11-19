using UnityEngine;
using System.Collections.Generic;
using System;

public class Creature : SpawnableObject {

    public CreatureCard parentCard;  //card from which this creature was created
    public CreatureStats stats;  //basic stats (attack, health, speed, range)
    private int maxAttacks = 1;  //attacks per turn
    private int maxMoves = 1;  //moves per turn
    private int maxSpecials = 1;  //specials per turn
    private int attacksLeft = 0;
    private int movesLeft = 0;
    private int specialsLeft = 0;
    private bool haste = false;  //can attack/move/special in the same turn it was summoned
    private bool swiftness = false;  //can't be counterattacked
    private bool flying = false;  //can fly above obstacles
    private bool destroyed = false;
    private int stunCounter = 0;
    public bool hasSpecial = false;
    public int specialRange = 0;

    private HashSet<CreatureEffect> effects = new HashSet<CreatureEffect>();

    public Creature(string id, CreatureCard card, Field _field, Player _owner)
    {
        gameObject = AssetManager.get("creatureHex");
        parentCard = card;
        spriteName = parentCard.name;
        stats = card.stats;
        field = _field;
        owner = _owner;
        haste = card.haste;
        flying = card.flying;
        swiftness = card.swiftness;
        maxAttacks = card.maxAttacks;
        maxMoves = card.maxMoves;
        hasSpecial = card.hasSpecial;
        maxSpecials = card.maxSpecials;
        specialRange = card.specialRange;
        if(haste)
        {
            attacksLeft = maxAttacks;
            movesLeft = maxMoves;
            specialsLeft = maxSpecials;
        }
    }

    public void stun(int turns) 
    {
        stunCounter = Math.Max(stunCounter, turns);
        movesLeft = 0;
        attacksLeft = 0;
        specialsLeft = 0;
        GameEngine.log(parentCard.name + " has been stunned for " +turns +" turns");
    }

    public bool canMove()
    {
        return movesLeft > 0;
    }

    public bool canAttack()
    {
        return attacksLeft > 0;
    }

    public bool canSpecial()
    {
        return specialsLeft > 0;
    }

    public bool canMoveThrough(Field f)
    {
        if (flying)
            return true;
        return !(f.content is Creature || f.content is Tower);
    }

    public bool canAttackThrough(Field f)
    {
        return true;
    }

    public bool canSpecialThrough(Field f)
    {
        return true;
    }

    public void move(Field f)
    {
        HashSet<CreatureEffect> eff = new HashSet<CreatureEffect>(effects);
        foreach(CreatureEffect ef in eff)
        {
            ef.onMove(this, field, f);
        }
        field.content = null;
        GameObject go = field.contentObject;
        field.contentObject = null;
        f.contentObject = go;
        f.content = this;
        field = f;
        movesLeft--;
    }

    public void startTurn()
    {
        HashSet<CreatureEffect> eff = new HashSet<CreatureEffect>(effects);
        foreach (CreatureEffect ef in eff)
        {
            ef.onStartTurn(this);
        }
        if (stunCounter > 0)
        {
            stunCounter--;
        }
        else
        {
            movesLeft = maxMoves;
            attacksLeft = maxAttacks;
            specialsLeft = maxSpecials;
        }
    }

    public void endTurn()
    {
        HashSet<CreatureEffect> eff = new HashSet<CreatureEffect>(effects);
        foreach (CreatureEffect ef in eff)
        {
            ef.onEndTurn(this);
        }
    }

    public override void onBeingAttacked(Creature target)
    {
        HashSet<CreatureEffect> eff = new HashSet<CreatureEffect>(effects);
        foreach (CreatureEffect ef in eff)
        {
            ef.onBeingAttacked(this, target);
        }
    }

    public void removeEffect(CreatureEffect ef)
    {
        effects.Remove(ef);
    }

    public void attack(SpawnableObject target)
    {
        HashSet<CreatureEffect> eff = new HashSet<CreatureEffect>(effects);
        foreach (CreatureEffect ef in eff)
        {
            ef.onAttack(this, target);
        }
        target.onBeingAttacked(this);
        target.dealDamage(stats.attack, this);
        attacksLeft--;
        if(!swiftness && target is Creature && !target.isDestroyed())
        {
            if (GameEngine.validateAttackPath(target.field, field, (Creature)target))
            {
                ((Creature)target).counterAttack(this);
            }
            
        }
    }

    public bool validateSpecialPath(Field beg, Field end)
    {
        return parentCard.validateSpecialPath(beg, end);
    }

    public void special(Field target)
    {
        parentCard.special(this, target);
        specialsLeft--;
    }

    public void counterAttack(Creature target)
    {
        target.dealDamage(stats.attack, this);
    }

    public override void dealDamage(int amount, string effectName)
    {
        GameEngine.log(effectName + " deals " + amount + " damage to " + parentCard.name);
        GameEngine.hpindicate(-amount, field.contentObject.transform);
        stats.health -= amount;
        if (stats.health <= 0)
            destroy();
    }

    public override void dealDamage(int amount, Creature attacker)
    {
        GameEngine.log(attacker.parentCard.name + " deals " + amount + " damage to " + parentCard.name);
        GameEngine.hpindicate(-amount, field.contentObject.transform);
        stats.health -= amount;
        if (stats.health <= 0)
            destroy();
    }

    public void destroy()
    {
        GameEngine.log(parentCard.name +" has been destroyed!");
        HashSet<CreatureEffect> eff = new HashSet<CreatureEffect>(effects);
        foreach (CreatureEffect ef in eff)
        {
            ef.onDestroy(this);
        }
        destroyed = true;
        field.content = null;
        UnityEngine.Object.Destroy(field.contentObject);
        field.contentObject = null;

    }

    public override bool isDestroyed()
    {
        return destroyed;
    }

    public override string getDescription()
    {
        return stats.attack + "/" + stats.health + "/" + stats.speed + "/" + stats.range;
    }

    public void addEffect(CreatureEffect ef)
    {
        effects.Add(ef);
    }
}
