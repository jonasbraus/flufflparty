using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class MinigamesClient : MonoBehaviour
{
    private TcpClient client;
    private Stream stream;

    private Queue<Job> jobs = new Queue<Job>();
    
    private void Start()
    {
        client = new TcpClient("185.245.96.48", 8051);
        stream = client.GetStream();
        
        stream.Write(new byte[]{5, (byte)Client.playerID, 0, 0, 0, 0, 0, 0, 0, 0});

        byte[] originalRoomCode = Encoding.ASCII.GetBytes(PlayerPrefs.GetString("roomcode"));
        
        stream.Write(originalRoomCode);
        
        new Thread(Read).Start();
    }

    private void Update()
    {
        if (jobs.Count > 0)
        {
            Job job = jobs.Dequeue();

            switch (job.data[0])
            {
                //TEST
                case 1:
                    stream.Write(new byte[]{1, 0, 0, 0, 0, 0, 0, 0, 0, 0});
                    break;
            }
        }
    }

    private void Read()
    {
        while (true)
        {
            byte[] read = new byte[10];

            stream.Read(read, 0, 10);

            jobs.Enqueue(new Job(read));
        }
    }
}
