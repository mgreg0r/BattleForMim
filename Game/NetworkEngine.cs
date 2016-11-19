using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.IO;

public class NetworkEngine : MonoBehaviour
{

    public static string HOSTNAME = "127.0.0.1";
    public static int PORT = 6789;
    public static MessageQueue messageQueue = new MessageQueue();
    public static StreamWriter writer;
    public static bool connectionActive = false;
    public static TcpClient client;

    public static void send(string msg)
    {
        writer.WriteLine(msg);
        writer.Flush();
    }

    public static void listenForMessages(System.Object argument)
    {
        StreamReader reader = null;
        lock(GameEngine.clientLock)
        {
            client = (TcpClient)argument;
            reader = new StreamReader(client.GetStream());
        }       
        string message = String.Empty;
        while(true)
        {
            try
            {
                message = reader.ReadLine();
            }
            catch(Exception)
            {
               lock(GameEngine.dcFlagLock)
                {
                    GameEngine.dcFlag = true;
                }
                return;
            }
            messageQueue.add(message);
        }
    }
    
    void Update()
    {
        string msg = messageQueue.get();
        if (msg.Equals("EMPTY"))
            return;
        string[] parts = msg.Split('#');
        string type = parts[0];
        List<int> args = new List<int>();
        if (!type.Equals("msg"))
        {
            for (int i = 1; i < parts.Length; i++)
            {
                args.Add(int.Parse(parts[i]));
            }
        }
        ParametersQueue param = new ParametersQueue(args);

        if(type.Equals("startGame"))
        {
            GameEngine.startGame(args);
        }
        else if(type.Equals("drawCard"))
        {
            GameEngine.drawCard(args);
        }
        else if(type.Equals("startTurn"))
        {
            GameEngine.startTurn();
        }
        else if(type.Equals("startOpponentTurn"))
        {
            GameEngine.startOpponentTurn();
        }
        else if(type.Equals("cast"))
        {
            GameEngine.opponentCast(param);
        }
        else if(type.Equals("lclick"))
        {
            GameEngine.opponentLClick(param);
        }
        else if(type.Equals("rclick"))
        {
            GameEngine.opponentRClick(param);
        }
        else if(type.Equals("msg"))
        {
            GameEngine.log("Opponent says: " +msg.Substring(4));
        }
        else if(type.Equals("opponentDrawCard"))
        {
            GameEngine.opponentDrawCard();
        }
        else if(type.Equals("opponentLeft"))
        {
            GameEngine.endGame("Your opponent has left. You win the game!");
        }
        else if(type.Equals("loseGame"))
        {
            GameEngine.endGame("You lost the game");
        }
        else if(type.Equals("winGame"))
        {
            GameEngine.endGame("You won the game!");
        }
    }

}

