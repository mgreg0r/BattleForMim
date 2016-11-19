using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIInterface : MonoBehaviour {

    public int opponentHand = 0;
    public int opponentDeck = 30;

    public Text previewAttack;
    public Text previewHealth;
    public Text previewSpeed;
    public Text previewRange;
    public Text previewDescription;

    public Text localMana;
    public Text localHand;
    public Text localDeck;
    public Text oppMana;
    public Text oppHand;
    public Text oppDeck;
    public GameObject previewPanel;

    public Image previewImage;

    private HashSet<Field> fields;
    private HashSet<Field> path = new HashSet<Field>();
    private HashSet<Field> range = new HashSet<Field>();
    private HashSet<Field> targetedSet = new HashSet<Field>();
    private bool ready = false;

    private static Color INVALID = Color.red;
    private static Color VALID = Color.green;
    private static Color SELECT = Color.blue;
    private static Color HIGHLIGHT = Color.yellow;
    private static Color ZERO = Color.white;

    public void setPath(List<Field> _path)
    {
        path = new HashSet<Field>(_path);
    }

    public void setRange(List<Field> _range)
    {
        range = new HashSet<Field>(_range);
    }

    public void setTargetedSet(List<Field> _set)
    {
        targetedSet = new HashSet<Field>(_set);
    }

    public void zeroPath()
    {
        path.Clear();
    }

    public void zeroSet()
    {
        targetedSet.Clear();
    }

    public void zeroFields()
    {
        path.Clear();
        range.Clear();
        targetedSet.Clear();
    }

    private void setColor(Field f, Color color)
    {
        setFieldColor(f, color);
        setContentColor(f, color);
    }

    private void setFieldColor(Field f, Color color)
    {
        f.GetComponent<SpriteRenderer>().color = color;
    }

    private void setContentColor(Field f, Color color)
    {
        if(f.contentObject != null)
        {
            f.contentObject.GetComponent<SpriteRenderer>().color = color;
        }
    }

    public void init()
    {
        fields = new HashSet<Field>(GameEngine.getValidFields());
        ready = true;
    }

    public void updateColors()
    {
        foreach (Field f in fields)
        {
            Field selected = GameEngine.getSelectedField();
            Field hovered = GameEngine.hoveredField;
            bool switched = GameEngine.switchedTarget;
            if (selected != null)
            {
                if (f == selected)
                {
                    if (targetedSet.Contains(f))
                    {
                        if (GameEngine.validAction())
                            setFieldColor(f, VALID);
                        else setFieldColor(f, INVALID);
                        setContentColor(f, SELECT);
                    }
                    else
                    {
                        setColor(f, SELECT);
                    }
                }
                else
                {
                    if (targetedSet.Contains(f))
                    {
                        if (GameEngine.validAction())
                            setColor(f, VALID);
                        else setColor(f, INVALID);
                    }
                    else
                    {
                        if (path.Contains(f))
                        {
                            setColor(f, VALID);
                        }
                        else if (range.Contains(f))
                        {
                            setColor(f, HIGHLIGHT);
                        }
                        else
                        {
                            setColor(f, ZERO);
                        }
                    }
                }
            }
            else if (GameEngine.getCast() != null)
            {
                if (targetedSet.Contains(f))
                {
                    if (GameEngine.validAction())
                        setColor(f, VALID);
                    else setColor(f, INVALID);
                }
                else
                {
                    setColor(f, ZERO);
                }
            }
            else
            {
                if (f == hovered)
                {
                    setColor(f, SELECT);
                }
                else
                {
                    setColor(f, ZERO);
                }
            }
        }
    }

    public void opponentDrawCard()
    {
        if(opponentDeck > 0)
        {
            opponentDeck--;
            opponentHand++;
        }

    }

    private void clearPreview()
    {
        previewImage.sprite = AssetManager.getSprite("empty");
        previewAttack.text = "";
        previewHealth.text = "";
        previewSpeed.text = "";
        previewRange.text = "";
    }

    private string buildDescription(Card c)
    {
        string result = c.name + "\n";
        result += c.description + "\n";

        if(c is CreatureCard)
        {
            CreatureCard cc = (CreatureCard)c;
            if (cc.hasSpecial)
                result += "SPECIAL ABILITY:\n" + cc.specialDescription +"\n";

            List<string> abilities = new List<string>();
            if (cc.maxAttacks == 2)
                abilities.Add("double strike");
            if (cc.flying)
                abilities.Add("flying");
            if (cc.haste)
                abilities.Add("haste");
            if (cc.swiftness)
                abilities.Add("swiftness");
            if(abilities.Count > 0)
            {
                result += "PASSIVE ABILITIES:\n" +abilities[0];
                for (int i = 1; i < abilities.Count; i++)
                    result += ", " + abilities[i];
            }
        }

        return result;
    }

    private void updatePreview(CardObjectHandler obj)
    {
        Card c = obj.card;
        if(c is CreatureCard)
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

    public void updatePreview(HistoryObjectHandler obj)
    {
        Card c = obj.card;
        if (c is CreatureCard)
        {
            CreatureCard cc = (CreatureCard)c;
            if (obj.owner == CARD_OWN.FRIENDLY)
                previewImage.sprite = AssetManager.getSprite("friendly" + c.name);
            else if (obj.owner == CARD_OWN.ENEMY)
                previewImage.sprite = AssetManager.getSprite("enemy" + c.name);
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

    public void updatePreview(Card c)
    {
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

    private void updatePreview(SpawnableObject obj)
    {
        if (obj is Creature)
        {
            Creature c = (Creature)obj;
            if (c.owner == GameEngine.getLocalPlayer())
                previewImage.sprite = AssetManager.getSprite("friendly" + c.parentCard.name);
            else previewImage.sprite = AssetManager.getSprite("enemy" + c.parentCard.name);
            previewAttack.text = c.stats.attack.ToString();
            previewHealth.text = c.stats.health.ToString();
            previewSpeed.text = c.stats.speed.ToString();
            previewRange.text = c.stats.range.ToString();
            previewDescription.text = buildDescription(c.parentCard);
        }
        else if(obj is Tower)
        {
            previewAttack.text = "";
            previewHealth.text = ((Tower)obj).health.ToString();
            previewSpeed.text = "";
            previewRange.text = "";
            if (obj.owner == GameEngine.getLocalPlayer())
            {
                previewImage.sprite = AssetManager.getSprite("friendlyTower");
                previewDescription.text = "This is your tower. If its health drops to 0, you lose the game.";
            }
            else
            {
                previewImage.sprite = AssetManager.getSprite("enemyTower");
                previewDescription.text = "This is enemy tower. If its health drops to 0, you win the game.";
            }
        }
    }

    public void Update()
    {
        if (ready)
        {
            localMana.text = GameEngine.getLocalPlayer().mana.ToString();
            localHand.text = GameEngine.getLocalPlayer().cardsInHand.ToString();
            localDeck.text = GameEngine.getLocalPlayer().deckSize.ToString();
            oppMana.text = GameEngine.getOpPlayer().mana.ToString();
            oppHand.text = opponentHand.ToString();
            oppDeck.text = opponentDeck.ToString();

            if (GameEngine.draggedCard != null)
            {
                updatePreview(GameEngine.draggedCard);
                previewPanel.SetActive(true);
            }
            else if (GameEngine.hoveredCard != null)
            {
                updatePreview(GameEngine.hoveredCard);
                previewPanel.SetActive(true);
            }
            else if(GameEngine.hoveredHistory != null)
            {
                updatePreview(GameEngine.hoveredHistory);
                previewPanel.SetActive(true);
            }
            else if (GameEngine.hoveredField != null)
            {
                if (GameEngine.hoveredField.content != null)
                {
                    updatePreview(GameEngine.hoveredField.content);
                    previewPanel.SetActive(true);
                }
                else
                {
                    if(GameEngine.getCast() != null)
                    {
                        updatePreview(GameEngine.getCast());
                        previewPanel.SetActive(true);
                    }
                    else {
                        clearPreview();
                        previewPanel.SetActive(false);
                    }
                }
            }
            else if(GameEngine.getCast() != null)
            {
                updatePreview(GameEngine.getCast());
                previewPanel.SetActive(true);
            }
            else
            {
                clearPreview();
                previewPanel.SetActive(false);
            }

        }
    }
}
