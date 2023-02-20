using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class Player : MonoBehaviour
{
    public GameObject Layout2; 
    
    public string name = "";
    public bool activated = false;
    protected bool wurfelt = false;
    
    public GameObject camera;
    protected Vector3 velocity = Vector3.zero;

    protected Vector3 cameraOffset = new Vector3(-2.81f, 8.13f, 5.6f);
    public int index;
    public int coins = 0;
    public TMP_Text textCoinsInfo;
    protected bool eventstop = false;
    
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

    protected void AddCoins(int amount)
    {
        coins += amount;
        if (coins < 0)
        {
            coins = 0;
        }
        textCoinsInfo.text = coins + "";
        PlayAnimation(AnimationType.Coin);
    }
    
    private void OnTriggerEnter(Collider c)
    {
        eventstop = true;
        Layout2.SetActive(true);
    }
    
    public void EventStopFinished()
    {
        eventstop = false;
    }
}
