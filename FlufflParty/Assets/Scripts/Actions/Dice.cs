using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class Dice : MonoBehaviour
{
    [SerializeField] private TMP_Text[] numbers;
    [SerializeField] private Rigidbody rb;
    private bool started;
    private float timeLastNumberChanged = 0;
    

    private void Start()
    {
        rb.useGravity = false;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if(started)
        {
            if (Time.time - timeLastNumberChanged >= .100f)
            {
                RandomNumbers();
            }
        }
    }
    
    public void StartRandom()
    {
        if (!started)
        {
            rb.useGravity = false;
            gameObject.SetActive(true);
            started = true;
        }
    }

    public void StopRandom(int dec)
    {
        started = false;

        rb.angularVelocity = new Vector3(5, 5, 5);
        rb.useGravity = true;

        foreach (TMP_Text t in numbers)
        {
            t.text = dec.ToString();
        }
    }

    private void RandomNumbers()
    {
        timeLastNumberChanged = Time.time;
        int random = Random.Range(1, 7);
        
        rb.angularVelocity = new Vector3(5, 5, 5);
        
        foreach (TMP_Text t in numbers)
        {
            t.text = random.ToString();
        }
    }
}