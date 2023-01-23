using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Field : MonoBehaviour
{
    //Liste der nächsten Feld, die auf das diesige Feld folgen
    [SerializeField] private Field[] target;

    public Field[] Target
    {
        get
        {
            return target;
        }
    }

    //Feld aktion (Münzen etc..)
    public abstract void Action();
}
