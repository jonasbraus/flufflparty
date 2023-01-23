using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Field : MonoBehaviour
{
    [SerializeField] private GameObject[] target;

    public GameObject[] Target
    {
        get
        {
            return target;
        }
    }

    public abstract void Action();
}
