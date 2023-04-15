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
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Client : MonoBehaviour
{
    [SerializeField] private TMP_Text textLeftMoves;
    private TcpClient client;
    private Stream stream;
    [SerializeField] private GameObject[] playerPrefab;
    [SerializeField] private PlayerHeightInfo[] playerPrefabsHeightInfo;
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
    [SerializeField] public Item[] items;
    [SerializeField] private Button[] itemButtons;

    [SerializeField] private TMP_Text fpsText;

    private int playerAmount = 0;

    [SerializeField] private Image imageCurrentItem;
    [SerializeField] private Image[] playerItemInfoImages;

    private Dice diceScript;
    public static int playerID = -1;

    //Icon
    [SerializeField] private Image[] playerIconInfoImages;
    [SerializeField] private Sprite[] playerIconInfoImagesPrefabs;

    private static Client thisClient;

    //players in game text blub
    [SerializeField] private TMP_Text playersInGameText;

    //Scene System
    public static Scene sceneMap;

    //Minigames
    private string[] miniGamesMapping = new string[] { "TestGame" };
    private int currentMiniGameId = -1;

    public static Client GetCurrentInstance()
    {
        return thisClient;
    }

    public void LoadMinigame(int id)
    {
        currentMiniGameId = id;

        //Hide the current Scene (not the client!)
        GameObject[] objectsToUnload = sceneMap.GetRootGameObjects();
        foreach (GameObject g in objectsToUnload)
        {
            if (!g.name.Equals("EventSystem"))
            {
                g.SetActive(false);
            }
        }

        SceneManager.LoadScene(miniGamesMapping[id], LoadSceneMode.Additive);
    }

    public void UnloadMinigame()
    {
        GameObject[] objectsToUnload = sceneMap.GetRootGameObjects();
        foreach (GameObject g in objectsToUnload)
        {
            g.SetActive(true);
        }

        SceneManager.UnloadSceneAsync(miniGamesMapping[currentMiniGameId]);
    }

    private void Awake()
    {
        Time.fixedDeltaTime = 0.01f;
    }

    private void Start()
    {
        sceneMap = SceneManager.GetActiveScene();

        diceScript = dice.GetComponent<Dice>();

        Application.targetFrameRate = 60;

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
        int characterID = PlayerPrefs.GetInt("characterID");
        stream.Write(new byte[] { (byte)characterID, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 0, 10);

        for (int i = 0; i < playerInfo.Length; i++)
        {
            playerInfoElements[i] = playerInfo[i].GetComponent<PlayerInfoElements>();
        }

        new Thread(Read).Start();
    }

    /*
     *
     *
     *
     * Logic to load another Scene with Seperate Connection, while the current connection is holding
     */

    // private bool bub = false;
    // private void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.Space) && !bub)
    //     {
    //
    //         bub = true;
    //         GameObject[] gameObjectsToHide = sceneMap.GetRootGameObjects();
    //         foreach (GameObject g in gameObjectsToHide)
    //         {
    //             if (!g.name.Equals("EventSystem"))
    //             {
    //                 g.SetActive(true);
    //             }
    //         }
    //     
    //         SceneManager.LoadScene("Test", LoadSceneMode.Additive);
    //     }
    //
    //     else if (Input.GetKeyDown(KeyCode.Space) && bub)
    //     {
    //
    //         bub = false;
    //         GameObject[] gameObjectsToHide = sceneMap.GetRootGameObjects();
    //         foreach (GameObject g in gameObjectsToHide)
    //         {
    //             g.SetActive(true);
    //         }
    //
    //         SceneManager.UnloadSceneAsync("Test");
    //     }
    // }

    public void CalculatePlacement()
    {
        Player[] temp = new Player[playerAmount];
        bool[] samePlacement = new bool[playerAmount];

        for (int j = 0; j < temp.Length; j++)
        {
            temp[j] = players[j];
            samePlacement[j] = false;
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
                else if (temp[f].stars == temp[f + 1].stars && temp[f].coins == temp[f + 1].coins)
                {
                    samePlacement[f] = true;
                }
            }
        }

        int currentPlacement = 1;
        for (int j = 0; j < temp.Length; j++)
        {
            for (int i = 0; i < temp.Length; i++)
            {
                if (temp[j] == players[i])
                {
                    // playerInfoElements[i].textPlacement.text = j+1 + ".";
                    playerInfoElements[i].textPlacement.text = currentPlacement + "";

                    if (!samePlacement[j])
                    {
                        currentPlacement++;
                    }
                }
            }
        }
    }

    private void FixedUpdate()
    {
        fpsText.text = "" + Time.frameCount / Time.time + "";

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
                        GameObject player = Instantiate(playerPrefab[job.data[9]]);
                        playerIconInfoImages[job.data[5]].sprite = playerIconInfoImagesPrefabs[job.data[9]];

                        player.transform.position = new Vector3(job.data[2],
                            job.data[3] + playerPrefabsHeightInfo[job.data[9]].heightOffset, job.data[4]);

                        PlayablePlayer script = player.AddComponent<PlayablePlayer>();
                        players[job.data[5]] = script;
                        script.client = this;
                        script.nextField = nextField;
                        script.dice = dice;
                        script.camera = cam;
                        script.imageCurrentItem = imageCurrentItem;

                        //ITEM INFO IMAGES
                        Image[] temp = new Image[3];

                        int itemInfoImagesStartIndex = job.data[5] * 3;
                        int idx = 0;
                        for (int i = itemInfoImagesStartIndex; i < itemInfoImagesStartIndex + 3; i++)
                        {
                            temp[idx] = playerItemInfoImages[i];
                            temp[idx].color = new Color(0, 0, 0, 0);
                            idx++;
                        }

                        script.itemInfoImages = temp;
                        //END

                        itemButtons[job.data[5]].interactable = true;
                    }
                    //player is not playable
                    else
                    {
                        GameObject player = Instantiate(playerPrefab[job.data[9]]);
                        playerIconInfoImages[job.data[5]].sprite = playerIconInfoImagesPrefabs[job.data[9]];

                        player.transform.position = new Vector3(job.data[2],
                            job.data[3] + playerPrefabsHeightInfo[job.data[9]].heightOffset, job.data[4]);

                        NoPlayablePlayer script = player.AddComponent<NoPlayablePlayer>();
                        players[job.data[5]] = script;
                        script.client = this;
                        script.nextField = nextField;
                        script.dice = dice;
                        script.camera = cam;
                        script.imageCurrentItem = imageCurrentItem;

                        //ITEM INFO IMAGES
                        Image[] temp = new Image[3];

                        int itemInfoImagesStartIndex = job.data[5] * 3;
                        int idx = 0;
                        for (int i = itemInfoImagesStartIndex; i < itemInfoImagesStartIndex + 3; i++)
                        {
                            temp[idx] = playerItemInfoImages[i];
                            temp[idx].color = new Color(0, 0, 0, 0);
                            idx++;
                        }

                        script.itemInfoImages = temp;
                        //END

                        itemButtons[job.data[5]].interactable = false;
                    }

                    break;
                //---------------------------------------------------------
                //-----Activate a Player
                case 2:
                    players[job.data[1]].nameSign.SetActive(false);
                    diceScript.SetMaterial(0);
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
                    players[job.data[1]].textLeftMoves = textLeftMoves;
                    players[job.data[1]].AddCoins(3);


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
                case 9:
                    players[job.data[1]].AddCoins(-items[job.data[2]].Cost);
                    players[job.data[1]].AddItem(items[job.data[2]]);
                    break;
                case 10:
                    players[job.data[1]].ActivateItem(job.data[2]);
                    break;
                case 11:
                    playersInGameText.text = job.data[1] + "/4";
                    break;
                case 12:
                    ((NoPlayablePlayer)players[job.data[1]]).pinkFieldLostItem(job.data[2]);
                    break;
                case 13:
                    int amount = job.data[2];
                    if (job.data[1] == 0)
                    {
                        amount *= -1;
                    }

                    players[job.data[3]].AddCoins(amount);
                    break;
                case 14:
                    int type = job.data[9];
                    RandomRotator rotator = uiHandler.rotator.GetComponent<RandomRotator>();
                    switch (type)
                    {
                        case 0:

                            for (int i = 0; i < 4; i++)
                            {
                                int option = job.data[i + 1];
                                int value = job.data[i + 5];

                                switch (option)
                                {
                                    case 0:
                                        rotator.SetOptionsText(i, value + " Coins");
                                        break;
                                    case 1:
                                        rotator.SetOptionsText(i, "-" + value + " Coins");
                                        break;
                                    case 2:
                                        rotator.SetOptionsText(i, "Random Item");
                                        break;
                                }
                            }

                            break;
                    }

                    uiHandler.ActivateRotator();
                    rotator.StartRandom();
                    break;
                case 15:
                    RandomRotator rotator2 = uiHandler.rotator.GetComponent<RandomRotator>();
                    rotator2.Stop(job.data[1]);
                    Invoke("ActivateLayout1", 3.5f);
                    break;
                case 100:

                    playerID = job.data[1];
                    LoadMinigame(job.data[2]);

                    break;

                case 101:
                    Debug.Log("test");
                    UnloadMinigame();
                    break;
                case 126:
                    stream.Write(new byte[] { 126, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 0, 10);
                    break;
                case 127:
                    client.Close();
                    SceneManager.LoadScene(0, LoadSceneMode.Single);
                    break;
            }
        }
    }

    private void ActivateLayout1()
    {
        uiHandler.ActivateLayout1();
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
        stream.Write(new byte[] { 3, (byte)zahl, 0, 0, 0, 0, 0, 0, 0, 0 });
    }

    //0 4 arrow selected
    //1 x, x = arrow idx
    public void SendArrowSelected(int idx)
    {
        stream.Write(new byte[] { 4, (byte)idx, 0, 0, 0, 0, 0, 0, 0, 0 });
    }

    //0 5

    public void SendFinished()
    {
        stream.Write(new byte[] { 5, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
    }

    public void SendCoinFieldAction(int action)
    {
        stream.Write(new byte[] { 6, (byte)action, 0, 0, 0, 0, 0, 0, 0, 0 });
    }

    public void SendEventStopFinished()
    {
        stream.Write(new byte[] { 7, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
    }

    public void SendBuyStar()
    {
        stream.Write(new byte[] { 8, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
    }

    public void SendBuyItem(Item item)
    {
        stream.Write(new byte[] { 9, (byte)item.type, 0, 0, 0, 0, 0, 0, 0, 0 });
    }

    public void SendActiveItem(byte index)
    {
        stream.Write(new byte[] { 10, index, 0, 0, 0, 0, 0, 0, 0, 0 });
    }

    private void OnApplicationQuit()
    {
        stream.Write(new byte[] { 126, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        stream.Write(new byte[] { 126, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
    }

    public void SendLostItem(byte index)
    {
        stream.Write(new byte[] { 12, index, 0, 0, 0, 0, 0, 0, 0, 0 });
    }

    public void SendCoins(int amount)
    {
        byte isPositive = 1;
        if (amount < 0)
        {
            isPositive = 0;
            amount *= -1;
        }


        stream.Write(new byte[] { 13, isPositive, (byte)amount, 0, 0, 0, 0, 0, 0, 0 });
    }

    /*
     * OPT:
     * 0 = Get Coins
     * 1 = Loose Coins
     * 2 = Get Item (Random)
     * 3 = Loose Item (Random)
     *
     * VAL:
     * value of each option (not needed = 0)
     *
     * TYPE:
     * 0 = Pink Field
     */
    public void SendStartRotator(int opt1, int opt2, int opt3, int opt4, int val1, int val2, int val3, int val4,
        int type)
    {
        stream.Write(new byte[]
        {
            14, (byte)opt1, (byte)opt2, (byte)opt3, (byte)opt4, (byte)val1, (byte)val2, (byte)val3, (byte)val4,
            (byte)type
        });
    }

    public void SendStopRotator(int stopIndex)
    {
        stream.Write(new byte[] { 15, (byte)stopIndex, 0, 0, 0, 0, 0, 0, 0, 0 });
    }
}