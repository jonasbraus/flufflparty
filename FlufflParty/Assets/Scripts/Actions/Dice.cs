using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class Dice : MonoBehaviour
{
    [SerializeField] private TMP_Text[] numbers;
    [SerializeField] private Rigidbody rb;
    private bool started;
    

    private void Start()
    {
        rb.useGravity = false;
        gameObject.SetActive(false);
    }
    
    public void StartRandom()
    {
        if (!started)
        {
            rb.useGravity = false;
            gameObject.SetActive(true);
            started = true;
            InvokeRepeating("RandomNumbers", 0f, 0.1f);
        }
    }

    public void StopRandom(int dec)
    {
        started = false;
        CancelInvoke("RandomNumbers");
        
        rb.useGravity = true;
        rb.angularVelocity = new Vector3(Random.Range(5, 10), Random.Range(5, 10), Random.Range(5, 10));

        foreach (TMP_Text t in numbers)
        {
            t.text = dec.ToString();
        }
    }

    private void RandomNumbers()
    {
        int random = Random.Range(1, 7);

        rb.angularVelocity = new Vector3(Random.Range(5, 10), Random.Range(5, 10), Random.Range(5, 10));
        foreach (TMP_Text t in numbers)
        {
            t.text = random.ToString();
        }
    }
}