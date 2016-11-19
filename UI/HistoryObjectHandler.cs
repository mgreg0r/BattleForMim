using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum CARD_OWN
{
    FRIENDLY, ENEMY, NEUTRAL
}

public class HistoryObjectHandler : MonoBehaviour {

    public Card card;
    public CARD_OWN owner;

    public void setCard(Card _card, CARD_OWN _owner)
    {
        card = _card;
        if (card != null)
        {
            string spriteName = "";
            owner = _owner;
            if (owner == CARD_OWN.FRIENDLY)
                spriteName += "friendly";
            else if (owner == CARD_OWN.ENEMY)
                spriteName += "enemy";
            spriteName += card.name;
            GetComponent<Image>().sprite = AssetManager.getSprite(spriteName);
        }
    }

    public void onMouseEnter()
    {
        if (card != null)
        {
            GameEngine.hoveredHistory = this;
        }
    }

    public void onMouseLeave()
    {
        if (card != null)
        {
            GameEngine.hoveredHistory = null;
        }
    }
}
