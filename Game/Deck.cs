using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;

public class Deck {

    public List<Card> cards;
    private int cardsInHand = 0;
	
    public Deck(List<Card> initContent)
    {
        cards = new List<Card>(initContent);
    }

    public int size()
    {
        return cards.Count;
    }

    public string getDeckHash()
    {
        string res = "";
        Dictionary<int, int> cardGroup = new Dictionary<int, int>();
        foreach(Card c in cards)
        {
            if (!cardGroup.ContainsKey(c.id))
            {
                cardGroup.Add(c.id, 0);
            }
            cardGroup[c.id]++;
        }
        foreach(int id in cardGroup.Keys)
        {
            res += "" + id + "x" + cardGroup[id] + "#";
        }
        return res;
    }

    public int getCardsInHand()
    {
        return cardsInHand;
    }

    public int getDeckSize()
    {
        return cards.Count;
    }
}
