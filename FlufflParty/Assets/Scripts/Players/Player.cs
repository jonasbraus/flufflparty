using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class Player : MonoBehaviour
{
    public string name = "";
    public bool activated = false;
    protected bool wurfelt = false;
    
    public GameObject camera;
    protected Vector3 velocity = Vector3.zero;

    protected Vector3 cameraOffset = new Vector3(-2.81f, 8.13f, 5.6f);
    public int index;
    public int coins = 0;
    public int stars = 0;
    public TMP_Text textCoinsInfo, textStarsInfo, textLeftMoves;
    protected bool eventstop = false;
    public UIHandler uiHandler;
    public Item[] items = new Item[3];
    public Image[] uiItems;
    
    //for coin animation
    private AnimationHandler animationHandler;

    private void Start()
    {
        animationHandler = GetComponentInChildren<AnimationHandler>();
    }

    //Aktiviert den Player
    public void Activate()
    {
        activated = true;
        wurfelt = false;
    }

    //Deaktiviert den Player
    public void DeActivate()
    {
        activated = false;
    }

    public abstract void Init();

    public void PlayAnimation(AnimationType animationType)
    {
        animationHandler.StartAnimation(animationType);
    }

    public enum AnimationType
    {
        Coin
    }

    public void AddStars(int amount)
    {
        stars += amount;
        textStarsInfo.text = stars + "";
        Client.GetCurrentInstance().CalculatePlacement();
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        if (coins < 0)
        {
            coins = 0;
        }
        textCoinsInfo.text = coins + "";
        PlayAnimation(AnimationType.Coin);
        Client.GetCurrentInstance().CalculatePlacement();
    }

    public void AddItem(Item item)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                items[i] = item;
                uiItems[i].sprite = item.sprite;
                break;
            }
        }
    }
    
    public void OnTriggerEnter(Collider c)
    {
        eventstop = true;
    }
    
    public void EventStopFinished()
    {
        eventstop = false;
    }
}
