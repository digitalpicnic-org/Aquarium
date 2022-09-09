using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

using System.Threading;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

// TCP socket communication - server
// - Create thread to listen incoming clients
// - After connect with client, create thread to receive incoming message from each client

public class TCP_Server : MonoBehaviour
{
	public static TCP_Server instance;

	public int port = 7777;

	private Queue queue_receiveMessage;
	private bool IsConnected = false;
	private TcpListener tcpListener;
	private Thread tcpListenerThread;

	private List<ClientProperties> list_connectedClient;	//use to collect client attributes
	private float duration_ping = 5;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			Destroy(gameObject);
		}

		DontDestroyOnLoad(gameObject);
	}

	void Start()
	{
		list_connectedClient = new List<ClientProperties>();
		queue_receiveMessage = new Queue();

		StartServerThread();
		// used this method if you want to check connected status
		// InvokeRepeating("Ping", duration_ping, duration_ping);

	}

	// need to kill all threads when stop editor or program
	private void OnDestroy()
	{
		Debug.Log("kill thread when stop editor");

		ResetAllConnectionInServer();
	}

	// need to ping because unity cannot detect connection loss
	// so, error will occur when send a message that not receiever
	// but python can detect connection loss immediately (so, no need to ping to clients)
	void Ping()
	{
		if (IsConnected)
		{
			SendBroadcastMessage(new Message("ping from server..."));
			Debug.Log("ping");
		}
	}


	private void StartServerThread()
	{
		try
		{
			// Start TcpServer background thread 		
			tcpListenerThread = new Thread(new ThreadStart(ListenForIncommingRequests));
			tcpListenerThread.IsBackground = true;
			tcpListenerThread.Start();
		}
		catch (Exception e)
		{
			Debug.Log("On client connect exception " + e);
		}
	}


	// This thread used for listening incoming client, then create new thread for that client
	private void ListenForIncommingRequests()
	{
		Debug.Log("Server start listening incoming client");

		tcpListener = new TcpListener(IPAddress.Any, port);
		tcpListener.Start();

		while (true)
		{
            try
            {
				TcpClient connectedTcpClient = tcpListener.AcceptTcpClient();
				string ip_client = ((IPEndPoint)connectedTcpClient.Client.RemoteEndPoint).Address.ToString();

				// create new thread to receive message from the client
				Thread clientThread = new Thread(new ParameterizedThreadStart(ListenMessageInClient));
				list_connectedClient.Add(new ClientProperties(ip_client, connectedTcpClient, clientThread));

				IsConnected = true;
				Debug.Log("Client connected count: " + list_connectedClient.Count);

				clientThread.IsBackground = true;
				clientThread.Start(connectedTcpClient);
			}
			catch (Exception exception) 
			{
				Debug.Log("loss connection "+exception);
			}
		}
	}

	//Use to listen incoming message from a client
	private void ListenMessageInClient(System.Object obj)
	{
		TcpClient tcpClient = ((TcpClient)obj);
		string ip_client = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString();

		Byte[] data = new Byte[4096];

		try
		{
			while (true)
			{
				NetworkStream stream = tcpClient.GetStream();

				String responseData = String.Empty;
				// Read the first batch of the TcpServer response bytes.
				Int32 bytes = stream.Read(data, 0, data.Length); //(**This receives the data using the byte method**)
				
				responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes); // Convert byte array to string message. 
				// if receive null message, it means loss connection
				if(responseData == ""){
					throw new Exception("Loss connection");
				}

				//convert string message (json format) to object
				Message msg = JsonUtility.FromJson<Message>(responseData);

				if (msg != null)
				{
					queue_receiveMessage.Enqueue(msg);
					// Debug.Log(msg.message);
				}

				stream.Flush();

				// int length;
				// NetworkStream stream = tcpClient.GetStream();
				// //Read incomming stream into byte arrary. 						

				// while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
				// {
				// 	var incommingData = new byte[length];
				// 	Array.Copy(bytes, 0, incommingData, 0, length);
				// 	// Convert byte array to string message. 							
				// 	string clientMessage = Encoding.ASCII.GetString(incommingData);

				// 	Debug.Log("RECEIVE: " + clientMessage);

				// 	Message msg = JsonUtility.FromJson<Message>(clientMessage);
				// 	//Debug.Log(msg.message);

				// 	if (msg != null)
                //     {
				// 		queue_receiveMessage.Enqueue(msg);
				// 		//SendMessageToClient(tcpClient, msg); //test sending message back to client
				// 	}
						
				// 	stream.Flush();
				//}
			}

		}
		catch (Exception exception)
		{
			Debug.Log("Exception: " + exception);

			//KillClient(ip_client);
			KillNotActiveClient();
			Thread.CurrentThread.Abort();
		}
	}

	// kill client when it's not active
	// note - unity might not detect active status when it raise exception
	// 		so, to fix this problem, ping is applied to detect loss connection
	// 		loss connection will raised when cannot send a data 
	private void KillNotActiveClient()
	{
		Debug.Log("count:" + list_connectedClient.Count);

		// if it have only one connection left, then reset the server
		if (list_connectedClient.Count == 1)
		{
			Debug.Log("reconnect");
			ResetServer();
		}
		// try to kill non-active client
		else
		{
			Debug.Log("Kill Client...");

			Thread tmpThread = null;

			for (int i = 0; i < list_connectedClient.Count; i++)
			{
				Debug.Log(list_connectedClient[i].ip +" "+ list_connectedClient[i].tcpClient.Connected);
				if (!list_connectedClient[i].tcpClient.Connected)
				{
                    list_connectedClient[i].tcpClient.Close();
                    tmpThread = list_connectedClient[i].thread;
                    list_connectedClient.RemoveAt(i);
                    break;
                }
			}

			Debug.Log("Client remain: "+list_connectedClient.Count);

			// if no clients left, then change status and it will reconnect to server
			if (list_connectedClient.Count == 0)
				IsConnected = false;

			// kill the non-used thread 
			if(tmpThread != null)
            {
				Debug.Log("abort client thread");
				tmpThread.Abort();
			}
		}
	}

	// private void KillClient(string key_ipAddress)
	// {
	// 	Debug.Log("count:" + list_connectedClient.Count);

	// 	if (list_connectedClient.Count == 1)
	// 	{
	// 		Debug.Log("reconnect");
	// 		ReconnectServer();
	// 	}
	// 	else
	// 	{
	// 		Debug.Log("Abort Client " + key_ipAddress);

	// 		Thread tmpThread = null;

	// 		for (int i = 0; i < list_connectedClient.Count; i++)
	// 		{
	// 			Debug.Log(list_connectedClient[i].ip + " " + key_ipAddress+" "+ list_connectedClient[i].tcpClient.Connected);
	// 			if (list_connectedClient[i].ip == key_ipAddress && !list_connectedClient[i].tcpClient.Connected)
	// 			{
    //                 list_connectedClient[i].tcpClient.Close();
    //                 tmpThread = list_connectedClient[i].thread;
    //                 list_connectedClient.RemoveAt(i);
    //                 break;
    //             }
	// 		}

	// 		Debug.Log(list_connectedClient.Count);

	// 		if (list_connectedClient.Count == 0)
	// 			IsConnected = false;

	// 		if(tmpThread != null)
    //         {
	// 			Debug.Log("abort client thread");
	// 			tmpThread.Abort();
	// 		}
	// 	}
	// }

	// kill all threads
	private void ResetAllConnectionInServer()
	{
		IsConnected = false;

		//kill all clients
        for (int i = 0; i < list_connectedClient.Count; i++)
        {
			Debug.Log("closed client: "+i);
            list_connectedClient[i].tcpClient.Close();
        }

		list_connectedClient.Clear();

		//kill server thread
		tcpListener.Stop();
		tcpListenerThread.Abort();

	}

	private void ResetServer()
	{
		ResetAllConnectionInServer();
		StartServerThread();
	}

	// get message from receive message queue
	public Message GetMessage() {

		if(queue_receiveMessage.Count > 0)
			return (Message)queue_receiveMessage.Dequeue();

		return null;
	}

	//send message to all connected clients
	public void SendBroadcastMessage(Message messageObj)
	{

		if (list_connectedClient.Count == 0)
			return;

		for (int i = 0; i < list_connectedClient.Count; i++)
		{
			SendMessageToClient(list_connectedClient[i].tcpClient, messageObj);
		}

	}

	//send message to specific client
	public void SendMessageToClient(TcpClient client, Message messageObj)
	{

		if (client == null)
			return;

		try
		{
			// Get a stream object for writing. 			
			NetworkStream stream = client.GetStream();
			if (stream.CanWrite)
			{
				//IFormatter formatter = new BinaryFormatter();
				//// Write byte array to socketConnection stream.                 
				//formatter.Serialize(stream, messageObj);

				//serialize object to json format
				string jsonMessage = JsonUtility.ToJson(messageObj);
				byte[] serverMessageAsByteArray = Encoding.ASCII.GetBytes(jsonMessage);
				stream.Write(serverMessageAsByteArray, 0, serverMessageAsByteArray.Length);
				Debug.Log("SEND: " + jsonMessage);

				stream.Flush();
			}
		}
		catch (Exception exception)
		{
			Debug.Log("Send Exception: " + exception);
			KillNotActiveClient();
		}
	}

	private GUIStyle guiStyle = new GUIStyle(); //create a new variable

	//display text when no connection exist
	private void OnGUI()
    {
		guiStyle.fontSize = 20; //change the font size
		guiStyle.normal.textColor = Color.gray;
		if (!IsConnected)
        {
            GUI.Label(new Rect(10, 10, 100, 20), "No Connection", guiStyle);
		}
		
	}

}