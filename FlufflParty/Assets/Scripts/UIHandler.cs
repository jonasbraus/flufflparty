using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHandler : MonoBehaviour
{
    [SerializeField] private GameObject layout0;
    [SerializeField] private GameObject layout1;
    [SerializeField] private GameObject layoutMiniMap;
    [SerializeField] private GameObject layoutBuyStar;

    private Player currentPlayer = null;

    private void Start()
    {
        layout1.SetActive(false);
        layout0.SetActive(true);
        layoutMiniMap.SetActive(false);
        layoutBuyStar.SetActive(false);
    }

    public void ActivateLayout1()
    {
        layout0.SetActive(false);
        layout1.SetActive(true);
        layoutMiniMap.SetActive(false);
        layoutBuyStar.SetActive(false);
    }

    public void ActivateLayoutMiniMap()
    {
        layout0.SetActive(false);
        layout1.SetActive(false);
        layoutMiniMap.SetActive(true);
        layoutBuyStar.SetActive(false);
    }

    public void ActivateLayoutBuyStar(Player player)
    {
        currentPlayer = player;
        layout0.SetActive(false);
        layout1.SetActive(false);
        layoutMiniMap.SetActive(false);
        layoutBuyStar.SetActive(true);
    }
}
