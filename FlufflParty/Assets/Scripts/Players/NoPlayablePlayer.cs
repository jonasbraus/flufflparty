using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NoPlayablePlayer : Player
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
    private bool lastActivated = false;

    private float timeSinceWurfeln = 0;

    private Vector3 useLess = new Vector3();

    public override void Init()
    {
        GetComponentInChildren<TMP_Text>().text = name;
        
        activated = false;
        diceScript = dice.GetComponent<Dice>();
    }

    public void Wurfeln(int wurfelZahl)
    {
        wurfelt = true;
        this.wurfelZahl = wurfelZahl;
        //nächstes Feld anvisieren
        TargetNextField();

        //stop the dice
        diceScript.StopRandom(wurfelZahl);

        //delay for walk start
        timeSinceWurfeln = Time.time;
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
                        InterPolerate();
                    }
                }
                //Würfelzahl um 1 verringern falls noch züge übrig sind
                else if (wurfelZahl > 0)
                {
                    wurfelZahl--;

                    //Visiere das nächste Feld an falls noch ein Zug übrig ist (Player steht gerade auf einem Feld oder läuft darüber)
                    if (wurfelZahl > 0)
                    {
                        TargetNextField();
                    }
                }
                //Der Spieler steht gerade
                else
                {
                    if (!wurfelt)
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

    //Bewegt den Player von einem Punkt(lastPos) zu einem anderen Punkt(moveTo) mit konstanter Geschwindigkeit
    private void InterPolerate()
    {
        Vector3 nextPoint = Vector3.Lerp(lastPos, moveTo, interPol);
        transform.position = new Vector3(nextPoint.x, transform.position.y, nextPoint.z);
        interPol += (Time.deltaTime * speed) / Mathf.Abs(Vector3.Distance(moveTo, lastPos));
    }
    
}
