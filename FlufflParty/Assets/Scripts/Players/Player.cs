using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public bool activated = false;
    protected bool wurfelt = false;
    
    //Aktiviert den Player
    public void Activate()
    {
        activated = true;
        wurfelt = false;
    }

    //Deaktiviert den Player
    public void DeActivate()
    {
        activated = false;
    }
}
