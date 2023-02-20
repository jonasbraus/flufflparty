using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemShop : MonoBehaviour
{
    [SerializeField] private GameObject Layout2;
    [SerializeField] private GameObject LayoutItemShop;
    [SerializeField] private Client client;
    public void buttonMoveOn()
    {
        Layout2.SetActive(false);
        client.EventStopFinished();
    }

    public void buttonEnter()
    {
        Layout2.SetActive(false);
        LayoutItemShop.SetActive(true);
    }
}
