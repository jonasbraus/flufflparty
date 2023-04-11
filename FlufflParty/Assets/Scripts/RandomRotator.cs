using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RandomRotator : MonoBehaviour
{
    [SerializeField] private Image[] options;
    [SerializeField] private TMP_Text[] textOptions;
    private bool started = false;
    private int currentIndex = 0;
    private float lastTimeSwitched = 0;
    private int stopIndex = 0;

    public void SetOptionsText(int index, string text)
    {
        textOptions[index].text = text;
    }

    private void ColorOption(int index)
    {
        for (int i = 0; i < options.Length; i++)
        {
            if (index == i)
            {
                options[i].color = new Color(254/255f, 190/255f, 140/255f, 255/255f);
            }
            else
            {
                options[i].color = new Color(0, 0, 0, 150/255f);
            }
        }
    }

    private void Update()
    {
        if (started)
        {
            if (Time.time - lastTimeSwitched > 0.1f)
            {
                lastTimeSwitched = Time.time;
                ColorOption(currentIndex);
                currentIndex++;
                if (currentIndex > options.Length - 1)
                {
                    currentIndex = 0;
                }
            }
        }
        else
        {
            ColorOption(stopIndex);
        }
    }

    private void Start()
    {
        SetOptionsText(0, "Bub");
        SetOptionsText(1, "Test");
        SetOptionsText(2, "Hulululu");
        SetOptionsText(3, "Testuuuu");
        StartRandom();
        
        Invoke("Temp", 5);
    }

    private void Temp()
    {
        Stop(2);
    }

    public void StartRandom()
    {
        lastTimeSwitched = Time.time;
        started = true;
    }

    public void Stop(int index)
    {
        stopIndex = index;
        started = false;
    }
}