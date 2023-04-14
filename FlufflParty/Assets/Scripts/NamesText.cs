using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NamesText : MonoBehaviour
{
    public float orgY;
    private GameObject camera;

    private void Start()
    {
        camera = GameCamera.gObject;
    }
    
    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Euler(0, orgY, 0);
    }
}
