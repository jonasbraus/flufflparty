using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
    //TODO: Replace with Follow Function later (player camera should follow)
    [SerializeField] private GameObject followUp;
    private Vector3 offset;
    private Vector3 velocity = Vector3.zero;

    //TODO: Replace with Follow Function later (player camera should follow)
    private void Start()
    {
        offset = followUp.transform.position - transform.position;
    }

    private void FixedUpdate()
    {
        transform.position = Vector3.SmoothDamp(transform.position, followUp.transform.position - offset, ref velocity, 0.6f);
    }
}
