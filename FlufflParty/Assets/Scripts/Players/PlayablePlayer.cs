using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayablePlayer : Player
{
    public Field nextField;
    public GameObject dice;
    private Dice diceScript;
    private Vector3 moveTo = Vector3.zero;
    private Vector3 lastPos = Vector3.zero;
    private float speed = 5;
    private float interPol = 0;
    private int wurfelZahl = 0;
    private bool checkDoubleDice = false;
    private RandomRotator rotator;
    private int rotatorStopIndex = 0;

    private bool wait = false;


    private float timeSinceWurfeln = 0;
    private float timeSinceStop = 0;

    private Vector3 useLess = new Vector3();

    public Client client;

    private static PlayablePlayer instance;


    //Animation:
    private Vector3 lastPosAnim = Vector3.zero;
    private Quaternion rotation = Quaternion.identity;
    private bool jumpRequest = false;
    private float lastTimeDoneIdleAction = 0, idleActionDelay = 5;

    public override void Init()
    {
        TMP_Text[] playerTexts = GetComponentsInChildren<TMP_Text>();
        playerTexts[0].text = playerTexts[1].text = name;

        activated = false;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 61;
        diceScript = dice.GetComponent<Dice>();

        instance = this;

        nameSign = playerTexts[1].gameObject;
    }

    public static PlayablePlayer GetCurrentInstance()
    {
        return instance;
    }

    private void Start()
    {
        base.Start();
        lastPosAnim = transform.position;
    }

    private void Update()
    {
        //Animation Control
        if (jumpRequest)
        {
            jumpRequest = false;
            animator.SetInteger("value", 2);
            lastTimeDoneIdleAction = Time.time;
        }
        else if (Vector3.Distance(transform.position, lastPosAnim) > 0)
        {
            animator.SetInteger("value", 1);
        }
        else if (Time.time - lastTimeDoneIdleAction > idleActionDelay)
        {
            lastTimeDoneIdleAction = Time.time;
            idleActionDelay = Random.Range(2f, 5f);
            int randomAction = Random.Range(3, 6);
            animator.SetInteger("value", randomAction);
        }
        else
        {
            animator.SetInteger("value", 0);
        }
        //END

        if (activated && !name.Equals(""))
        {
            //Generiere Würfelzahl von 1-6 wenn gerade gewürfelt werden darf
            if ((Input.GetMouseButtonUp(0) && !uiHandler.MapOpen) && wurfelZahl == 0 && !wurfelt)
            {
                if (!Physics.Raycast(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -200), Vector3.forward))
                {
                    DiceRollRoutine();
                }
            }
        }

        lastPosAnim = transform.position;
    }

    private void DiceRollRoutine()
    {
        diceScript.SetMaterial(0);

        jumpRequest = true;

        wurfelt = true;
        wurfelZahl = Random.Range(1, 7);

        switch (activeItem)
        {
            case Item.Type.Mushroom:
                wurfelZahl += 3;
                activeItem = Item.Type.None;
                diceScript.SetMaterial(2);
                break;
            case Item.Type.DoubleDice:
                checkDoubleDice = true;
                activeItem = Item.Type.None;
                diceScript.SetMaterial(1);
                break;
        }

        textLeftMoves.text = wurfelZahl + "";

        client.SendWurfeln(wurfelZahl);

        //nächstes Feld anvisieren
        TargetNextField();

        //stop the dice
        diceScript.StopRandom(wurfelZahl);

        //delay for walk start
        timeSinceWurfeln = Time.time;
    }

    private void FixedUpdate()
    {
        if (activated && !name.Equals(""))
        {
            camera.transform.position = Vector3.SmoothDamp(camera.transform.position, transform.position + cameraOffset,
                ref velocity, 0.2f);

            if (!wait)
            {
                //Laufe zum nöchsten Feld, falls noch Züge übrig sind
                if (interPol < 1 && wurfelZahl > 0)
                {
                    //delay
                    if (Time.time - timeSinceWurfeln > 2)
                    {
                        nameSign.SetActive(true);
                        dice.SetActive(false);
                        InterPolerate();
                    }
                }
                //Würfelzahl um 1 verringern falls noch züge übrig sind
                else if (wurfelZahl > 0)
                {
                    InterPolerate();
                    wurfelZahl--;
                    textLeftMoves.text = wurfelZahl + "";

                    //the next field is the current field XD
                    currentField = nextField;

                    //Visiere das nächste Feld an falls noch ein Zug übrig ist (Player steht gerade auf einem Feld oder läuft darüber)
                    if (wurfelZahl > 0)
                    {
                        TargetNextField();
                    }
                }
                //Der Spieler steht gerade
                else
                {
                    if (currentField != null)
                    {
                        if (currentField.placedItem != null)
                        {
                            Trap t = currentField.placedItem.GetComponent<Trap>();

                            if (t.target.index != index)
                            {
                                int coinsAmount = (int)(coins * 0.1f);
                                AddCoins(-coinsAmount);
                                t.target.AddCoins(coinsAmount);
                                currentField.placedItem = null;
                                Destroy(t.gameObject);
                            }
                        }
                    }

                    Vector3 currentRotation = Quaternion.ToEulerAngles(transform.rotation);
                    currentRotation.y = -34;
                    transform.rotation = Quaternion.Euler(currentRotation);

                    int fieldReturnStatus = -1;
                    if (currentField != null)
                    {
                        fieldReturnStatus = currentField.Action(index);
                    }

                    //TODO: temp spieler finsihed erst, wenn field action ausgeführt wurde
                    if (wurfelt)
                    {
                        switch (fieldReturnStatus)
                        {
                            case 0: //Blaues Feld
                                client.SendCoinFieldAction(0);
                                AddCoins(3);
                                break;
                            case 1: //Rotes Feld
                                client.SendCoinFieldAction(1);
                                AddCoins(-3);
                                break;
                            case 2: //LuckField
                                int randCoins1 = Random.Range(2, 6);
                                int randCoins2 = Random.Range(1, 4);
                                int randCoins3 = Random.Range(-6, 0);

                                RandomRotator rotator = uiHandler.rotator.GetComponent<RandomRotator>();
                                this.rotator = rotator;
                                uiHandler.ActivateRotator();
                                rotator.SetOptionsText(0, randCoins1 + " Coins");
                                rotator.SetOptionsText(1, randCoins2 + " Coins");
                                rotator.SetOptionsText(2, randCoins3 + " Coins");
                                rotator.SetOptionsText(3, "Random Item");
                                rotator.SetHint(0);

                                rotator.StartRandom();
                                client.SendStartRotator(0, 0, 1, 2, randCoins1, randCoins2, -randCoins3, 0, 0);

                                int randomOption = Random.Range(0, 4);
                                switch (randomOption)
                                {
                                    case 0:
                                        AddCoins(randCoins1);
                                        client.SendCoins(randCoins1);
                                        break;
                                    case 1:
                                        AddCoins(randCoins2);
                                        client.SendCoins(randCoins2);
                                        break;
                                    case 2:
                                        AddCoins(randCoins3);
                                        client.SendCoins(randCoins3);
                                        break;
                                    case 3:
                                        int randomItem = Random.Range(0, 3);
                                        AddItem(client.items[randomItem]);
                                        break;
                                }

                                rotatorStopIndex = randomOption;
                                Invoke("StopRotator", 2);
                                Invoke("ActivateLayout1", 5.5f);
                                break;

                            case 3: //Item Field keine Ahnung ob das funktioniert XD muss man mal testen; Keine Ahnung was SendStartRotator an den Server weitersendet MUSS überarbeitet werden
                                RandomRotator rotatorItemField = uiHandler.rotator.GetComponent<RandomRotator>();
                                this.rotator = rotatorItemField;
                                uiHandler.ActivateRotator();
                                rotatorItemField.SetOptionsText(0, "Mushroom");
                                rotatorItemField.SetOptionsText(1, "Double Dice");
                                rotatorItemField.SetOptionsText(2, "Trap");
                                rotatorItemField.SetOptionsText(3, "Trap");
                                rotatorItemField.SetHint(1);

                                rotatorItemField.StartRandom();
                                client.SendStartRotator(0, 1, 2, 2, 0, 0, 0, 0, 1);

                                int randomItemField = Random.Range(0, 4);

                                rotatorStopIndex = randomItemField;

                                randomItemField = randomItemField == 3 ? randomItemField - 1 : randomItemField;
                                AddItem(client.items[randomItemField]);

                                Invoke("StopRotator", 2);
                                Invoke("ActivateLayout1", 5.5f);
                                break;
                            
                            /*case 4: //UnluckyField
                                int looseRandCoins1 = Random.Range(-6, -2);
                                int looseRandCoins2 = Random.Range(-4, -1);
                                int looseRandCoins3 = Random.Range(-6, 0);

                                RandomRotator rotatorUnluckyField = uiHandler.rotator.GetComponent<RandomRotator>();
                                this.rotator = rotatorUnluckyField;
                                uiHandler.ActivateRotator();
                                rotatorUnluckyField.SetOptionsText(0, "Loose" + looseRandCoins1 + " Coins");
                                rotatorUnluckyField.SetOptionsText(1, "Loose" + looseRandCoins2 + " Coins");
                                rotatorUnluckyField.SetOptionsText(2, "Loose" + looseRandCoins3 + " Coins");
                                rotatorUnluckyField.SetOptionsText(3, "Loose a star");
                                rotatorUnluckyField.SetHint(0);

                                rotatorUnluckyField.StartRandom();
                                client.SendStartRotator(0, 0, 1, 2, looseRandCoins1, looseRandCoins2, looseRandCoins3, 0, 0); //TODO: Überprüfen, keine Ahnung ob das stimmt :D

                                int randomOptionUnluckyField = Random.Range(0, 4);
                                switch (randomOptionUnluckyField)
                                {
                                    case 0:
                                        AddCoins(looseRandCoins1);
                                        client.SendCoins(looseRandCoins1);
                                        break;
                                    case 1:
                                        AddCoins(looseRandCoins2);
                                        client.SendCoins(looseRandCoins2);
                                        break;
                                    case 2:
                                        AddCoins(looseRandCoins3);
                                        client.SendCoins(looseRandCoins3);
                                        break;
                                    case 3:
                                        AddStars(-1);
                                        client.SendLostStar();
                                        break;
                                }

                                rotatorStopIndex = randomOptionUnluckyField;
                                Invoke("StopRotator", 2);
                                Invoke("ActivateLayout1", 5.5f);
                                break;*/
                        }

                        if (!checkDoubleDice)
                        {
                            if (fieldReturnStatus != 2 && fieldReturnStatus != 3)
                            {
                                Invoke("Finish", 3);
                            }

                            activated = false;
                        }
                        else
                        {
                            if (fieldReturnStatus != 2 && fieldReturnStatus != 3)
                            {
                                Invoke("RollDiceAgain", 3);
                            }
                        }
                    }
                    else
                    {
                        StartDice();
                    }
                }
            }
            else
            {
                foreach (Arrow a in nextField.DirectionalArrow)
                {
                    a.Activate();
                }

                if (nextField.DirectionalArrow[0].IsClicked())
                {
                    client.SendArrowSelected(0);

                    foreach (Arrow a in nextField.DirectionalArrow)
                    {
                        a.Deactivate();
                    }

                    nextField = nextField = nextField.Target[0];
                    moveTo = nextField.transform.position;
                    wait = false;
                }
                else if (nextField.DirectionalArrow[1].IsClicked())
                {
                    client.SendArrowSelected(1);
                    foreach (Arrow a in nextField.DirectionalArrow)
                    {
                        a.Deactivate();
                    }

                    nextField = nextField = nextField.Target[1];
                    moveTo = nextField.transform.position;
                    wait = false;
                }
            }
        }
    }

    //Das nächste Feld aufgrund des aktuellen Feldes anvisieren
    private void TargetNextField()
    {
        interPol = 0;
        lastPos = transform.position;
        if (nextField.Target.Length == 1)
        {
            nextField = nextField.Target[0];
            moveTo = nextField.transform.position;
        }
        else
        {
            wait = true;
        }
    }

    //Bewegt den Player von einem Punkt(lastPos) zu einem anderen Punkt(moveTo) mit konstanter Geschwindigkeit

    private Vector3 nextPoint;

    private void InterPolerate()
    {
        if (!eventstop)
        {
            nextPoint = Vector3.Lerp(lastPos, moveTo, interPol);

            //Rotation in move direction
            Vector3 lookPos = nextPoint - transform.position;
            lookPos.y = 0;
            if (lookPos != Vector3.zero)
            {
                Quaternion rotation = Quaternion.LookRotation(lookPos);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationDamping);
            }
            //END

            transform.position = new Vector3(nextPoint.x, transform.position.y, nextPoint.z);
            interPol += (Time.deltaTime * speed) / Mathf.Abs(Vector3.Distance(moveTo, lastPos));
        }
    }

    private void Finish()
    {
        imageCurrentItem.color = new Color(0, 0, 0, 0);
        diceScript.SetMaterial(0);
        client.SendFinished();
    }


    private Star currentStar = null;

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.name.Equals("ItemShopTrigger"))
        {
            base.OnTriggerEnter(collider);
            uiHandler.ActivateLayout2(this);
        }
        else if (collider.name.Equals("StarTrigger"))
        {
            currentStar = collider.GetComponent<Star>();

            if (currentStar.IsActive)
            {
                base.OnTriggerEnter(collider);
                uiHandler.ActivateLayoutBuyStar(this);
            }
        }
    }

    public void BuyStar(bool buy)
    {
        client.SendEventStopFinished();

        if (buy)
        {
            currentStar.Buy(this);
            client.SendBuyStar();
            AddStars(1);
        }
        else
        {
            currentStar = null;
        }
    }

    public void SkipShop()
    {
        client.SendEventStopFinished();
    }

    public new void AddItem(Item item)
    {
        base.AddItem(item);
        client.SendBuyItem(item);
    }

    public void StartDice()
    {
        gameObject.SetActive(true);
        //start the dice
        dice.transform.position = Vector3.SmoothDamp(dice.transform.position,
            transform.position + Vector3.up * 3f, ref useLess, 0.2f);
        diceScript.StartRandom();
    }

    public void ActivateItem(int index)
    {
        activeItem = items[index].type;
        imageCurrentItem.sprite = items[index].sprite;
        imageCurrentItem.color = new Color(1, 1, 1, 1);
        itemInfoImages[index].color = new Color(0, 0, 0, 0);
        client.SendActiveItem((byte)index);
        if (items[index].type == Item.Type.Trap)
        {
            GameObject temp = Instantiate(items[index].gameObject);
            Trap t = temp.GetComponent<Trap>();
            currentField.placedItem = t.gameObject;
            t.target = gameObject.GetComponent<PlayablePlayer>();
            temp.transform.position = currentField.transform.position + (Vector3.up / 2);
            activeItem = Item.Type.None;
        }

        items[index] = null;
    }

    private void ActivateLayout1()
    {
        uiHandler.ActivateLayout1();
        Finish();
    }

    private void StopRotator()
    {
        client.SendStopRotator(rotatorStopIndex);
        rotator.Stop(rotatorStopIndex);
    }

    private void RollDiceAgain()
    {
        checkDoubleDice = false;
        wurfelt = false;
    }
}