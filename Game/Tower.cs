using UnityEngine;
using System.Collections;

public class Tower : SpawnableObject {
    public int health = 50;
    private bool destroyed = false;

    public override bool isDestroyed()
    {
        return destroyed;
    }

    public Tower()
    {
        spriteName = "tower";
        gameObject = AssetManager.get("tower");
    }

    public override void dealDamage(int amount, string effectName)
    {
        if (owner == GameEngine.getLocalPlayer())
            GameEngine.log(effectName + " deals " + amount + " damage to your tower");
        else
        {
            GameEngine.log(effectName + " deals " + amount + " damage to enemy tower");
        }
        GameEngine.hpindicate(-amount, field.contentObject.transform);
        health -= amount;
        if (health <= 0)
            destroy();
    }

    public override void dealDamage(int amount, Creature attacker)
    {
        if (owner == GameEngine.getLocalPlayer())
            GameEngine.log(attacker.parentCard.name + " deals " + amount + " damage to your tower");
        else
        {
            GameEngine.log(attacker.parentCard.name + " deals " + amount + " damage to enemy tower");
        }
        GameEngine.hpindicate(-amount, field.contentObject.transform);
        health -= amount;
        if (health <= 0)
            destroy();
    }

    public void destroy()
    {
        field.content = null;
        Object.Destroy(field.contentObject);
        field.contentObject = null;
        owner.loseGame();
        if (owner == GameEngine.getLocalPlayer())
            GameEngine.log("Your tower has been destroyed! You lose!");
        else GameEngine.log("Enemy tower has been destroyed! You win!");
    }

    public override string getDescription()
    {
        return "HP: " +health;
    }
}
