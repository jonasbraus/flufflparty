using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHandler : MonoBehaviour
{
    [SerializeField] private GameObject layout0;
    [SerializeField] private GameObject layout1;

    private void Start()
    {
        layout1.SetActive(false);
        layout0.SetActive(true);
    }

    public void ActivateLayout1()
    {
        layout0.SetActive(false);
        layout1.SetActive(true);
    }
}
