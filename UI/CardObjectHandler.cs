using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CardObjectHandler : MonoBehaviour {

    private Vector3 initPos;
    private bool moving = false;
    public float selectSpeed = 1;
    private float distX;
    private float distY;
    public Card card;
    private bool selected = false;
    private int slot = -1;

    public Text cardName;
    public Text cost;
    public Text attack;
    public Text health;
    public Text speed;
    public Text range;
    public Image image;

    public void setDisplay()
    {
        cardName.text = card.name;
        cost.text = card.manaCost.ToString();      
        if (card is CreatureCard)
        {
            image.sprite = AssetManager.getSprite("friendly" + card.name);
            CreatureCard cc = (CreatureCard)card;
            attack.text = cc.stats.attack.ToString();
            health.text = cc.stats.health.ToString();
            speed.text = cc.stats.speed.ToString();
            range.text = cc.stats.range.ToString();
        }
        else
        {
            image.sprite = AssetManager.getSprite(card.name);
        }
    }

    public void setCard(Card _card)
    {
        card = _card;
    }

    public void Start()
    {
        initPos = transform.position;
        
    }

    public void mouseEnter()
    {
        if (!moving)
        {
            GameEngine.hoveredCard = this;
            selected = true;
            transform.position += new Vector3(0, 5, 0);
            GameEngine.setPreview(card);

        }
    }

    public void mouseLeave()
    {
        if (!moving)
        {
            GameEngine.hoveredCard = null;
            selected = false;
            transform.position = initPos;
        }
    }

    public void onBeginDrag()
    {
        if(!moving)
        {
            transform.SetAsLastSibling();
            GameEngine.draggedCard = this;
        }
    }

    public void onDrag()
    {
        if(!moving)
            transform.position = Input.mousePosition;
    }

    public void onDrop()
    {
        if (!moving)
        {
            Vector3 dist = initPos - transform.position;
            distX = dist.x;
            distY = dist.y;
            if (distY <= -200)
            {
                gameObject.SetActive(false);
                GameEngine.beginCast(card, this);
            }
            else
                moving = true;

            GameEngine.hoveredCard = null;
            GameEngine.draggedCard = null;
        }
    }

    public void endCast(bool result)
    {
        if(result)
            Destroy(gameObject);
        else
        {
            gameObject.SetActive(true);
            moving = true;
        }
    }

    public void Update()
    {        
        if(moving)
        {
            float x = transform.position.x;
            float y = transform.position.y;
            float z = transform.position.z;
            Vector3 nextPos = new Vector3(x + distX * Time.deltaTime *3, y + distY * Time.deltaTime *3, z);
            if ((initPos - transform.position).sqrMagnitude <= 1000)
            {
                transform.position = initPos;
                moving = false;
            }
            else transform.position = nextPos;
        }
    }

}
