using MUD;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Chat : MonoBehaviour {
	public bool usingChat = false;		//Can be used to determine if we need to stop player movement since we're chatting
	public GUISkin skin;				//Skin
	public bool showChat = false;		//Show/Hide the chat
	
	public Map salaComunal, escadarias, banheiros, quartoGriff, quartoSlyth, refeitorio, estufa, camaraSecreta;
	public Stuff espadaGriff, cascaWigg, losna, mandragora, piasMoveis, ofidioDicionario, potWigg, potMortoVivo;
	public NPC dumbledore, snape;
	public Quest fazerWigg, fazerMortoVivo;
	
	//lists
	public List<Player> playerList = new List<Player>();
	public List<Stuff> stuffList = new List<Stuff>();
	private List<Map> mapList = new List<Map>();
	private ArrayList chatEntries;
	
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
		//window = new Rect(Screen.width/2-width/2, Screen.height-height+5, width, height);
		window = new Rect(Screen.width-width, Screen.height-height+5, width, height);
		
		//We get the name from the masterserver example, if you entered your name there ;).
		/*playerName = PlayerPrefs.GetString("playerName", "");
		if(string.IsNullOrEmpty(Connect.playerName))
		{
			playerName = "RandomName"+ UnityEngine.Random.Range(1,999);
		}*/
		//Connect myConnect = Camera.main.gameObject.GetComponent<Connect>();
		//Connect myConnect = GameObject.FindGameObjectWithTag("Code");
	}
	
	
	public void Start() {
		// let's create our Maps
		salaComunal =  new Map("Sala Comunal", "Sala de comunhao entre todos os alunos de Hogwarts.", this);
		escadarias = new Map ("Escadarias", "Escadaria do castelo, as escadas tem vida propria e se movem, tenha cuidado para nao se perder!", this);
		estufa = new Map("Estufa da Professora Sprout", "Na estufa da professora Sprout sao guardadas as ervas medicinais e ingredientes para todos os tipos de pocoes.", this);
		banheiros = new Map("Banheiros", "Os banheiros estao um pouco umidos demais, provavelmente por que o Troll espanta a todos por muito tempo.", this);
		quartoGriff = new Map("Quarto de Gryffindor", "Quarto dos membros da Casa de Griffindor.", this);
		quartoSlyth = new Map("Quarto de Slytherin", "Quarto dos membros da Casa de Slytherin.", this);
		refeitorio = new Map("Refeitorios", "Refeitorios onde sao realizadas as refeicoes e celebracoes.", this);
		camaraSecreta = new Map("Camara Secreta", "Camara criada por Salazar Slytherin, lar do poderoso Basilisk!", this);
		
		escadarias.randomizeExits = true;
		
		// then we link them
		refeitorio.setDirections(salaComunal, null, null, null);
		salaComunal.setDirections(escadarias, refeitorio, quartoSlyth, null);
		escadarias.setDirections(estufa, salaComunal, quartoGriff, banheiros);
		quartoGriff.setDirections(null, null, null, escadarias);
		quartoSlyth.setDirections(null, null, null, salaComunal);
		estufa.setDirections(null, escadarias, null, null);
		banheiros.setDirections(camaraSecreta, null, escadarias, null);
		camaraSecreta.setDirections(null, banheiros, null, null);
		
		// now we list them
		mapList.Add(salaComunal);
		mapList.Add(escadarias);
		mapList.Add(estufa);
		mapList.Add(banheiros);
		mapList.Add(quartoGriff);
		mapList.Add(quartoSlyth);
		mapList.Add(refeitorio);
		mapList.Add(camaraSecreta);
		
		// create and assign our items
		espadaGriff = new Stuff(Stuff.stuffTypes.EspadaGryff, banheiros);
		banheiros.stuffHere.Add(espadaGriff);
		
		piasMoveis = new Door(Stuff.stuffTypes.PiasMoveis, banheiros);
		banheiros.stuffHere.Add(piasMoveis);
		
		cascaWigg = new Stuff(Stuff.stuffTypes.CascaWiggentree, estufa);
		estufa.stuffHere.Add(cascaWigg);
		
		losna = new Stuff(Stuff.stuffTypes.Losna, estufa);
		estufa.stuffHere.Add(losna);
		
		mandragora = new Stuff(Stuff.stuffTypes.Mandragora, estufa);
		estufa.stuffHere.Add(mandragora);
		
		ofidioDicionario = new Key(Stuff.stuffTypes.OfidioDicionario, quartoSlyth, (Door)piasMoveis);
		quartoSlyth.stuffHere.Add(ofidioDicionario);
		
		potWigg = new Stuff(Stuff.stuffTypes.PotWigg, null);
		potMortoVivo = new Stuff(Stuff.stuffTypes.PotMortoVivo, null);
		
		// list our items
		stuffList.Add(espadaGriff);
		stuffList.Add(piasMoveis);
		stuffList.Add(cascaWigg);
		stuffList.Add(losna);
		stuffList.Add(mandragora);
		stuffList.Add(ofidioDicionario);
		
		// set our item permissions
		quartoGriff.requiredItemToEnter = espadaGriff;
		
		// set our door permissions
		camaraSecreta.requiredDoorToEnter = (Door)piasMoveis;
		
		// create our NPCs and put them on a map
		dumbledore = new NPC(NPC.masterWizards.Dumbledore, refeitorio);
		refeitorio.npcsHere.Add(dumbledore);
		snape = new NPC(NPC.masterWizards.Snape, refeitorio);
		refeitorio.npcsHere.Add(snape);
		
		// create our available quests
		fazerWigg = new Quest("WIGGENWELD", "A Pocao Wiggenweld e essencial. Traga-me uma Casca de Wiggentree se voce deseja aprender como preparar a Pocao Wiggenweld.", cascaWigg, potWigg);
		fazerMortoVivo = new Quest("MORTO-VIVO", "A Pocao do Morto-Vivo e extremamente eficiente contra tudo o que respira. Traga-me uma muda de Losna se voce deseja aprender como prepara-la.", losna, potMortoVivo);
		snape.availableQuests.Add(fazerWigg);
		snape.availableQuests.Add(fazerMortoVivo);
	}
	
	public void Update() {
		foreach (Map aMap in mapList) {
			if (aMap.randomizeExits) {
				aMap.Update();
			}
		}
	}
	
	/*private void AddToPlayerList(string playerName, NetworkPlayer networkPlayer) {
		Player newPlayer = new Player(playerName, salaComunal, networkPlayer);
		salaComunal.playersHere.Add(newPlayer);
		playerList.Add(newPlayer);
		addGameChatMessage(newPlayer.playerName + " joined the chat");
	}*/
	
	//Client function
	/*public void OnConnectedToServer() {
		playerName = transform.GetComponent<Connect>().playerName;
		ShowChatWindow();
		AddToPlayerList(playerName, Network.player);
		//networkView.RPC ("TellServerOurName", RPCMode.Server, playerName);
		// //We could have also announced ourselves:
		// addGameChatMessage(playerName" joined the chat");
		// //But using "TellServer.." we build a list of active players which we can use for other stuff as well.
	}*/
	
	//Server function
	public void OnServerInitialized() {
		playerName = transform.GetComponent<Connect>().playerName;
		ShowChatWindow();
		Player newPlayer = new Player(playerName, salaComunal, Network.player, NetworkPeerType.Server, this);
		playerList.Add(newPlayer);
		salaComunal.playersHere.Add(newPlayer);
		transform.GetComponent<Connect>().server = newPlayer;
		addGameChatMessage(newPlayer.playerName + " esta on-line.");
		//I wish Unity supported sending an RPC on the server to the server itself :(
		// If so; we could use the same line as in "OnConnectedToServer();"
		/*Player newPlayer = new Player(playerName, salaComunal, Network.player);
		playerList.Add(newPlayer);
		
		addGameChatMessage(newPlayer.playerName + " joined the chat");*/
	}
	
	//A handy wrapper function to get the PlayerNode by networkplayer
	public Player GetPlayer(NetworkPlayer networkPlayer) {
		foreach (Player player in playerList)
		{
			if(player.networkPlayer == networkPlayer)
			{
				return player;
			}
		}
		Debug.LogError("GetPlayer: Requested a playernode of non-existing player!");
		return null;
	}
	
	//A handy function to pick a Player from the PlayerList by its name
	public Player GetPlayerByName (string name) {
		foreach (Player player in playerList)
		{
			if(player.playerName == name)
			{
				return player;
			}
		}
		Debug.LogError("GetPlayerByName: This player does not exist!");
		return null;
	}
	
	//Server function
	public void OnPlayerDisconnected(NetworkPlayer player) {
		Player gonePlayer = GetPlayer(player);
		addGameChatMessage(gonePlayer.playerName + " esta off-line.");
		
		//Remove player from the server list
		gonePlayer.onMap.playersHere.Remove(gonePlayer);
		playerList.Remove(gonePlayer);
	}
	
	public void OnDisconnectedFromServer() { 
		foreach (Map map in mapList) {
			map.playersHere.Clear();
		}
		playerList.Clear();
		CloseChatWindow();
	}
	
	//Server function
	public void OnPlayerConnected(NetworkPlayer player) { 
		//addGameChatMessage("Player connected from: " + player.ipAddress + ":" + player.port); 
		networkView.RPC("ApplyGlobalChatText", player, "", "Bem vindo a Hogwarts!");
	}
	
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
		ProcessMessage(Network.player, msg);	
		inputField = ""; //Clear line
		//GUI.UnfocusWindow();//Deselect chat
		//lastUnfocusTime = Time.time;
		usingChat = true;
	}
	
	public void ProcessMessage(NetworkPlayer netPlayer, string msg) {
		Player thePlayer = GetPlayer(netPlayer);
		string feedbackMsg = thePlayer.processRemoteMessage(msg);
		
		if (feedbackMsg == msg) {
			foreach (Player aPlayer in thePlayer.onMap.playersHere) {
				if (aPlayer != null) {
					if (aPlayer.peerType == NetworkPeerType.Server) {
						ApplyGlobalChatText(thePlayer.playerName, msg);	
					} else {
						networkView.RPC("ApplyGlobalChatText", aPlayer.networkPlayer, thePlayer.playerName, msg);
					}
				}
			}
		} else {			
			if (thePlayer.peerType == NetworkPeerType.Server) {
				ApplyGlobalChatText("", feedbackMsg);
			} else {
				networkView.RPC("ApplyGlobalChatText", netPlayer, "", feedbackMsg);
			}
		}
	}
	
	public void SendPrivateMessage(Player toPlayer, Player fromPlayer, string msg) {
		string fromPlayerLabel = "De " + fromPlayer.playerName;
		if (toPlayer.peerType == NetworkPeerType.Server) {
			ApplyGlobalChatText(fromPlayerLabel, msg);
		} else {
			networkView.RPC("ApplyGlobalChatText", toPlayer.networkPlayer, fromPlayerLabel, msg);
		}
	}
	
	public void EventAnnouncementMessages(string msg, Map targetMap = null) {
		if (targetMap != null) {
			foreach (Player aPlayer in playerList) {
				if (aPlayer != null && aPlayer.onMap == targetMap) {
					if (aPlayer.peerType == NetworkPeerType.Server) {
						ApplyGlobalChatText("", msg);	
					} else {
						networkView.RPC("ApplyGlobalChatText", aPlayer.networkPlayer, "", msg);
					}
				}
			}
		} else {
			networkView.RPC("ApplyGlobalChatText", RPCMode.All, "", msg);	
		}
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
	public void TellServerOurName(string name, NetworkMessageInfo info) {
		bool validUserName = true;
		
		//Sent by newly connected clients, recieved by server
		if (playerList.Count > 0) {
			foreach (Player aPlayer in playerList) {
				if (aPlayer.playerName == name) {
					validUserName = false;
					Network.CloseConnection(info.sender, true);
					//something
					break;
				}
			}
		}
		
		if (validUserName) {
			Player newPlayer = new Player(name, salaComunal, info.sender, NetworkPeerType.Client, this);
			playerList.Add(newPlayer);
			salaComunal.playersHere.Add(newPlayer);
			addGameChatMessage(newPlayer.playerName + " esta on-line.");
		}
	}
	
	[RPC]
	public void ApplyGlobalChatText(string name, string msg) {
		ChatEntry entry = new ChatEntry();
		entry.fromName = name;
		entry.text = msg;
		chatEntries.Add(entry);
		RemoveOldEntries();
	}
	
	[RPC]
	public void MessageTreatement(NetworkPlayer netPlayer, string msg) {
		ProcessMessage(netPlayer, msg);
	}
}