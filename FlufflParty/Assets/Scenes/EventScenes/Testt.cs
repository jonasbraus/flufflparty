using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;

public class Testt : MonoBehaviour
{
    private TcpClient client;

    private void Start()
    {
        client = new TcpClient("185.245.96.48", 8051);

        Stream stream = client.GetStream();
        
        stream.Write(new byte[]{5, 0, 0, 0, 0, 0, 0, 0, 0, 0});
    }
}
