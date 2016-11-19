using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Threading;
using System.Net.Sockets;
using UnityEngine.SceneManagement;

public enum TARGET_TYPE { NONE, FIELD, SET_OF_FIELDS }

public class GameEngine : MonoBehaviour
{

    public static Color MOUSE_OVER_FIELD_COLOR = new Color(0.5f, 1, 1, 0.9f);
    public static Color VALID_TARGET_COLOR = new Color(0, 1, 0, 0.9f);
    public static Color INVALID_TARGET_COLOR = new Color(1, 0, 0, 0.9f);
    public static int BOARD_SIZE_X = 8;
    public static int BOARD_SIZE_Y = 12;
    public static int INIT_HAND_SIZE = 5;

    private static int[] neighModX = { 0, 1, 1, 0, -1, -1 };
    private static int[] neighModY = { 2, 1, -1, -2, -1, 1 };

    public static float[] SLOTX = { 30, 90, 150, 210, 270, 330, 390, 450, 510, 570 };
    public static float SLOTY = 40;

    public static Dictionary<int, Card> CARD_BASE = new Dictionary<int, Card>();
    public static bool cardsInit = false;
    public static Thread listener;
    public static TcpClient client;
    public static object clientLock = new object();
    public static bool dcFlag = false;
    public static object dcFlagLock = new object();

    public void OnApplicationQuit()
    {
        leaveGame();
    }

    public static void leaveGame()
    {
        if (GameEngine.listener != null && GameEngine.listener.IsAlive)
        {
            GameEngine.listener.Abort();
        }
        lock (GameEngine.clientLock)
        {
            if (GameEngine.client != null)
            {
                GameEngine.client.Close();
            }
        }
    }

    public static void initCards()
    {
        if (cardsInit)
            return;
        cardsInit = true;
        CARD_BASE.Add(2, new Soldier1());
        CARD_BASE.Add(3, new Soldier2());
        CARD_BASE.Add(4, new Soldier3());
        CARD_BASE.Add(5, new ScoutingDrone());
        CARD_BASE.Add(6, new Scout1());
        CARD_BASE.Add(7, new FireballCard());
        CARD_BASE.Add(8, new Sniper1());
        CARD_BASE.Add(9, new ThunderstormCard());
        CARD_BASE.Add(10, new PoisonWall());
    }

    private HashSet<GlobalEffect> effects = new HashSet<GlobalEffect>();
    public GameObject uiCanvas;
    public Text gameMessageField;
    public Text debugLog;
    public static Field hoveredField = null;
    public static HistoryObjectHandler hoveredHistory = null;
    public static CardObjectHandler hoveredCard = null;
    public static CardObjectHandler draggedCard = null;
    public static bool switchedTarget = false;
    public static bool switched = false;
    private static bool writing = false;
    public InputField inputField;

    private static GameEngine GAME;
    private Player activePlayer, localPlayer, opPlayer;
    private Card curCast = null;
    private CardObjectHandler cardHandler = null;
    private TARGET_TYPE currentRequest = TARGET_TYPE.NONE;
    private FieldValidator currentValidator = null;
    private UIInterface uiManager = null;
    private Creature selectedCreature = null;
    private Field selectedField = null;
    private Dictionary<Field, Dictionary<Field, int>> attackDistances = new Dictionary<Field, Dictionary<Field, int>>();
    private Dictionary<Field, int> moveDistances = new Dictionary<Field, int>();
    private Dictionary<Field, int> towerDistances = new Dictionary<Field, int>();
    private Dictionary<Field, Dictionary<Field, Field>> attackPaths = new Dictionary<Field, Dictionary<Field, Field>>();
    private Dictionary<Field, Field> movePaths = new Dictionary<Field, Field>();
    private int[] setRequestModX = { };
    private int[] setRequestModY = { };
    public GameObject[] historyObjects;

    private GameObject[,] board;

    public static void gmsg(string s)
    {
        GAME.gameMessageField.text = s;
    }

    public static void hpindicate(int amount, Transform transform)
    {
        floatingTextController.createFloatingText(amount.ToString(), transform);
    }

    public void addToHistory(Card c, CARD_OWN owner)
    {
        for (int i = historyObjects.Length - 2; i >= 0; i--)
        {
            Card prevC = historyObjects[i].GetComponent<HistoryObjectHandler>().card;
            CARD_OWN prevCO = historyObjects[i].GetComponent<HistoryObjectHandler>().owner;
            historyObjects[i + 1].GetComponent<HistoryObjectHandler>().setCard(prevC, prevCO);
        }
        historyObjects[0].GetComponent<HistoryObjectHandler>().setCard(c, owner);
    }

    public static Card getCast()
    {
        return GAME.curCast;
    }

    public static Card getCardById(int id)
    {
        return CARD_BASE[id].getInstance();
    }

    public static Player getOpPlayer()
    {
        return GAME.opPlayer;
    }

    public static void startGame(List<int> args)
    {
        Field ft1 = getField(BOARD_SIZE_X, 0).GetComponent<Field>();
        Field ft2 = getField(BOARD_SIZE_X, BOARD_SIZE_Y * 2).GetComponent<Field>();

        Tower t1 = new Tower();
        Tower t2 = new Tower();

        log("Game starts!");
        GAME.localPlayer = new Player(t1);
        GAME.opPlayer = new NetworkedPlayer(t2);
        t1.owner = GAME.localPlayer;
        t2.owner = GAME.opPlayer;

        int act = args[0];
        if (act == 0)
        {
            GAME.activePlayer = GAME.localPlayer;
            log("You go first");
        }
        else
        {         
            GAME.activePlayer = GAME.opPlayer;
            log("Opponent goes first");
        }
        t1.field = ft1;
        t2.field = ft2;

        ft1.spawn(t1);
        ft2.spawn(t2);

        GAME.generateNeighbours();
        generateAttackPaths();
        generateLayers();
    }

    public static void drawCard(List<int> args)
    {
        int id = args[0];
        GAME.localPlayer.drawCard(args[0]);
    }

    void Start()
    {
        initCards();
        GAME = this;
        uiManager = GetComponent<UIInterface>();
        board = GetComponent<GridGenerator>().GenerateGrid();
        floatingTextController.init();
        uiManager.init();
    }

    public static Field getSelectedField()
    {
        return GAME.selectedField;
    }

    private void generateNeighbours()
    {
        foreach (Field f in getValidFields())
        {

            for (int i = 0; i < neighModX.Length; i++)
            {
                int cordX = f.x + neighModX[i];
                int cordY = f.y + neighModY[i];
                GameObject go = getField(cordX, cordY);
                if (go != null)
                {
                    f.addNeighbour(go.GetComponent<Field>());
                }
            }
        }
    }

    public static void endGame(string msg)
    {
        if (GameEngine.listener != null && GameEngine.listener.IsAlive)
        {
            GameEngine.listener.Abort();
        }
        lock (GameEngine.clientLock)
        {
            if (GameEngine.client != null)
            {
                GameEngine.client.Close();
            }
        }
        endGameManager.message = msg;
        SceneManager.LoadScene("endGameScene");
    }

    // Update is called once per frame
    void Update()
    {
        lock(dcFlagLock)
        {
            if(dcFlag)
            {
                dcFlag = false;
                if (GameEngine.listener != null && GameEngine.listener.IsAlive)
                {
                    GameEngine.listener.Abort();
                }
                lock (GameEngine.clientLock)
                {
                    if (GameEngine.client != null)
                    {
                        GameEngine.client.Close();
                    }
                }
                endGameManager.message = "[ERROR] Server has closed connection.";
                SceneManager.LoadScene("endGameScene");
            }
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (writing)
            {
                string message = inputField.text;
                if (message != "")
                {
                    NetworkEngine.send("msg#" + message);
                    log("You say: " + message);
                    inputField.text = "";
                    writing = false;
                }
            }
            else
            {
                inputField.ActivateInputField();
                writing = true;
            }
        }
    }

    public static GameObject getField(int x, int y)
    {
        if (x >= 0 && y >= 0 && x <= 2 * BOARD_SIZE_X && y <= 2 * BOARD_SIZE_Y)
            return GAME.board[x, y];
        return null;
    }

    public static Player getActivePlayer()
    {
        return GAME.activePlayer;
    }

    public static Player getLocalPlayer()
    {
        return GAME.localPlayer;
    }

    public static List<GameObject> constructSetOfFieldsObjects(Field f)
    {
        List<GameObject> fields = new List<GameObject>();
        for (int i = 0; i < GAME.setRequestModX.Length; i++)
        {
            int mx = GAME.setRequestModX[i];
            int my = GAME.setRequestModY[i];
            GameObject go = GameEngine.getField(f.x + mx, f.y + my);
            if (go != null)
            {
                fields.Add(go);
            }
        }
        return fields;
    }

    private static List<Field> constructSetOfFields(Field f)
    {
        List<Field> fields = new List<Field>();
        for (int i = 0; i < GAME.setRequestModX.Length; i++)
        {
            int mx = GAME.setRequestModX[i];
            int my = GAME.setRequestModY[i];
            GameObject go = GameEngine.getField(f.x + mx, f.y + my);
            if (go != null)
            {
                fields.Add(go.GetComponent<Field>());
            }
        }
        return fields;
    }

    public static void beginCast(Card card, CardObjectHandler cardObj)
    {

        if (GAME.activePlayer == GAME.localPlayer)
        {
            if (GAME.curCast == null && GAME.activePlayer.mana >= card.manaCost)
            {
                GAME.curCast = card;
                GAME.cardHandler = cardObj;
                CAST_EFFECT effect = card.beginCast();
                if (effect == CAST_EFFECT.SUCCESS)
                {
                    finishCast(true);
                }
                else if (effect == CAST_EFFECT.FAILURE)
                    finishCast(false);
                if (hoveredField != null)
                {
                    if (GAME.currentRequest == TARGET_TYPE.SET_OF_FIELDS)
                    {
                        List<Field> _set = new List<Field>(constructSetOfFields(hoveredField));
                        GAME.uiManager.setTargetedSet(_set);
                    }
                    else if (GAME.currentRequest == TARGET_TYPE.FIELD)
                    {
                        List<Field> _set = new List<Field>();
                        _set.Add(hoveredField);
                        GAME.uiManager.setTargetedSet(_set);
                    }

                }
                updateColors();
            }
            else
            {
                cardObj.endCast(false);
            }
        }
        else
        {
            cardObj.endCast(false);
        }
    }

    public void endTurnClick()
    {
        if (activePlayer == localPlayer)
        {
            if (curCast != null)
            {
                finishCast(false);
            }
            if (selectedField != null)
            {
                selectedField = null;
                selectedCreature = null;
                switched = false;
            }

            uiManager.zeroFields();
            gmsg("");
            switched = false;

            NetworkEngine.send("endTurn");
            log("Your turn has ended");
        }
    }

    public static Field mirrorField(Field f)
    {
        int x = BOARD_SIZE_X * 2 - f.x;
        int y = BOARD_SIZE_Y * 2 - f.y;
        return getField(x, y).GetComponent<Field>();
    }

    public static void opponentCast(ParametersQueue args)
    {
        Card c = getCardById(args.getInt());
        log("Opponent casts " + c.name);
        if (c is CreatureCard)
        {
            GAME.addToHistory(c, CARD_OWN.ENEMY);
        }
        else
        {
            GAME.addToHistory(c, CARD_OWN.NEUTRAL);
        }
        GAME.uiManager.opponentHand--;
        GAME.opPlayer.mana -= c.manaCost;
        c.opponentCast(args);

    }

    public static int distanceFromTower(Field f)
    {
        return GAME.towerDistances[f];
    }

    public static void opponentLClick(ParametersQueue args)
    {
        Field from = args.getField();
        Field to = args.getField();
        Creature invoker = (Creature)from.content;
        SpawnableObject target = to.content;
        if (target == null)
        {
            generateMovePaths(from, invoker);
            List<Field> path = new List<Field>();
            Field parent = to;
            while (parent != from)
            {
                path.Add(parent);
                parent = GAME.movePaths[parent];
            }
            List<Field> invokedPath = new List<Field>();
            for (int i = path.Count - 1; i >= 0; i--)
            {
                bool interrupt = path[i].onMoveThrough(invoker);
                invokedPath.Add(path[i]);
                if (interrupt)
                {
                    to = path[i];
                    break;
                }
            }
            GAME.StartCoroutine(invoker.MovementAnimation(invokedPath, from.contentObject));
            invoker.move(to);
        }
        else
        {
            invoker.attack(target);
        }
    }

    public static void opponentRClick(ParametersQueue args)
    {
        Field from = args.getField();
        Field to = args.getField();
        log("oppRClick from "+from.x +" " +from.y +" to " +to.x + " " + to.y);
        Creature invoker = (Creature)from.content;
        invoker.special(to);
    }

    private static void endStep()
    {
        foreach (GlobalEffect ef in GAME.effects)
        {
            ef.onEndTurn();
        }
        foreach (Field f in getValidFields())
        {
            if (f.content != null && f.content is Creature)
                ((Creature)f.content).endTurn();
        }
    }

    public static HashSet<GlobalEffect> getEffects()
    {
        return GAME.effects;
    }

    private static void upkeep()
    {
        foreach (GlobalEffect ef in GAME.effects)
        {
            ef.onStartTurn();
        }
        foreach (Field f in getValidFields())
        {
            if (f.content != null && f.content is Creature)
                ((Creature)f.content).startTurn();
        }
    }

    public static void startTurn()
    {
        log("Your turn starts");
        GAME.opPlayer.endTurn();
        endStep();
        GAME.activePlayer = GAME.localPlayer;
        upkeep();
        GAME.localPlayer.startTurn();
    }

    public static void startOpponentTurn()
    {
        GAME.localPlayer.endTurn();
        endStep();
        GAME.activePlayer = GAME.opPlayer;
        upkeep();
        GAME.opPlayer.startTurn();
    }

    public static void finishCast(bool result)
    {
        if (result)
        {
            log("You cast " + GAME.curCast.name);
            if (GAME.curCast is CreatureCard)
            {
                GAME.addToHistory(GAME.curCast, CARD_OWN.FRIENDLY);
            }
            else
            {
                GAME.addToHistory(GAME.curCast, CARD_OWN.NEUTRAL);
            }
            GAME.activePlayer.cast(GAME.curCast);
            NetworkEngine.send(GAME.curCast.getCastNetworkMessage());
            GAME.curCast.endCast();
        }
        else
        {
            GAME.curCast.cancelCast();
        }
        GAME.cardHandler.endCast(result);
        GAME.currentRequest = TARGET_TYPE.NONE;
        GAME.currentValidator = null;
        GAME.curCast = null;
        GAME.cardHandler = null;
        GAME.uiManager.zeroFields();
    }

    public static TARGET_TYPE getTargetType()
    {
        return GAME.currentRequest;
    }

    public static void requestFieldTarget(FieldValidator validator, string hint)
    {
        GAME.currentRequest = TARGET_TYPE.FIELD;
        GAME.currentValidator = validator;
        gmsg(hint);
    }

    public static void requestSetOfFieldsTarget(int[] modX, int[] modY, FieldValidator validator, string hint)
    {
        GAME.currentRequest = TARGET_TYPE.SET_OF_FIELDS;
        GAME.currentValidator = validator;
        GAME.setRequestModX = modX;
        GAME.setRequestModY = modY;
        gmsg(hint);
    }

    public static void log(string s)
    {
        GAME.debugLog.text += s + "\n";
    }

    public static void rightClick(Field f)
    {
        if (GAME.selectedField != null && GAME.selectedCreature.hasSpecial)
        {
            switched = !switched;
            if (switched)
            {
                gmsg("Select target for special ability");
            }
            else
            {
                gmsg("Select target to attack, or field to move");
            }

            updateRange();
            if (hoveredField != null)
            {
                onEnter(hoveredField);
            }
            GAME.uiManager.updateColors();
        }
    }

    public static void onLeave(Field f)
    {
        GAME.uiManager.zeroPath();
        GAME.uiManager.zeroSet();
    }

    public static void onEnter(Field f)
    {
        GAME.uiManager.zeroPath();
        if (GAME.curCast != null)
        {
            if (GAME.currentRequest == TARGET_TYPE.SET_OF_FIELDS)
            {
                List<Field> _set = constructSetOfFields(f);
                GAME.uiManager.setTargetedSet(_set);
            }
            else if (GAME.currentRequest == TARGET_TYPE.FIELD)
            {
                List<Field> _set = new List<Field>();
                _set.Add(f);
                GAME.uiManager.setTargetedSet(_set);
            }
        }
        else if (GAME.selectedCreature != null)
        {
            if (switched)
            {
                List<Field> _set = new List<Field>();
                for (int i = 0; i < GAME.selectedCreature.parentCard.specialModX.Length; i++)
                {
                    GameObject go = getField(f.x + GAME.selectedCreature.parentCard.specialModX[i], f.y + GAME.selectedCreature.parentCard.specialModY[i]);
                    if (go != null)
                    {
                        _set.Add(go.GetComponent<Field>());
                    }
                }
                GAME.uiManager.setTargetedSet(_set);
            }
            else if (f.content == null)
            {
                if (GAME.selectedCreature.canMove())
                {
                    if (GAME.selectedCreature.stats.speed >= GAME.moveDistances[f])
                    {
                        List<Field> path = new List<Field>();
                        Field parent = f;
                        while (parent != GAME.selectedField)
                        {
                            path.Add(parent);
                            parent = GAME.movePaths[parent];
                        }
                        GAME.uiManager.setPath(path);
                    }
                }
                List<Field> _set = new List<Field>();
                _set.Add(f);
                GAME.uiManager.setTargetedSet(_set);
            }
            else
            {
                List<Field> _set = new List<Field>();
                _set.Add(f);
                GAME.uiManager.setTargetedSet(_set);
            }
        }
    }

    public static void fieldClick(Field f)
    {
        gmsg("");
        if (GAME.activePlayer == GAME.localPlayer)
        {
            if (GAME.curCast != null)
            {
                if (GAME.currentRequest == TARGET_TYPE.FIELD)
                {
                    if (!f.invalidTarget && GAME.currentValidator.validate(f))
                    {
                        if (GAME.curCast.continueCast(f) == CAST_EFFECT.SUCCESS)
                        {
                            finishCast(true);
                        }
                    }
                    else
                    {
                        GAME.curCast.cancelCast();
                        finishCast(false);
                    }
                }
                else if (GAME.currentRequest == TARGET_TYPE.SET_OF_FIELDS)
                {
                    if (!f.invalidTarget && GAME.currentValidator.validate(f))
                    {
                        if (GAME.curCast.continueCast(f) == CAST_EFFECT.SUCCESS)
                            finishCast(true);
                    }
                    else
                    {
                        GAME.curCast.cancelCast();
                        finishCast(false);
                    }
                }
            }
            else
            {
                if (GAME.selectedField == null)
                {
                    if (f.content != null && f.content is Creature && ((Creature)f.content).owner == GAME.localPlayer)
                    {
                        GAME.selectedCreature = (Creature)f.content;
                        GAME.selectedField = f;
                        generateMovePaths(f);
                        updateRange();
                        gmsg("Select target to attack, or field to move");
                    }
                }
                else
                {
                    if (switched)
                    {
                        if (GAME.selectedCreature.canSpecial())
                        {
                            if (GAME.selectedCreature.parentCard.specialValidator.validate(f) && validateSpecialPath(GAME.selectedField, f))
                            {
                                GAME.selectedCreature.special(f);
                                NetworkEngine.send("rclick#" + GAME.selectedField.x + "#" + GAME.selectedField.y + "#" + f.x + "#" + f.y);
                            }
                        }
                        GAME.selectedField = null;
                        GAME.selectedCreature = null;
                        GAME.uiManager.zeroFields();
                        switched = false;
                    }
                    else if (f.content != null)
                    {
                        if (GAME.selectedCreature.canAttack())
                        {
                            if (f.content.owner != GAME.localPlayer && validateAttackPath(GAME.selectedField, f))
                            {
                                GAME.selectedCreature.attack(f.content);
                                NetworkEngine.send("lclick#" + GAME.selectedField.x + "#" + GAME.selectedField.y + "#" + f.x + "#" + f.y);
                            }
                        }
                        GAME.selectedField = null;
                        GAME.selectedCreature = null;
                        GAME.uiManager.zeroFields();
                    }
                    else
                    {
                        if (GAME.selectedCreature.canMove())
                        {
                            if (GAME.selectedCreature.stats.speed >= GAME.moveDistances[f])
                            {
                                log("moving");
                                List<Field> path = new List<Field>();
                                Field parent = f;
                                while (parent != GAME.selectedField)
                                {
                                    path.Add(parent);
                                    parent = GAME.movePaths[parent];
                                }
                                List<Field> invokedPath = new List<Field>();
                                for (int i = path.Count - 1; i >= 0; i--)
                                {
                                    bool interrupt = path[i].onMoveThrough(GAME.selectedCreature);
                                    invokedPath.Add(path[i]);
                                    if (interrupt)
                                    {
                                        log("interrupt at field " + path[i].x + " " + path[i].y);
                                        f = path[i];
                                        break;
                                    }
                                }
                                GAME.StartCoroutine(GAME.selectedCreature.MovementAnimation(invokedPath, GAME.selectedField.contentObject));
                                GAME.selectedCreature.move(f);
                                NetworkEngine.send("lclick#" + GAME.selectedField.x + "#" + GAME.selectedField.y + "#" + f.x + "#" + f.y);
                            }
                        }
                        GAME.selectedField = null;
                        GAME.selectedCreature = null;
                        GAME.uiManager.zeroFields();
                    }
                }

            }
        }
        updateColors();
    }

    public static void updateColors()
    {
        GAME.uiManager.updateColors();
    }

    public static bool validAction()
    {
        Field f = hoveredField;
        if (GAME.curCast != null)
        {
            if (GAME.currentRequest == TARGET_TYPE.FIELD)
            {
                if (!f.invalidTarget && GAME.currentValidator.validate(f))
                {
                    return true;
                }
                return false;
            }
            else if (GAME.currentRequest == TARGET_TYPE.SET_OF_FIELDS)
            {
                if (!f.invalidTarget && GAME.currentValidator.validate(f))
                {
                    return true;
                }
                return false;
            }
        }
        else
        {
            if (switched)
            {
                if (GAME.selectedCreature.canSpecial())
                {
                    if (GAME.selectedCreature.parentCard.specialValidator.validate(f) && validateSpecialPath(GAME.selectedField, f))
                    {
                        return true;
                    }
                }
                return false;
            }
            else if (f.content != null)
            {
                if (GAME.selectedCreature.canAttack())
                {
                    if (f.content.owner != GAME.localPlayer && validateAttackPath(GAME.selectedField, f))
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                if (GAME.selectedCreature.canMove())
                {
                    if (GAME.selectedCreature.stats.speed >= GAME.moveDistances[f])
                    {
                        return true;
                    }
                }
                return false;

            }
        }
        return false;

    }

    public static void updateRange()
    {
        List<Field> range = new List<Field>();
        foreach (Field f in getValidFields())
        {
            if (switched)
            {
                if (GAME.selectedCreature.canSpecial())
                {
                    if (GAME.selectedCreature.parentCard.specialValidator.validate(f) && validateSpecialPath(GAME.selectedField, f))
                    {
                        range.Add(f);
                    }
                }
            }
            else if (f.content != null)
            {
                if (GAME.selectedCreature.canAttack())
                {
                    if (f.content.owner != GAME.localPlayer && validateAttackPath(GAME.selectedField, f))
                    {
                        range.Add(f);
                    }
                }
            }
            else
            {
                if (GAME.selectedCreature.canMove())
                {
                    if (GAME.selectedCreature.stats.speed >= GAME.moveDistances[f])
                    {
                        range.Add(f);
                    }
                }
            }
        }
        GAME.uiManager.setRange(range);
    }

    public static Creature getSelectedCreature()
    {
        return GAME.selectedCreature;
    }

    public static bool validateAttackPath(Field beg, Field end, Creature invoker)
    {
        if (invoker.stats.range < GAME.attackDistances[beg][end])  
            return false;

        Field parent = GAME.attackPaths[beg][end];
        while (parent != beg)
        {
            if (!invoker.canAttackThrough(parent))
                return false;

            parent = GAME.attackPaths[beg][parent];
        }
        return true;
    }

    public static bool validateAttackPath(Field beg, Field end)
    {
        if (GAME.selectedCreature == null)
            return false;

        return validateAttackPath(beg, end, GAME.selectedCreature);
    }

    public static bool validateSpecialPath(Field beg, Field end)
    {
        if (GAME.selectedCreature == null)
            return false;

        if (GAME.selectedCreature.specialRange < GAME.attackDistances[beg][end])
            return false;

        Field parent = GAME.attackPaths[beg][end];
        while (parent != beg)
        {
            if (!GAME.selectedCreature.canSpecialThrough(parent))
                return false;
            parent = GAME.attackPaths[beg][parent];
        }
        return true;
    }

    public static List<Field> getValidFields()
    {
        List<Field> res = new List<Field>();
        for (int i = 0; i <= BOARD_SIZE_X * 2; i++)
        {
            for (int j = 0; j <= BOARD_SIZE_Y * 2; j++)
            {
                GameObject go = getField(i, j);
                if (go != null)
                {
                    Field fld = go.GetComponent<Field>();
                    res.Add(fld);
                }
            }
        }
        return res;
    }

    public static void opponentDrawCard()
    {
        GAME.uiManager.opponentDrawCard();
    }

    public static void generateLayers()
    {
        Field f = getField(8, 0).GetComponent<Field>();
        GAME.towerDistances = new Dictionary<Field, int>();
        Queue<Field> q = new Queue<Field>();
        q.Enqueue(f);

        foreach (Field fld in getValidFields())
        {
            GAME.towerDistances[fld] = int.MaxValue;
        }

        GAME.towerDistances[f] = 0;
        while (q.Count > 0)
        {
            Field w = q.Dequeue();
            foreach (Field n in w.getNeighbours())
            {
                if (GAME.towerDistances[n] == int.MaxValue)
                {
                    GAME.towerDistances[n] = GAME.towerDistances[w] + 1;
                    q.Enqueue(n);
                }
            }
        }
    }

    public static void generateMovePaths(Field f, Creature invoker)
    {
        GAME.moveDistances = new Dictionary<Field, int>();
        Queue<Field> q = new Queue<Field>();
        q.Enqueue(f);

        foreach (Field fld in getValidFields())
        {
            GAME.movePaths[fld] = null;
            GAME.moveDistances[fld] = int.MaxValue;
        }

        GAME.movePaths[f] = f;
        GAME.moveDistances[f] = 0;
        while (q.Count > 0)
        {
            Field w = q.Dequeue();
            foreach (Field n in w.getNeighbours())
            {
                if (GAME.movePaths[n] == null && invoker.canMoveThrough(n))
                {
                    GAME.movePaths[n] = w;
                    GAME.moveDistances[n] = GAME.moveDistances[w] + 1;
                    q.Enqueue(n);
                }
            }
        }
    }

    public static void generateMovePaths(Field f)
    {
        if (GAME.selectedCreature == null)
            return;

        generateMovePaths(f, GAME.selectedCreature);
    }

    public static void generateAttackPaths()
    {
        GAME.attackDistances.Clear();
        GAME.attackPaths.Clear();
        foreach (Field fld in getValidFields())
        {
            GAME.attackDistances.Add(fld, new Dictionary<Field, int>());
            GAME.attackPaths.Add(fld, new Dictionary<Field, Field>());
            foreach (Field trg in getValidFields())
            {
                GAME.attackDistances[fld].Add(trg, -1);
                GAME.attackPaths[fld].Add(trg, null);
            }
            generateAttackPathsForField(fld);
        }
    }

    public static void generateAttackPathsForField(Field f)
    {
        Queue<Field> q = new Queue<Field>();
        q.Enqueue(f);
        GAME.attackPaths[f][f] = f;
        GAME.attackDistances[f][f] = 0;
        while (q.Count > 0)
        {
            Field w = q.Dequeue();
            foreach (Field n in w.getNeighbours())
            {
                if (GAME.attackPaths[f][n] == null)
                {
                    q.Enqueue(n);
                    GAME.attackPaths[f][n] = w;
                    GAME.attackDistances[f][n] = GAME.attackDistances[f][w] + 1;
                }
            }
        }
    }

    public static void cancelAction()
    {
        if (GAME.curCast != null)
        {
            GAME.curCast.cancelCast();
            finishCast(false);
        }
    }

    public static void drawCardObject(Card card, int slot)
    {
        GameObject newCardObject = null;
        if (card is CreatureCard)
        {
            newCardObject = AssetManager.get("creatureCardObject");
            GameObject nObject = Instantiate(newCardObject, new Vector3(SLOTX[slot], SLOTY), Quaternion.identity) as GameObject;
            nObject.transform.SetParent(GAME.uiCanvas.transform, false);
            nObject.GetComponent<CardObjectHandler>().setCard(card);
            nObject.GetComponent<CardObjectHandler>().setDisplay();
        }
        else
        {
            newCardObject = AssetManager.get("spellCardObject");
            GameObject nObject = Instantiate(newCardObject, new Vector3(SLOTX[slot], SLOTY), Quaternion.identity) as GameObject;
            nObject.transform.SetParent(GAME.uiCanvas.transform, false);
            nObject.GetComponent<CardObjectHandler>().setCard(card);
            nObject.GetComponent<CardObjectHandler>().setDisplay();
        }


    }

    public static void setPreview(Card card)
    {
        card.showPreview(GAME.uiManager);
    }
}
