using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class deckBuilderDeckCard : MonoBehaviour {

    public Card card;
    public int slot;

    public void onEnter()
    {
        deckBuilderManager.hoveredId = card.id;
    }

    public void onExit()
    {
        deckBuilderManager.hoveredId = 0;
    }

    public void onClick()
    {
        deckBuilderManager.removeCard(slot);
    }

    public void Update()
    {
        if (card == null)
        {
            GetComponent<Image>().sprite = AssetManager.getSprite("empty");
        }
        else
        {
            if (card is CreatureCard)
            {
                GetComponent<Image>().sprite = AssetManager.getSprite("friendly" + card.name);
            }
            else GetComponent<Image>().sprite = AssetManager.getSprite(card.name);
        }
    }
}
