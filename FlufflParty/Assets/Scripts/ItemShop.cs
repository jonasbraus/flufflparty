using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemShop : MonoBehaviour
{
    [SerializeField] private UIItem[] items;

    public static Player currentPlayer = null;

    public void Init(Player _currentPlayer)
    {
        currentPlayer = _currentPlayer;
    }

    private void Start()
    {
        foreach(UIItem ui in items)
        {
            TMP_Text text = ui.GetComponentInChildren<TMP_Text>();
            text.text = ui.item.Cost + "";
        }
    }
}
