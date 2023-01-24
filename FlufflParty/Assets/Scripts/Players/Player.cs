using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Player : MonoBehaviour
{
    [SerializeField] private Field nextField;
    private Field currentField;
    [SerializeField] private GameObject dice;
    private Vector3 moveTo = Vector3.zero;
    private Vector3 lastPos = Vector3.zero;
    private float speed = 5;
    private float interPol = 0;
    private int wurfelZahl = 0;

    private bool activated = true;
    private bool wait = false;

    private void Start()
    {
        
    }
    
    private void Update()
    {
        if(activated)
        {
            //Generiere Würfelzahl von 1-6 wenn gerade gewürfelt werden darf
            if (Input.GetKeyDown(KeyCode.Space) && wurfelZahl == 0)
            {
                wurfelZahl = Random.Range(1, 7);
                Debug.Log("Test: gewürfelt wurde " + wurfelZahl);
                //nächstes Feld anvisieren
                TargetNextField();
            }
        }
    }

    private void FixedUpdate()
    {
        if (activated)
        {
            if (!wait)
            {
                //Laufe zum nöchsten Feld, falls noch Züge übrig sind
                if (interPol < 1 && wurfelZahl > 0)
                {
                    InterPolerate();
                }
                //Würfelzahl um 1 verringern falls noch züge übrig sind
                else if (wurfelZahl > 0 )
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
                
                }
            }
            else
            {
                foreach(Arrow a in nextField.DirectionalArrow)
                {
                    a.Activate();
                }
                
                if (nextField.DirectionalArrow[0].IsClicked())
                {
                    foreach(Arrow a in nextField.DirectionalArrow)
                    {
                        a.Deactivate();
                    }
                    nextField = nextField = nextField.Target[0];
                    moveTo = nextField.transform.position;
                    wait = false;
                }
                else if (nextField.DirectionalArrow[1].IsClicked())
                {
                    foreach(Arrow a in nextField.DirectionalArrow)
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
        currentField = nextField;
        if(nextField.Target.Length == 1)
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

    //Aktiviert den Player
    public void Activate()
    {
        activated = true;
    }

    //Deaktiviert den Player
    public void DeActivate()
    {
        activated = false;
    }
}