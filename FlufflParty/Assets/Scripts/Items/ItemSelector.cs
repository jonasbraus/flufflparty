using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSelector : MonoBehaviour
{
    [SerializeField] private GameObject[] itemButtons;
    [SerializeField] private Image[] itemImages;
    
    
    public void Init(Item[] items)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null)
            {
                itemButtons[i].SetActive(true);
                itemImages[i].sprite = items[i].sprite;
            }
            else
            {
                itemButtons[i].SetActive(false);
            }
        }
    }
}
