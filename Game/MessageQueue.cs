using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MessageQueue {
    private LinkedList<string> messages = new LinkedList<string>();
    private Object msgLock = new Object();

    public void add(string message)
    {
        lock(msgLock)
        {
            messages.AddLast(message);
        }
    }

    public string get()
    {
        lock(msgLock)
        {
            if (messages.Count == 0)
                return "EMPTY";
            string res = messages.First.Value;
            messages.RemoveFirst();
            return res;
        }
    }
}
