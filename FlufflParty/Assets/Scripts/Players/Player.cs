using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Player : MonoBehaviour
{
    public string name = "";
    public bool activated = false;
    protected bool wurfelt = false;
    
    public GameObject camera;
    protected Vector3 velocity = Vector3.zero;

    protected Vector3 cameraOffset = new Vector3(-2.81f, 8.13f, 5.6f);
    
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

    public abstract void Init();
}
