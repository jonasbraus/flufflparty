using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemShop : MonoBehaviour
{
    [SerializeField] private UIItem[] items;

    public static Player currentPlayer = null;
    [SerializeField] private TMP_Text textPlayerCoins;

    public void Init(Player _currentPlayer)
    {
        currentPlayer = _currentPlayer;
        textPlayerCoins.text = currentPlayer.coins + "";
    }

    private void Start()
    {
        foreach(UIItem ui in items)
        {
            TMP_Text text = ui.GetComponentsInChildren<TMP_Text>()[0];
            text.text = ui.item.Cost + "";
        }
    }
}
