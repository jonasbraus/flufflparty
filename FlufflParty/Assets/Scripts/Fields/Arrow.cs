using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    private bool clicked = false;

    private void Start()
    {
        Deactivate();
    }
    
    public bool IsClicked()
    {
        return clicked;
    }

    public void Activate()
    {
        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        clicked = false;
        gameObject.SetActive(false);
    }
    
    private void OnMouseDown()
    {
        clicked = true;
    }
}
