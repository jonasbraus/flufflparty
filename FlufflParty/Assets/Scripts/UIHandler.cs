using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHandler : MonoBehaviour
{
    [SerializeField] private GameObject layout0;

    private void Start()
    {
        layout0.SetActive(true);
    }

    public void DeactivateLayout0()
    {
        layout0.SetActive(false);
    }
}
