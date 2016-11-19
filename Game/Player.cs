using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player {

    public string name = "";
    public Tower tower;
    public Deck deck;
    public Card[] hand = { null, null, null, null, null, null, null, null, null, null };
    public int mana = 5;
    public int maxMana = 5;
    public int deckSize = 30;
    public int cardsInHand = 0;

    public Player()
    {

    }

    public Player(Tower _tower)
    {
        tower = _tower;
    }

    private int findFreeSlot()
    {
        for (int i = 0; i < hand.Length; i++)
            if (hand[i] == null)
                return i;
        return -1;
    }

    public void drawCard(int id)
    {
        Card newCard = GameEngine.getCardById(id);
        int slot = findFreeSlot();
        if (slot == -1)
        {
            GameEngine.log("You draw a card, but your hand is full. Card is lost.");
            return;
        }
        hand[slot] = newCard;
        hand[slot].slot = slot;
        cardsInHand++;
        deckSize--;
        GameEngine.drawCardObject(newCard, slot);
    }

    public void startTurn()
    {
        maxMana++;
        mana = maxMana;
    }

    public void cast(Card card)
    {
        hand[card.slot] = null;
        mana -= card.manaCost;
        cardsInHand--;
    }

    public void loseGame()
    {
        
    }

    public void endTurn()
    {

    }

}
