using MUD;
using UnityEngine;
using System.Collections;

namespace MUD {
	public class Door : Stuff {
		public bool doorOpen;
		
		public Door(stuffTypes newType, Map newMap) : base (newType, newMap) {
			doorOpen = false;
			
			switch (stuffType) {
				case stuffTypes.PiasMoveis :
					stuffName = "PIAS-MOVEIS";
					stuffDescription = "As Pias Moveis dos banheiros de Hogwarts sao um pouco suspeitas, voce nao acha?";
					break;
			}
		}
		
		public string toggleDoor(Player player, Chat world) {
			string doorMsg;
			
			if (player.onMap == this.isOnMap) {
				if (this.doorOpen) {
					this.doorOpen = false;
					world.EventAnnouncementMessages(player.playerName + " fechou " + this.stuffName, player.onMap);
					doorMsg = "Voce fechou " + this.stuffName;
				} else {
					this.doorOpen = true;
					world.EventAnnouncementMessages(player.playerName + " abriu " + this.stuffName, player.onMap);
					doorMsg = "Voce abriu " + this.stuffName;
				}
			} else {
				doorMsg = "Voce precisa estar no mesmo mapa de " + this.stuffName + ".";
			}
			
			return doorMsg;
		}
	
		// Use this for initialization
		void Start () {
		
		}
		
		// Update is called once per frame
		void Update () {
		
		}
	}
}