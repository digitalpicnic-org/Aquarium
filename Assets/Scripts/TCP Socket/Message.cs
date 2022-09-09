

using System;
using UnityEngine;

[Serializable]
public class Message {

    public string message;
    public int id;
    public string filepath;

    public Message(string message)
    {
        this.message = message;
    }

    public Message(string message, int id, string filepath)
    {
        this.message = message;
        this.id = id;
        this.filepath = filepath;
    }

    public string ConvertToJSON()
    {
        return JsonUtility.ToJson(this);
    }

}
