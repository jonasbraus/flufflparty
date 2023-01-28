using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public bool activated = false;
    protected bool wurfelt = false;
    
    public GameObject camera;
    protected Vector3 velocity = Vector3.zero;

    protected Vector3 cameraOffset = new Vector3(-5.81f, 8.13f, 7.6f);
    
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
