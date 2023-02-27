using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Star : MonoBehaviour
{
    private bool isActive;

    private StarController starController;
    private int cost = 20;

    [SerializeField] private GameObject starObject, starObject2;

    public int Cost
    {
        get
        {
            return cost;
        }
    }
    
    public bool IsActive
    {
        get
        {
            return isActive;
        }
        set
        {
            isActive = value;
            starObject.SetActive(value);
            starObject2.SetActive(value);
        }
    }

    public void init(StarController starController)
    {
        this.starController = starController;
    }

    public void Buy(Player player)
    {
        player.AddCoins(-cost);
        starController.SwitchStar();
    }
}
