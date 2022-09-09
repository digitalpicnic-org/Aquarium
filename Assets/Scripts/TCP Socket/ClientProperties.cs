
using System.Net.Sockets;
using System.Threading;
public class ClientProperties
{
	public string ip;
	public TcpClient tcpClient;
	public Thread thread;
	//public float receiveMessageTimer;

	public ClientProperties(string ip, TcpClient tcpClient, Thread thread)
	{
		this.ip = ip;
		this.tcpClient = tcpClient;
		this.thread = thread;
	}

}
