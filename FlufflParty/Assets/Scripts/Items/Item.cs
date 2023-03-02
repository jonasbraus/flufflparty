using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public Sprite sprite;
    public Type type;
    
    public int Cost
    {
        get
        {
            return cost;
        }
    }
    
    [SerializeField] private int cost;

    public enum Type
    {
        Mushroom = 0, DoubleDice = 1
    }
}
