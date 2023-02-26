using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarController : MonoBehaviour
{
    [SerializeField] private Star[] stars;

    private int current = 0;

    private void Start()
    {
        foreach(Star s in stars)
        {
            s.init(this);
        }
        
        Activate(current);
        current++;
    }

    private void Activate(int idx)
    {
        for (int i = 0; i < stars.Length; i++)
        {
            if (i == idx)
            {
                stars[i].IsActive = true;
            }
            else
            {
                stars[i].IsActive = false;
            }
        }
    }

    public void SwitchStar()
    {
        Activate(current);
        current++;
    }
}
