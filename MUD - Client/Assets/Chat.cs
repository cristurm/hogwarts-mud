/* 
*  This file is part of the Unity networking tutorial by M2H (http://www.M2H.nl)
*  The original author of this code is Mike Hergaarden, even though some small parts 
*  are copied from the Unity tutorials/manuals.
*  Feel free to use this code for your own projects, drop us a line if you made something exciting! 
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Chat : MonoBehaviour {
	public bool usingChat = false;		//Can be used to determine if we need to stop player movement since we're chatting
	public GUISkin skin;				//Skin
	public bool showChat = false;		//Show/Hide the chat
	
	/*maps
	public Map salaComunal, escadarias, calaboucos, quartoGriff, refeitorio, estufa;
	
	//items
	public Stuff chave, ervaMed, ervaVen, mandragora;
	
	//player and chat lists
	private List<Player> playerList = new List<Player>();*/
	private ArrayList chatEntries;
	public class ChatEntry {
		public string fromName = "";
		public string toName = "";
		public string text = "";
	}
	
	//private vars used by the script
	private string inputField = "";
	
	private Vector2 scrollPosition;
	private int height = Screen.height;
	private int width = Screen.width/2;
	private string playerName;
	private float lastUnfocusTime = 0;
	private Rect window;
	
	public void Awake()
	{
		window = new Rect(Screen.width-width, Screen.height-height+5, width, height);
	}
	
	//Client function
	public void OnConnectedToServer() {
		playerName = transform.GetComponent<Connect>().playerName;
		ShowChatWindow();
		Debug.Log ("zaza");
		networkView.RPC ("TellServerOurName", RPCMode.Server, playerName);
	}
	
	public void OnDisconnectedFromServer() { CloseChatWindow(); }
	
	public void CloseChatWindow() {
		showChat = false;
		inputField = "";
		chatEntries = new ArrayList();
	}
	
	public void ShowChatWindow() {
		showChat = true;
		inputField = "";
		chatEntries = new ArrayList();
	}
	
	public void OnGUI() {
		if(!showChat)
		{
			return;
		}
		
		GUI.skin = skin;		
				
		if (Event.current.type == EventType.keyDown && Event.current.character == '\n' && inputField.Length <= 0)
		{
			if(lastUnfocusTime + 0.25 < Time.time)
			{
				usingChat = true;
				GUI.FocusWindow(5);
				GUI.FocusControl("Chat input field");
			}
		}
		
		window = GUI.Window(5, window, GlobalChatWindow, "");
	}
	
	public void GlobalChatWindow (int id) {
		GUILayout.BeginVertical();
		GUILayout.Space(10);
		GUILayout.EndVertical();
		
		// Begin a scroll view. All rects are calculated automatically - 
	    // it will use up any available screen space and make sure contents flow correctly.
	    // This is kept small with the last two parameters to force scrollbars to appear.
		scrollPosition = GUILayout.BeginScrollView (scrollPosition);
	
		foreach (ChatEntry entry in chatEntries) {
			GUILayout.BeginHorizontal();
			if(entry.fromName == "") //Game message
			{
				GUILayout.Label(entry.text);
			}
			else
			{
				GUILayout.Label(entry.fromName + ": " + entry.text);
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(3);
			
		}
		// End the scrollview we began above.
	    GUILayout.EndScrollView ();
		
		if (Event.current.type == EventType.keyDown && Event.current.character == '\n' && inputField.Length > 0)
		{
			HitEnter(inputField);
		}
		
		GUI.SetNextControlName("Chat input field");
		inputField = GUILayout.TextField(inputField);
		
		if(Input.GetKeyDown("mouse 0"))
		{
			if(usingChat)
			{
				usingChat = false;
				GUI.UnfocusWindow();//Deselect chat
				lastUnfocusTime = Time.time;
			}
		}
	}
	
	public void HitEnter(string msg)
	{
		msg = msg.Replace("\n", "");
		networkView.RPC("MessageTreatement", RPCMode.Server, Network.player, msg);
		inputField = ""; //Clear line
		//GUI.UnfocusWindow();//Deselect chat
		//lastUnfocusTime = Time.time;
		usingChat = true;
	}
	
	public void RemoveOldEntries() {
		if (chatEntries.Count > 30) { chatEntries.RemoveAt(0); }
		scrollPosition.y = 1000000;	
	}
	
	//Add game messages etc
	public void addGameChatMessage(string str)
	{
		ApplyGlobalChatText("", str);
		if(Network.connections.Length > 0)
		{
			networkView.RPC("ApplyGlobalChatText", RPCMode.Others, "", str);	
		}	
	}
	
	[RPC]
	public void TellServerOurName(string name, NetworkMessageInfo info) {}
	
	[RPC]
	public void MessageTreatement(NetworkPlayer netPlayer, string msg) {}
	
	[RPC]
	public void ApplyGlobalChatText (string name, string msg) {
		ChatEntry entry = new ChatEntry();
		entry.fromName = name;
		entry.text = msg;
		chatEntries.Add(entry);
		RemoveOldEntries();
	}
	
	[RPC]
	public void SendFeedbackFromServer (string msg) {
		ChatEntry entry = new ChatEntry();
		entry.text = msg;
		chatEntries.Add(entry);
		RemoveOldEntries();
	}
}