using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIHandler : MonoBehaviour
{
    [SerializeField] private GameObject layout0;
    [SerializeField] private GameObject layout1;
    [SerializeField] private GameObject layout2;
    [SerializeField] private GameObject layoutMiniMap;
    [SerializeField] private GameObject layoutBuyStar;

    public bool MapOpen
    {
        get
        {
            return layoutMiniMap.activeSelf;
        }
    }

    private Player currentPlayer = null;

    private void Start()
    {
        layout1.SetActive(false);
        layout0.SetActive(true);
        layoutMiniMap.SetActive(false);
        layoutBuyStar.SetActive(false);
        layout2.SetActive(false);
        
    }

    public void ActivateLayout1()
    {
        layout0.SetActive(false);
        layout1.SetActive(true);
        layoutMiniMap.SetActive(false);
        layoutBuyStar.SetActive(false);
        layout2.SetActive(false);
    }

    public void ActivateLayoutMiniMap()
    {
        layout0.SetActive(false);
        layout1.SetActive(false);
        layoutMiniMap.SetActive(true);
        layoutBuyStar.SetActive(false);
        layout2.SetActive(false);
    }

    public void ActivateLayoutBuyStar(Player player)
    {
        currentPlayer = player;
        layout0.SetActive(false);
        layout1.SetActive(false);
        layoutMiniMap.SetActive(false);
        layoutBuyStar.SetActive(true);
        layout2.SetActive(false);

        LayoutBuyStarController controller = layoutBuyStar.GetComponent<LayoutBuyStarController>();

        if (player.coins < 20)
        {
            controller.infoText.text = "sorry... but you do not have enough coins";
            controller.buttonBuy.SetActive(false);
        }
        else
        {
            controller.infoText.text = "A Star for you!";
            controller.buttonBuy.SetActive(true);
        }
    }

    public void ActivateLayout2(Player player)
    {
        currentPlayer = player;
        
        layout0.SetActive(false);
        layout1.SetActive(false);
        layoutMiniMap.SetActive(false);
        layoutBuyStar.SetActive(false);
        layout2.SetActive(true);
    }

    public void ButtonBuyStar()
    {
        ((PlayablePlayer)currentPlayer).BuyStar(true);
        ActivateLayout1();
    }

    public void ButtonDontBuyStar()
    {
        ((PlayablePlayer)currentPlayer).BuyStar(false);
        ActivateLayout1();
    }
    
    public void ButtonMoveOn()
    {
        ActivateLayout1();
        ((PlayablePlayer)currentPlayer).SkipShop();
    }

    public void ButtonEnter()
    {
        
    }
}
