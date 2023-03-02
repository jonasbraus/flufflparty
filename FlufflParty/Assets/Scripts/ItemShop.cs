using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemShop : MonoBehaviour
{
    [SerializeField] private UIItem[] items;

    public static Player currentPlayer = null;

    public void Init(Player _currentPlayer)
    {
        currentPlayer = _currentPlayer;
    }
}
