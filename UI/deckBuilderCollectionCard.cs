using UnityEngine;
using System.Collections;

public class deckBuilderCollectionCard : MonoBehaviour {

    public int cardId;

    public void onEnter()
    {
        deckBuilderManager.hoveredId = cardId;
    }

    public void onExit()
    {
        deckBuilderManager.hoveredId = 0;
    }

    public void onClick()
    {
        deckBuilderManager.addCard(cardId);
    }

    public void Update()
    {

    }
}
