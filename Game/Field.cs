using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Field : MonoBehaviour {

    public int x, y ;
    private SpriteRenderer fieldRenderer;
    private HashSet<FieldEffect> effects = new HashSet<FieldEffect>();
    public SpawnableObject content = null;
    public GameObject contentObject = null;
    public Color defaultHighlight = Color.white;
    private List<Field> neighbours = new List<Field>();
    public bool hovered = false;
    public bool invalidTarget = false;


    public void init(int _x, int _y)
    {
        fieldRenderer = GetComponent<SpriteRenderer>();
        content = null;
        x = _x;
        y = _y;
    }

    public void addNeighbour(Field f)
    {
        neighbours.Add(f);
    }

    public List<Field> getNeighbours()
    {
        return neighbours;
    }

    public GameObject invokeParticle(string name)
    {
        GameObject go = AssetManager.getParticle(name);
        return Instantiate(go, transform.position, transform.rotation) as GameObject;
    }

    public void OnMouseEnter()
    {
        hovered = true;
        GameEngine.hoveredField = this;
        GameEngine.onEnter(this);
        GameEngine.updateColors();
    }

    public bool onMoveThrough(Creature c)
    {
        bool res = false;
        HashSet<FieldEffect> eff = new HashSet<FieldEffect>(effects);
        foreach (FieldEffect ef in eff)
        {
            if (ef.onStep(c, this))
                res = true;
        }
        return res;
    }

    public void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            GameEngine.rightClick(this);
        }
    }

    public void OnMouseExit()
    {
        hovered = false;
        GameEngine.hoveredField = null;
        GameEngine.onLeave(this);
        GameEngine.updateColors();
    }

    public void OnMouseDown()
    {
        GameEngine.fieldClick(this);
    }

    public void spawnEffect(FieldEffect ef)
    {
        effects.Add(ef);
        if(ef.particle != null)
        {
            ef.particleObject = invokeParticle(ef.particle);
        }
    }

    public void removeEffect(FieldEffect ef)
    {
        effects.Remove(ef);
    }

    public void spawn(SpawnableObject obj)
    {
        HashSet<GlobalEffect> eff = new HashSet<GlobalEffect>(GameEngine.getEffects());
        foreach (GlobalEffect ef in eff)
        {
            ef.onSpawn(obj);
        }
        this.content = obj;
        Vector3 pos = new Vector3(transform.position.x, transform.position.y, 1);
        contentObject = Instantiate(obj.gameObject, pos, transform.rotation) as GameObject;
        if(obj.owner == GameEngine.getLocalPlayer())
        {
            contentObject.GetComponent<SpriteRenderer>().sprite = AssetManager.getSprite("friendly" + obj.spriteName);
        }
        else
        {
            contentObject.GetComponent<SpriteRenderer>().sprite = AssetManager.getSprite("enemy" +obj.spriteName);
        }
        
        if(hovered) contentObject.GetComponent<SpriteRenderer>().color = defaultHighlight;

        if(obj is Creature)
        {
            Creature c = (Creature)obj;
            HashSet<FieldEffect> feff = new HashSet<FieldEffect>(effects);
            foreach (FieldEffect ef in feff)
            {
                ef.onStep(c, this);
            }
        }
        
    }

    public bool isEmpty()
    {
        return content == null;
    }

}
