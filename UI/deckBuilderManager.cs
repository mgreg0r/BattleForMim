using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class deckBuilderManager : MonoBehaviour {

    public static int cards = 30;
    public static int[] deck = new int[30];
    public Image[] deckImg;
    public static string DEFAULT_DECK = "3x30#";
    public GameObject previewPanel;
    public Image previewImage;
    public Text previewAttack;
    public Text previewHealth;
    public Text previewSpeed;
    public Text previewRange;
    public Text previewDescription;
    public Text deckSizeText;

    private static deckBuilderManager manager;
    public static int hoveredId = 0;

    public void decodeDeck(string s)
    {
        cards = 0;
        for(int i = 0; i < 30; i++)
        {
            deck[i] = 0;
        }
        int last = 0;
        int cur = 0;
        int xpos = 0;
        string prex = "";
        string postx = "";
        while (last < s.Length - 1)
        {
            while (s[cur] != '#')
            {
                if (s[cur] == 'x')
                    xpos = cur;
                else if (!char.IsDigit(s[cur]))
                    return; //invalid deck
                else if (xpos > last)
                    postx += s[cur];
                else prex += s[cur];


                cur++;
                if (cur == s.Length)
                    return; //invalid deck
            }
            last = cur;
            cur++;
            xpos = 0;

            int cardId = int.Parse(prex);
            int cardAmount = int.Parse(postx);
            prex = "";
            postx = "";
            for(int i = 0; i < cardAmount; i++)
            {
                addCard(cardId);
            }
        }
    }

    public static bool encodeDeck()
    {
        Dictionary<int, int> compressed = new Dictionary<int, int>();
        for(int i = 0; i < 30; i++)
        {
            if (deck[i] == 0)
                return false;
            if (!compressed.ContainsKey(deck[i]))
                compressed.Add(deck[i], 0);
            compressed[deck[i]]++;
        }
        string res = "";
        foreach(int i in compressed.Keys)
        {
            res += i.ToString() + "x" + compressed[i] + "#";
        }
        PlayerPrefs.SetString("deck", res);
        PlayerPrefs.Save();
        return true;
    }

    public void Start()
    {
        manager = this;
        GameEngine.initCards();
        string deckCode = "";
        if(PlayerPrefs.HasKey("deck"))
        {
            deckCode = PlayerPrefs.GetString("deck");
        }
        else
        {
            deckCode = DEFAULT_DECK;
        }
        decodeDeck(deckCode);
        
    }

    private string buildDescription(Card c)
    {
        string result = c.name + "\n";
        result += c.description + "\n";

        if (c is CreatureCard)
        {
            CreatureCard cc = (CreatureCard)c;
            if (cc.hasSpecial)
                result += "SPECIAL ABILITY:\n" + cc.specialDescription + "\n";

            List<string> abilities = new List<string>();
            if (cc.maxAttacks == 2)
                abilities.Add("double strike");
            if (cc.flying)
                abilities.Add("flying");
            if (cc.haste)
                abilities.Add("haste");
            if (cc.swiftness)
                abilities.Add("swiftness");
            if (abilities.Count > 0)
            {
                result += "PASSIVE ABILITIES:\n" + abilities[0];
                for (int i = 1; i < abilities.Count; i++)
                    result += ", " + abilities[i];
            }
        }

        return result;
    }

    public void Update()
    {
        deckSizeText.text = cards.ToString() + "/30";
        if(hoveredId != 0)
        {
            Card c = GameEngine.getCardById(hoveredId);
            previewPanel.SetActive(true);
            if (c is CreatureCard)
            {
                CreatureCard cc = (CreatureCard)c;
                previewImage.sprite = AssetManager.getSprite("friendly" + c.name);
                previewAttack.text = cc.stats.attack.ToString();
                previewHealth.text = cc.stats.health.ToString();
                previewSpeed.text = cc.stats.speed.ToString();
                previewRange.text = cc.stats.range.ToString();
                previewDescription.text = buildDescription(cc);
            }
            else
            {
                previewImage.sprite = AssetManager.getSprite(c.name);
                previewAttack.text = "";
                previewHealth.text = "";
                previewSpeed.text = "";
                previewRange.text = "";
                previewDescription.text = buildDescription(c);
            }
        }
        else
        {
            previewPanel.SetActive(false);
        }
    }

    public static int getSlot()
    {
        for (int i = 0; i < 30; i++)
        {
            if (deck[i] == 0)
                return i;
        }
        return -1;
    }

    public static void addCard(int card)
    {
        int slot = getSlot();
        if (slot == -1)
            return;
        deck[slot] = card;
        manager.deckImg[slot].GetComponent<deckBuilderDeckCard>().card = GameEngine.getCardById(card);
        deckBuilderManager.cards++;
    }

    public static void removeCard(int slot)
    {
        if(deck[slot] != 0)
            deckBuilderManager.cards--;
        deck[slot] = 0;
        manager.deckImg[slot].GetComponent<deckBuilderDeckCard>().card = null;
    }

    public void saveClick()
    {
        if(!encodeDeck())
        {
            //invalid deck
        }
    }

    public void defaultClick() {
        decodeDeck(DEFAULT_DECK);
        encodeDeck();
    }
}
