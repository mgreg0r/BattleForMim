using UnityEngine;
using System.Collections;

public class CreatureStats {

    public int attack = 0;
    public int health = 0;
    public int speed = 0;
    public int range = 0;

    public CreatureStats(int _attack, int _health, int _speed, int _range)
    {
        attack = _attack;
        health = _health;
        speed = _speed;
        range = _range;
    }

}
