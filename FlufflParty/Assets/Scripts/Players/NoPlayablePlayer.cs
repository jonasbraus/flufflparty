using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NoPlayablePlayer : Player
{
    public Field nextField;
    public GameObject dice;
    private Dice diceScript;
    private Vector3 moveTo = Vector3.zero;
    private Vector3 lastPos = Vector3.zero;
    private float speed = 5;
    private float interPol = 0;
    private int wurfelZahl = 0;
    
    private bool wait = false;
    private bool lastActivated = false;

    private float timeSinceWurfeln = 0;

    private Vector3 useLess = new Vector3();

    private bool checkDoubleDice = false;
    
    //Animation:
    private Vector3 lastPosAnim = Vector3.zero;
    private bool jumpRequest = false;
    private float lastTimeDoneIdleAction = 0, idleActionDelay = 5;


    public override void Init()
    {
        TMP_Text[] playerTexts = GetComponentsInChildren<TMP_Text>();
        playerTexts[0].text = playerTexts[1].text = name;
        
        activated = false;
        diceScript = dice.GetComponent<Dice>();
        
    }

    private void Start()
    {
        base.Start();
        lastPosAnim = transform.position;
    }


    private void Update()
    {
        // //Animation Control
        if (Vector3.Distance(transform.position, lastPosAnim) > 0f)
        {
            animator.SetInteger("value", 1);
        }
        else if(jumpRequest)
        {
            jumpRequest = false;
            animator.SetInteger("value", 2);
            lastTimeDoneIdleAction = Time.time;
        }
        else if(Time.time - lastTimeDoneIdleAction > idleActionDelay)
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
        
        lastPosAnim = transform.position;
    }

    public void BuyStar()
    {
        GameObject.Find("StarPositions").GetComponent<StarController>().SwitchStar();
        AddCoins(-20);
        AddStars(1);
    }
    
    public void CoinFieldAction(int action)
    {
        if (action == 0)
        {
            AddCoins(3);
        }
        else
        {
            AddCoins(-3);
        }
    }

    public void Wurfeln(int wurfelZahl)
    {
        jumpRequest = true;
        
        wurfelt = true;
        this.wurfelZahl = wurfelZahl;

        switch (activeItem)
        {
            case Item.Type.Mushroom: wurfelZahl += 3; activeItem = Item.Type.None; break;
            case Item.Type.DoubleDice: checkDoubleDice = true; activeItem = Item.Type.None; break;
        }
        
        //nächstes Feld anvisieren
        TargetNextField();

        //stop the dice
        diceScript.StopRandom(wurfelZahl);

        //delay for walk start
        timeSinceWurfeln = Time.time;
        textLeftMoves.text = wurfelZahl + "";
    }

    public void ArrowSelect(int idx)
    {
        foreach (Arrow a in nextField.DirectionalArrow)
        {
            a.Deactivate();
        }
        
        nextField = nextField.Target[idx]; 
        moveTo = nextField.transform.position;
        wait = false;
    }
    
    
    private void FixedUpdate()
    {
        
        if (activated && !name.Equals(""))
        {
            camera.transform.position = Vector3.SmoothDamp(camera.transform.position, transform.position + cameraOffset, ref velocity, 0.2f);
            if (!wait)
            {
                //Laufe zum nöchsten Feld, falls noch Züge übrig sind
                if (interPol < 1 && wurfelZahl > 0)
                {
                    //delay
                    if (Time.time - timeSinceWurfeln > 2)
                    {
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
                            AddCoins(-3);
                            t.target.AddCoins(3);
                            currentField.placedItem = null;
                            Destroy(t.gameObject);
                        }
                    }

                    Vector3 currentRotation = Quaternion.ToEulerAngles(transform.rotation);
                    currentRotation.y = -34;
                    transform.rotation = Quaternion.Euler(currentRotation);
                    
                    if (!wurfelt)
                    {
                       StartDice(); 
                    }
                    else
                    {
                        if (checkDoubleDice)
                        {
                            checkDoubleDice = false;
                            wurfelt = false;  
                        }
                    }
                }
            }
            else
            {
                foreach (Arrow a in nextField.DirectionalArrow)
                {
                    a.Activate();
                }
            }
        }
    }

    //Das nächste Feld aufgrund des aktuellen Feldes anvisieren
    private void TargetNextField()
    {
        interPol = 0;
        lastPos = transform.position;
        currentField = nextField;
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

    private Vector3 nextPoint;

    //Bewegt den Player von einem Punkt(lastPos) zu einem anderen Punkt(moveTo) mit konstanter Geschwindigkeit
    private void InterPolerate()
    {
        if (!eventstop)
        {
            nextPoint = Vector3.Lerp(lastPos, moveTo, interPol);
            //Rotation in move direction
            Vector3 lookPos = nextPoint - transform.position;
            lookPos.y = 0;
            if(lookPos != Vector3.zero)
            {
                Quaternion rotation = Quaternion.LookRotation(lookPos);

                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationDamping);
            }
            //END
            transform.position = new Vector3(nextPoint.x, transform.position.y, nextPoint.z);
            interPol += (Time.deltaTime * speed) / Mathf.Abs(Vector3.Distance(moveTo, lastPos));
        }
    }

    private Star currentStar;
    
    private void OnTriggerEnter(Collider collider)
    {
        if (collider.name.Equals("ItemShopTrigger"))
        {
            base.OnTriggerEnter(collider);
        }
        else if (collider.name.Equals("StarTrigger"))
        {
            currentStar = collider.GetComponent<Star>();

            if(currentStar.IsActive)
            {
                base.OnTriggerEnter(collider);
            }
        }
    }

    public void StartDice()
    {
        //start the dice
        dice.transform.position = Vector3.SmoothDamp(dice.transform.position,
            transform.position + Vector3.up * 3, ref useLess, 0.2f);
        diceScript.StartRandom();
    }
}
