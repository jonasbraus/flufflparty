using System.Collections;
using System.Collections.Generic;
using System.Net.Mail;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PlayablePlayer : Player
{
    public Field nextField;
    private Field currentField;
    public GameObject dice;
    private Dice diceScript;
    private Vector3 moveTo = Vector3.zero;
    private Vector3 lastPos = Vector3.zero;
    private float speed = 5;
    private float interPol = 0;
    private int wurfelZahl = 0;

    private bool wait = false;


    private float timeSinceWurfeln = 0;
    private float timeSinceStop = 0;

    private Vector3 useLess = new Vector3();

    public Client client;

    public override void Init()
    {
        TMP_Text[] playerTexts = GetComponentsInChildren<TMP_Text>();
        playerTexts[0].text = playerTexts[1].text = name;

        activated = false;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 61;
        diceScript = dice.GetComponent<Dice>();
    }

    private void Update()
    {
        /*
         if (Input.GetMouseButtonDown(0))
        {
            eventstop = false;
            client.EventStopFinished();
        }
        */
        if (activated && !name.Equals(""))
        {
            
            //Generiere Würfelzahl von 1-6 wenn gerade gewürfelt werden darf
            if ((Input.GetMouseButtonUp(0) && !uiHandler.MapOpen) && wurfelZahl == 0 && !wurfelt)
            {
                    if(!Physics.Raycast(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -200), Vector3.forward))
                    {
                        wurfelt = true;
                        wurfelZahl = Random.Range(1, 7);
                        textLeftMoves.text = wurfelZahl + "";

                        client.SendWurfeln(wurfelZahl);

                        //nächstes Feld anvisieren
                        TargetNextField();

                        //stop the dice
                        diceScript.StopRandom(wurfelZahl);

                        //delay for walk start
                        timeSinceWurfeln = Time.time;
                    }
                
            
            }
        }
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
                        InterPolerate();
                    }
                }
                //Würfelzahl um 1 verringern falls noch züge übrig sind
                else if (wurfelZahl > 0)
                {
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
                    //TODO: temp spieler finsihed erst, wenn field action ausgeführt wurde
                    if (wurfelt)
                    {
                        int fieldReturnStatus = currentField.Action(index);

                        switch (fieldReturnStatus)
                        {
                            case 0:
                                client.SendCoinFieldAction(0);
                                AddCoins(3);
                                break;
                            case 1:
                                client.SendCoinFieldAction(1);
                                AddCoins(-3);
                                break;
                        }

                        Invoke("Finish", 2);
                        activated = false;
                    }
                    else
                    {
                        //start the dice
                        dice.transform.position = Vector3.SmoothDamp(dice.transform.position,
                            transform.position + Vector3.up * 2, ref useLess, 0.2f);
                        diceScript.StartRandom();
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
            transform.position = new Vector3(nextPoint.x, transform.position.y, nextPoint.z);
            interPol += (Time.deltaTime * speed) / Mathf.Abs(Vector3.Distance(moveTo, lastPos));
        }
    }

    private void Finish()
    {
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

            if(currentStar.IsActive)
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
}