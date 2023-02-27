using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Client : MonoBehaviour
{
    private TcpClient client;
    private Stream stream;
    [SerializeField] private GameObject playerPrefab;
    private Player[] players = new Player[4];
    private Queue<Job> jobs = new Queue<Job>();
    [SerializeField] private GameObject dice;
    [SerializeField] private Field nextField;
    [SerializeField] private GameObject cam;

    [SerializeField] private UIHandler uiHandler;

    [SerializeField] private TMP_Text[] playerNameTexts;
    [SerializeField] private GameObject[] playerInfo;
    private PlayerInfoElements[] playerInfoElements = new PlayerInfoElements[4];
    [SerializeField] private GameObject layout2;

    private int playerAmount = 0;

    private static Client thisClient;

    public static Client GetCurrentInstance()
    {
        return thisClient;
    }
    
    private void Start()
    {
        thisClient = this;
        
        for (int i = 0; i < 4; i++)
        {
            playerInfo[i].SetActive(false);
        }

        client = new TcpClient("185.245.96.48", 8051);
        stream = client.GetStream();

        stream.Write(new byte[] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 0, 10);


        string roomCode = PlayerPrefs.GetString("roomcode");
        stream.Write(Encoding.ASCII.GetBytes(roomCode), 0, 10);
        string name = PlayerPrefs.GetString("name");
        stream.Write(Encoding.ASCII.GetBytes(name), 0, 10);

        for (int i = 0; i < playerInfo.Length; i++)
        {
            playerInfoElements[i] = playerInfo[i].GetComponent<PlayerInfoElements>();
        }

        new Thread(Read).Start();
    }

    public void CalculatePlacement()
    {
        Player[] temp = new Player[playerAmount];

        for (int j = 0; j < temp.Length; j++)
        {
            temp[j] = players[j];
        }
        
        for (int i = 0; i < temp.Length - 1; i++)
        {
            for (int f = 0; f < temp.Length - 1; f++)
            {
                if (temp[f].stars < temp[f + 1].stars)
                {
                    (temp[f], temp[f + 1]) = (temp[f + 1], temp[f]);
                }
                else if (temp[f].stars == temp[f + 1].stars && temp[f].coins < temp[f + 1].coins)
                {
                    (temp[f], temp[f + 1]) = (temp[f + 1], temp[f]);              
                }
            }
        }

        for (int j = 0; j < temp.Length; j++)
        {
            for (int i = 0; i < temp.Length; i++)
            {
                if (temp[j] == players[i])
                {
                    playerInfoElements[i].textPlacement.text = j+1 + ".";
                }
            }
        }
    }

    private void Update()
    {
        if (jobs.Count > 0)
        {
            Job job = jobs.Dequeue();

            switch (job.data[0])
            {
                //-----Create A new player-----
                case 1:
                    playerAmount++;
                    
                    //player is playable
                    if (job.data[1] == 1)
                    {
                        GameObject player = Instantiate(playerPrefab);

                        player.transform.position = new Vector3(job.data[2], job.data[3] + 0.5f, job.data[4]);

                        PlayablePlayer script = player.AddComponent<PlayablePlayer>();
                        players[job.data[5]] = script;
                        script.client = this;
                        script.nextField = nextField;
                        script.dice = dice;
                        script.camera = cam;
                    }
                    //player is not playable
                    else
                    {
                        GameObject player = Instantiate(playerPrefab);

                        player.transform.position = new Vector3(job.data[2], job.data[3] + 0.5f, job.data[4]);

                        NoPlayablePlayer script = player.AddComponent<NoPlayablePlayer>();
                        players[job.data[5]] = script;
                        script.nextField = nextField;
                        script.dice = dice;
                        script.camera = cam;
                    }

                    break;
                //---------------------------------------------------------
                //-----Activate a Player
                case 2:
                    Activate(job.data[1]);
                    break;
                //Jemand hat gewürfelt
                case 3:
                    ((NoPlayablePlayer)players[job.data[1]]).Wurfeln(job.data[2]);
                    break;
                //Jemand hat den Pfeil geklickt
                case 4:
                    ((NoPlayablePlayer)players[job.data[1]]).ArrowSelect(job.data[2]);
                    break;
                case 5:
                    byte[] name = new byte[8];
                    for (int i = 0; i < 8; i++)
                    {
                        name[i] = job.data[i + 2];
                    }

                    players[job.data[1]].name = Encoding.ASCII.GetString(name).Replace(" ", "");
                    players[job.data[1]].index = job.data[1];
                    players[job.data[1]].textCoinsInfo = playerInfoElements[job.data[1]].textCoinInfo;
                    players[job.data[1]].textStarsInfo = playerInfoElements[job.data[1]].textStarsInfo;
                    players[job.data[1]].uiHandler = uiHandler;
                    players[job.data[1]].Init();

                    playerNameTexts[job.data[1]].text = Encoding.ASCII.GetString(name).Replace(" ", "");

                    uiHandler.ActivateLayout1();

                    playerInfo[job.data[1]].SetActive(true);

                    break;

                case 6:
                    ((NoPlayablePlayer)players[job.data[1]]).CoinFieldAction(job.data[2]);
                    break;
                case 7:
                    (players[job.data[1]]).EventStopFinished();
                    break;
                case 8:
                    ((NoPlayablePlayer)players[job.data[1]]).BuyStar();
                    break;
                case 126:
                    stream.Write(new byte[]{126, 0, 0, 0, 0, 0, 0, 0, 0, 0}, 0, 10);
                    break;
                case 127:
                    client.Close();
                    SceneManager.LoadScene(0, LoadSceneMode.Single);
                    break;
            }
        }
    }

    private void Activate(int player)
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (!(players[i] is null))
            {
                if (i == player)
                {
                    players[i].Activate();
                }
                else
                {
                    players[i].DeActivate();
                }
            }
        }

        // cam.transform.parent = players[player].transform;
        // cam.transform.localPosition = new Vector3(-3, 8, 7);
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

    //0 3 wurfeln
    //1 x, x = zahl
    public void SendWurfeln(int zahl)
    {

        stream.Write(new byte[]{3, (byte)zahl, 0, 0, 0, 0, 0, 0, 0, 0});
    }

    //0 4 arrow selected
    //1 x, x = arrow idx
    public void SendArrowSelected(int idx)
    {
        stream.Write(new byte[]{4, (byte)idx, 0, 0, 0, 0, 0, 0, 0, 0});
    }

    //0 5
    public void SendFinished()
    {
        stream.Write(new byte[]{5, 0, 0, 0, 0, 0, 0, 0, 0, 0});
    }

    public void SendCoinFieldAction(int action)
    {
        stream.Write(new byte[]{6, (byte)action, 0, 0, 0, 0, 0, 0, 0, 0});
    }

    public void SendEventStopFinished()
    {
        stream.Write(new byte[]{7, 0, 0, 0, 0, 0, 0, 0, 0, 0});
    }

    public void SendBuyStar()
    {
        stream.Write(new byte[]{8, 0, 0, 0, 0, 0, 0, 0, 0, 0});
    }
}