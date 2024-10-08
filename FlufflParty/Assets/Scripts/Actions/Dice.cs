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
    //0 = default, 1 = double dice, 2 = red
    [SerializeField] private Material[] materials;
    private Renderer renderer;

    public void SetMaterial(int mat)
    {
        renderer.material = materials[mat];
    }
    
    private void Start()
    {
        rb.useGravity = false;
        gameObject.SetActive(false);
        renderer = GetComponent<Renderer>();
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

        rb.angularVelocity = new Vector3(0, 0, 0);
        transform.rotation = Quaternion.Euler(0, 0, 0);

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