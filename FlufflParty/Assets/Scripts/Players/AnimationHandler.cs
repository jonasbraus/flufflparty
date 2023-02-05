using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AnimationHandler : MonoBehaviour
{
    [SerializeField] private Animator anim;
    
    public void StartAnimation(Player.AnimationType type)
    {
        switch (type)
        {
            case Player.AnimationType.Coin:
                anim.SetInteger("value", 1);
                break;
        }
        Invoke("Stop", 0.6f);
    }

    private void Stop()
    {
        anim.SetInteger("value", 0);
    }
}
