using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
    public static GameObject gObject;

    private void Start()
    {
        gObject = gameObject;
    }
}
