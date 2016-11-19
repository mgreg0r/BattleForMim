using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using UnityEngine.SceneManagement;
using System.Threading;
using UnityEngine.UI;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour {
    private bool inQueue = false;
    public static Deck playerDeck;
    public GameObject enterQueueButton;
    public GameObject exitButton;
    public GameObject deckPanel;
    public GameObject collectionPanel;
    public GameObject queueInfoPanel;
    public Text queueInfoText;
    public static MainMenu menu;

    public static void gameFound()
    {
        menu.inQueue = false;
    }

    public void OnApplicationQuit()
    {
        leaveQueue();
    }

	void Start () {
        menu = this;
	}
	
	void Update () {
        lock (GameEngine.dcFlagLock)
        {
            if (GameEngine.dcFlag)
            {
                leaveQueue();
                queueInfoText.text = "Server has closed connection.";
                GameEngine.dcFlag = false;
            }
        }
	}

    public void enterQueueClick()
    {
        if (!inQueue)
        {
            inQueue = true;
            enterQueueButton.SetActive(false);
            exitButton.SetActive(false);
            deckPanel.SetActive(false);
            collectionPanel.SetActive(false);
            queueInfoPanel.SetActive(true);
            enterQueue();
        }       
    }

    public void exitClick()
    {
        Application.Quit();
    }

    private void enterQueue()
    {
        try
        {
            GameEngine.client = new TcpClient(NetworkEngine.HOSTNAME, NetworkEngine.PORT);
            StreamReader reader = new StreamReader(GameEngine.client.GetStream());
            StreamWriter writer = new StreamWriter(GameEngine.client.GetStream());
            String message = String.Empty;

            if (!deckBuilderManager.encodeDeck())
            {
                //todo:inform;
                return;
            }

            writer.WriteLine(PlayerPrefs.GetString("deck"));
            writer.Flush();

            NetworkEngine.writer = writer;

            GameEngine.listener = new Thread(NetworkEngine.listenForMessages);
            GameEngine.listener.Start(GameEngine.client);
            queueInfoText.text = "Searching for opponent...";
        }
        catch(Exception e)
        {
            queueInfoText.text = "Could not connect to the server";
            leaveQueue();
        }

    }

    public void cancelClick()
    {
        enterQueueButton.SetActive(true);
        exitButton.SetActive(true);
        deckPanel.SetActive(true);
        collectionPanel.SetActive(true);
        queueInfoPanel.SetActive(false);
        if (inQueue)
        {
            inQueue = false;           
            leaveQueue();
        }
    }

    private void leaveQueue()
    {
        
        if (GameEngine.listener != null && GameEngine.listener.IsAlive)
        {
            GameEngine.listener.Abort();
        }
        lock(GameEngine.clientLock)
        {
            if (GameEngine.client != null)
            {
                GameEngine.client.Close();
            }
        }
        NetworkEngine.connectionActive = false;
        inQueue = false;
    }
}
