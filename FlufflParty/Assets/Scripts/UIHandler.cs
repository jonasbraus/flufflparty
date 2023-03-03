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
    [SerializeField] private GameObject layoutItemShop;

    private static UIHandler currentInstance;

    public static UIHandler GetInstance()
    {
        return currentInstance;
    }
    
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
        layoutItemShop.SetActive(false);

        currentInstance = this;
    }

    public void ActivateLayout1()
    {
        layout0.SetActive(false);
        layout1.SetActive(true);
        layoutMiniMap.SetActive(false);
        layoutBuyStar.SetActive(false);
        layout2.SetActive(false);
        layoutItemShop.SetActive(false);
    }

    public void ActivateLayoutMiniMap()
    {
        layout0.SetActive(false);
        layout1.SetActive(false);
        layoutMiniMap.SetActive(true);
        layoutBuyStar.SetActive(false);
        layout2.SetActive(false);
        layoutItemShop.SetActive(false);
    }

    public void ActivateLayoutBuyStar(Player player)
    {
        currentPlayer = player;
        layout0.SetActive(false);
        layout1.SetActive(false);
        layoutMiniMap.SetActive(false);
        layoutBuyStar.SetActive(true);
        layout2.SetActive(false);
        layoutItemShop.SetActive(false);

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
        layoutItemShop.SetActive(false);
    }

    public void ActivateLayoutItemShop()
    {
        layout0.SetActive(false);
        layout1.SetActive(false);
        layoutMiniMap.SetActive(false);
        layoutBuyStar.SetActive(false);
        layout2.SetActive(false);
        layoutItemShop.SetActive(true);
        layoutItemShop.GetComponent<ItemShop>().Init(currentPlayer);
    }

    //TODO: Only temp, make additional function for Item selection
    public void ActivateLayoutItemSelect()
    {
        currentPlayer.ActivateItem(0, Item.Type.Mushroom);
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
        ActivateLayoutItemShop();
    }

    public void ButtonBack()
    {
        ActivateLayout1();
        ((PlayablePlayer)currentPlayer).SkipShop();
    }
}
