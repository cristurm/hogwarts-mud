using MUD;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace MUD {
	public class Map {
		public string name;
		public string description;
		public List<Player> playersHere;
		public List<Stuff> stuffHere;
		public List<NPC> npcsHere;
		public Door requiredDoorToEnter;
		public Stuff requiredItemToEnter;
		public bool randomizeExits;
		
		private float timer;
		private int toggleIndex;
		private Chat world;
		private Map north, south, east, west;
		private Map safeNorth, safeSouth, safeEast, safeWest;
			
		public Map (string newName, string newDesc, Chat newWorld) {
			name = newName;
			description = newDesc;
			north = null;
			south = null;
			east = null;
			west = null;
			requiredDoorToEnter = null;
			requiredItemToEnter = null;
			playersHere = new List<Player>();
			stuffHere = new List<Stuff>();
			npcsHere = new List<NPC>();
			randomizeExits = false;
			timer = 10.0f;
			toggleIndex = 0;
			world = newWorld;
		}
		
		public void setDirections (Map atNorth, Map atSouth, Map atEast, Map atWest) {
			north = atNorth;
			safeNorth = north;
			
			south = atSouth;
			safeSouth = south;
			
			east = atEast;
			safeEast = east;
			
			west = atWest;
			safeWest = west;
		}
		
		public Map getNorth() { return this.north; }
		public Map getSouth() { return this.south; }
		public Map getEast() { return this.east; }
		public Map getWest() { return this.west; }
		
		private void toggleExits() {
			string msg = "";
			toggleIndex++;
			if (toggleIndex >= 4) toggleIndex = 0;
			
			//reset all the exits! ,o\
			north = null;
			south = null;
			east = null;
			west = null;			
			
			switch (toggleIndex) {
			case 0:
				north = safeNorth;
				break;
			case 1:
				south = safeSouth;
				break;
			case 2:
				east = safeEast;
				break;
			case 3: 
				west = safeWest;
				break;
			default:
				toggleIndex = 0;
				break;
			}
			
			if (north != null) { msg = msg + "NORTE: " + north.name + Environment.NewLine; } 
			else { msg = msg + "NORTE: --" + Environment.NewLine; }
			
			if (south != null) { msg = msg + "SUL: " + south.name + Environment.NewLine; } 
			else { msg = msg + "SUL: --" + Environment.NewLine; }
			
			if (east != null) { msg = msg + "LESTE: " + east.name + Environment.NewLine; } 
			else { msg = msg + "LESTE: --" + Environment.NewLine; }
			
			if (west != null) { msg = msg + "OESTE: " + west.name + Environment.NewLine; } 
			else { msg = msg + "OESTE: --" + Environment.NewLine; }
			
			world.EventAnnouncementMessages(msg, this);
		}
		
		public string Analyze() {
			string returnStr = null;
			
			returnStr = "Voce esta em: " + this.name + Environment.NewLine;
			returnStr = returnStr + this.description + Environment.NewLine;
			
			if(north != null) { returnStr = returnStr + "NORTE: " + north.name + Environment.NewLine; }
			if(south != null) { returnStr = returnStr + "SUL: " + south.name + Environment.NewLine; }
			if(east != null) { returnStr = returnStr + "LESTE: " + east.name + Environment.NewLine; }
			if(west != null) { returnStr = returnStr + "OESTE: " + west.name + Environment.NewLine; }
			
			if (stuffHere.Count > 0) {
				returnStr = returnStr + "Itens neste mapa: ";
				foreach (Stuff item in stuffHere) {
					returnStr = returnStr + item.stuffName + ", ";
				}
			} else {
				returnStr = returnStr + "Nao ha itens neste mapa.";
			}
			
			if (npcsHere.Count > 0) {
				returnStr = returnStr + Environment.NewLine + "NPCs neste mapa: ";
				foreach (NPC npc in npcsHere) {
					returnStr = returnStr + npc.npcName + ", ";
				}
			} else {
				returnStr = returnStr + Environment.NewLine + "Nao ha NPCs neste mapa.";
			}
			
			if (playersHere.Count > 0) {
				returnStr = returnStr + Environment.NewLine + "Bruxos neste mapa: ";
				foreach (Player player in playersHere) {
					returnStr = returnStr + player.playerName + ", ";
				}
			} else {
				returnStr = returnStr + Environment.NewLine + "Nao ha outros bruxos neste mapa.";
			}
				
			return returnStr;
		}
		
		// Update is called once per frame
		public void Update () {
			if (this.randomizeExits) {
				timer = timer + (1.0f * Time.deltaTime);
				
				if (timer > 10.0f) {
					timer = 1;
					this.toggleExits();
				}
			}
		}
	}
}