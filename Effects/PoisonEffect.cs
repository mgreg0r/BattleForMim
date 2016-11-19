using UnityEngine;
using System.Collections;

public class PoisonEffect : CreatureEffect {

    public int turns = 1;
    public int dmg = 1;

    public PoisonEffect(int _dmg, int _turns)
    {
        dmg = _dmg;
        turns = _turns;
    }

    public override void onStartTurn(Creature c)
    {
        turns--;
        c.dealDamage(dmg, "poison");
        if(turns <= 0)
        {
            c.removeEffect(this);
        }
    }

}
