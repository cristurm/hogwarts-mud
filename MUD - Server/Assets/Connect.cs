/* 
*  This file is part of the Unity networking tutorial by M2H (http://www.M2H.nl)
*  The original author of this code Mike Hergaarden, even though some small parts 
*  are copied from the Unity tutorials/manuals.
*  Feel free to use this code for your own projects, drop me a line if you made something exciting! 
*/
using MUD;
using UnityEngine;
using System.Collections;
using System;

public class Connect : MonoBehaviour
{
	public string connectToIP = "127.0.0.1";
	public int connectPort = 25001;
	public string playerName = "Server";
	private string myInfo = String.Empty;
	public Player server;
	
	public void Update() {
		if (server != null) {
			myInfo = server.playerInfo();
		}
	}
	
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
			
			if (GUILayout.Button ("Start Server"))
			{
				//Start a server for 32 clients using the "connectPort" given via the GUI
				//Ignore the nat for now	
				Network.useNat = false;
				Network.InitializeServer(32, connectPort);
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
			
			else if (Network.peerType == NetworkPeerType.Server)
			{
				GUILayout.Label("Connection status: Server!");
				GUILayout.Label("Connections: " + Network.connections.Length);
				if (Network.connections.Length >= 1)
				{
					GUILayout.Label("Ping to first player: " + Network.GetAveragePing(Network.connections[0]));
				}
				
				if (myInfo != string.Empty) {
					//this is our player info being shown at the top-right
					GUILayout.BeginArea(new Rect(0, 110, 500, 300));
						GUILayout.BeginVertical();
							GUILayout.Label(myInfo);
						GUILayout.EndVertical();
					GUILayout.EndArea();
				}
			}
	
			if (GUILayout.Button ("Disconnect"))
			{
				Network.Disconnect(200);
			}
		}
	}
	
	// NONE of the functions below is of any use in this demo, the code below is only used for demonstration.
	// First ensure you understand the code in the OnGUI() function above.

	//Server functions called by Unity
	public void OnPlayerConnected(NetworkPlayer player)
	{
		Debug.Log("Player connected from: " + player.ipAddress + ":" + player.port);
	}
	
	public void OnServerInitialized()
	{
		Debug.Log("Server initialized and ready");
	}
	
	public void OnPlayerDisconnected(NetworkPlayer player)
	{
		Debug.Log("Player disconnected from: " + player.ipAddress + ":" + player.port);
	}
	
	// OTHERS:
	// To have a full overview of all network functions called by unity
	// the next four have been added here too, but they can be ignored for now
	
	public void OnFailedToConnectToMasterServer(NetworkConnectionError info)
	{
		Debug.Log("Could not connect to master server: " + info);
	}
	
	public void OnNetworkInstantiate (NetworkMessageInfo info)
	{
		Debug.Log("New object instantiated by " + info.sender);
	}
	
	public void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		//Custom code here (your code!)
	}
	
	[RPC]
	public void RetrieveMyInfo(string playerInfo) { }
	
	[RPC]
	public void GetMyInfo(NetworkPlayer netPlayer) { 
		Player player = transform.GetComponent<Chat>().GetPlayer(netPlayer);
		string info = player.playerInfo();
		
		networkView.RPC("RetrieveMyInfo", netPlayer, info);
	}
}