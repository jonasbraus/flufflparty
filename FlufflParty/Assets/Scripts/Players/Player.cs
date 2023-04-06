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
    public bool wurfelt = false;
    
    public GameObject camera;
    protected Vector3 velocity = Vector3.zero;
    protected Field currentField;

    protected Vector3 cameraOffset = new Vector3(-2.81f, 8.13f, 5.6f);
    public int index;
    public int coins = 0;
    public int stars = 0;
    public TMP_Text textCoinsInfo, textStarsInfo, textLeftMoves;
    protected bool eventstop = false;
    public UIHandler uiHandler;
    public Item[] items = new Item[3];
    public Item.Type activeItem = Item.Type.None;
    protected float rotationDamping = 10;
    public Image imageCurrentItem;
    public Image[] itemInfoImages;

    protected Animator animator;
    
    //for coin animation
    private AnimationHandler animationHandler;

    protected void Start()
    {
        animator = GetComponent<Animator>();
        animationHandler = GetComponentInChildren<AnimationHandler>();
    }

    //Aktiviert den Player
    public void Activate()
    {
        imageCurrentItem.color = new Color(0, 0, 0, 0);
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
                itemInfoImages[i].sprite = item.sprite;
                itemInfoImages[i].color = new Color(1, 1, 1, 1);
                break;
            }
        }
        
        Debug.Log(item.type);
    }

    public void ActivateItem(int index)
    {
        activeItem = items[index].type;
        imageCurrentItem.sprite = items[index].sprite;
        imageCurrentItem.color = new Color(1, 1, 1, 1);
        itemInfoImages[index].color = new Color(0, 0, 0, 0);
        if (items[index].type == Item.Type.Trap)
        {
            GameObject temp = Instantiate(items[index].gameObject);
            Trap t = temp.GetComponent<Trap>();
            currentField.placedItem = t.gameObject;
            t.target = gameObject.GetComponent<NoPlayablePlayer>();
            temp.transform.position = currentField.transform.position + (Vector3.up / 2);
            activeItem = Item.Type.None;
        }
        items[index] = null;
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
