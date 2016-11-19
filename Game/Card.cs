using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum CAST_EFFECT { SUCCESS, FAILURE, SUSPEND };

public class Card {

    public int id;
    public string name = "";
    public string description = "";
    public int manaCost = 0;
    public int slot = -1;

    public virtual string getCastNetworkMessage()
    {
        return "cast";
    }

    public virtual Card getInstance()
    {
        return null;
    }

    public virtual CAST_EFFECT cast()
    {
        return CAST_EFFECT.FAILURE;
    }

    public virtual CAST_EFFECT beginCast()
    {
        return CAST_EFFECT.FAILURE;
    }

    public virtual CAST_EFFECT continueCast(Field target)
    {
        return CAST_EFFECT.FAILURE;
    }

    public virtual void cancelCast()
    {

    }

    public virtual void endCast()
    {

    }

    public virtual void showPreview(UIInterface uiManager)
    {

    }

    public virtual void opponentCast(ParametersQueue args)
    {

    }

}
