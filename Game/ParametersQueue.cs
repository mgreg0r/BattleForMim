using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ParametersQueue {

    private List<int> args = new List<int>();
    private int counter = 0;

    public ParametersQueue()
    {

    }

	public ParametersQueue(List<int> _args)
    {
        args = _args;
    }

    public int getInt()
    {
        int res = args[counter];
        counter++;
        return res;
    }

    public Field getField()
    {
        int x = getInt();
        int y = getInt();
        return GameEngine.mirrorField(GameEngine.getField(x, y).GetComponent<Field>());
    }
}
