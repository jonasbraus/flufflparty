using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIItem : MonoBehaviour
{
    public Item item;

    public void BuyItem()
    {
        Player player = ItemShop.currentPlayer;
        
        if (player.coins - item.Cost >= 0)
        {
            player.AddCoins(-item.Cost);
            UIHandler.GetInstance().ButtonBack();
            player.AddItem(item);
        }
    }
}
