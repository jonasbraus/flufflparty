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
    public UIHandler uiHandler;
    private bool ended = false;

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

    private float slowDown = 0.5f;
    private void Update()
    {
        if (started)
        {
            if (Time.time - lastTimeSwitched > 0.2f)
            {
                lastTimeSwitched = Time.time;
                ColorOption(currentIndex);
                currentIndex++;
                if (currentIndex >= options.Length)
                {
                    currentIndex = 0;
                }
            }
        }
        else if(!ended)
        {
            if (currentIndex != stopIndex)
            {
                if (Time.time - lastTimeSwitched > slowDown)
                {
                    slowDown += 0.05f;
                    lastTimeSwitched = Time.time;
                    ColorOption(currentIndex);
                    currentIndex++;
                    if (currentIndex >= options.Length)
                    {
                        currentIndex = 0;
                    }
                }
            }
            if(currentIndex == stopIndex)
            {
                if (Time.time - lastTimeSwitched > slowDown)
                {
                    ColorOption(stopIndex);
                    ended = true;
                }
            }
        }
    }

    public void StartRandom()
    {
        lastTimeSwitched = Time.time;
        started = true;
        ended = false;
    }

    public void Stop(int index)
    {
        stopIndex = index;
        started = false;
    }
}