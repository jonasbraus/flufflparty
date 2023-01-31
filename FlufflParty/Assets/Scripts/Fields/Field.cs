using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Field : MonoBehaviour
{
    //Liste der nächsten Feld, die auf das diesige Feld folgen
    [SerializeField] private Field[] target;
    [SerializeField] private Arrow[] directionalArrows;

    /*
     * Field Definition:
     */
    
    public Field[] Target
    {
        get
        {
            return target;
        }
    }

    public Arrow[] DirectionalArrow
    {
        get
        {
            return directionalArrows;
        }
    }

    //Feld aktion (Münzen etc..)
    public abstract int Action(int playerIndex);
}
