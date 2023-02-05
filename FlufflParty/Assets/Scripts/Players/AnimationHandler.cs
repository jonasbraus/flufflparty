using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AnimationHandler : MonoBehaviour
{
    [SerializeField] private Animator anim;
    
    public void StartAnimation()
    {
        anim.SetInteger("value", 1);
        Invoke("Stop", 0.6f);
    }

    private void Stop()
    {
        anim.SetInteger("value", 0);
    }
}
