using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ServerController : MonoBehaviour
{   
    private Message _message;
    private UnityEvent OnReceiveMessage;

    private void Awake() {
        // OnReceiveMessage += RecieveMessage;
    }

    public void Update()
    {
        //receive message
        _message = TCP_Server.instance.GetMessage();
        if(_message != null)
        {
            //do something here
            Debug.Log("RECEIVE: "+_message.message+", "+_message.id+", "+_message.filepath);
            RecieveMessage(_message);
        }


        //send message to client
        if (Input.GetKeyDown(KeyCode.S))
        {
            if (TCP_Server.instance != null)
            {
                TCP_Server.instance.SendBroadcastMessage(new Message("Hello all clients", 0, ""));
            }

        }
    }

    public void RecieveMessage(Message message)
    {
        //do something here
        List<Spawner> spawners = new List<Spawner>();
        spawners.AddRange(FindObjectsOfType<Spawner>());
        var spawner = spawners.Find(spawner => (int)spawner.spawnerType == message.id);
        spawner.filename = message.filepath;
        spawner.FetchNewUnit(message.filepath);
    }

   
}
