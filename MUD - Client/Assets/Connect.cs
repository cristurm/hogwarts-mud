/* 
*  This file is part of the Unity networking tutorial by M2H (http://www.M2H.nl)
*  The original author of this code Mike Hergaarden, even though some small parts 
*  are copied from the Unity tutorials/manuals.
*  Feel free to use this code for your own projects, drop me a line if you made something exciting! 
*/
using UnityEngine;
using System.Collections;
using System;

public class Connect : MonoBehaviour
{
	public string connectToIP = "127.0.0.1";
	//public string connectToIP = "10.32.170.25";
	public int connectPort = 25001;
	public string playerName = null;
	private string myInfo = null;
	
	//Obviously the GUI is for both client&servers (mixed!)
	public void OnGUI()
	{
		if (Network.peerType == NetworkPeerType.Disconnected)
		{
			//We are currently disconnected: Not a client or host
			GUILayout.Label("Connection status: Disconnected");
			
			connectToIP = GUILayout.TextField(connectToIP, GUILayout.MinWidth(100));
			connectPort = int.Parse(GUILayout.TextField(connectPort.ToString()));
			playerName = GUILayout.TextField(playerName, GUILayout.MinWidth(100));
			
			GUILayout.BeginVertical();
			if (GUILayout.Button ("Connect as client"))
			{
				//Connect to the "connectToIP" and "connectPort" as entered via the GUI
				//Ignore the NAT for now
				Network.useNat = false;
				Network.Connect(connectToIP, connectPort);
			}
			
			GUILayout.EndVertical();
		}
		else
		{
			//We've got a connection(s)!
			if (Network.peerType == NetworkPeerType.Connecting)
			{
				GUILayout.Label("Connection status: Connecting");
			}
			else if (Network.peerType == NetworkPeerType.Client)
			{
				//player = transform.GetComponent<Chat>().GetPlayer(Network.player);
				GUILayout.Label("Connection status: Client!");
				GUILayout.Label("Ping to server: " + Network.GetAveragePing(Network.connections[0]));
				
				//this is our player info being shown at the top-right
				networkView.RPC("GetMyInfo", RPCMode.Server, Network.player);	
				GUILayout.BeginArea(new Rect(0, 90, 500, 300));
					GUILayout.BeginVertical();
						GUILayout.Label(myInfo);
					GUILayout.EndVertical();
				GUILayout.EndArea();
			}
	
			if (GUILayout.Button ("Disconnect"))
			{
				Network.Disconnect(200);
			}
		}
	}
	
	// NONE of the functions below is of any use in this demo, the code below is only used for demonstration.
	// First ensure you understand the code in the OnGUI() function above.
	
	//Client functions called by Unity
	public void OnConnectedToServer()
	{
		Debug.Log("This CLIENT has connected to a server");	
	}
	
	public void OnDisconnectedFromServer(NetworkDisconnection info)
	{
		Debug.Log("This SERVER OR CLIENT has disconnected from a server");
	}
	
	public void OnFailedToConnect(NetworkConnectionError error)
	{
		Debug.Log("Could not connect to server: " + error);
	}
	
	[RPC]
	public void RetrieveMyInfo(string playerInfo) {
		myInfo = playerInfo;
	}
	
	[RPC]
	public void GetMyInfo(NetworkPlayer netPlayer) { }
}
