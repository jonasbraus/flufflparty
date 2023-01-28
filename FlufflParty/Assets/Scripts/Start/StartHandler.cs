using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartHandler : MonoBehaviour
{
    private TcpClient client;
    [SerializeField] private GameObject layout1;
    [SerializeField] private GameObject layout2;
    [SerializeField] private TMP_Text roomCodeEnter;
    [SerializeField] private GameObject buttonJoin;

    private string roomcode;

    private void Start()
    {
        client = new TcpClient("185.245.96.48", 8051);
        
        layout1.SetActive(true);
        layout2.SetActive(false);
        
        buttonJoin.SetActive(false);
    }

    public void ButtonNewRoom()
    {
        Stream stream = client.GetStream();
        byte[] readMessage = new byte[10];
        
        //Send message new room
        stream.Write(new byte[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0});
        
        layout1.SetActive(false);
        layout2.SetActive(true);
        
        //wait for room code
        int i = stream.Read(readMessage, 0, 10);
        
        Debug.Log(i);

        string code = Encoding.ASCII.GetString(readMessage);
        
        PlayerPrefs.SetString("roomcode", code);
        roomcode = code;
        PlayerPrefs.Save();
        
        layout2.GetComponentInChildren<TMP_Text>().text = "Your room code is: \n" + code;
    }

    public void ButtonJoin()
    {
        string code = roomCodeEnter.text;
        roomcode = code;
        PlayerPrefs.SetString("roomcode", code);
        PlayerPrefs.Save();
        
        SceneManager.LoadScene(1, LoadSceneMode.Single);
        client.Close();
    }

    public void ButtonJoin2()
    {
        PlayerPrefs.SetString("roomcode", roomcode);
        PlayerPrefs.Save();
        
        SceneManager.LoadScene(1, LoadSceneMode.Single);
        client.Close();
    }

    public void ButtonBack()
    {
        layout1.SetActive(true);
        layout2.SetActive(false);
    }

    public void ButtonCopy()
    {
        GUIUtility.systemCopyBuffer = PlayerPrefs.GetString("roomcode");
    }

    public void OnTextHasChanged(string s)
    {
        if (s.Length >= 10)
        {
            buttonJoin.SetActive(true);
        }
        else
        {
            buttonJoin.SetActive(false);
        }
    }
}
