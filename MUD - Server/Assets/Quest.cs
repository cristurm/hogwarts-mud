using MUD;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace MUD {
	public class Quest {
		public string questName, questDescription;
		public Stuff requiredStuff, resultStuff;
		
		public Quest(string newQuestName, string newQuestDesc, Stuff newReqStuff, Stuff newResultStuff) {
			questName = newQuestName;
			questDescription = newQuestDesc;
			requiredStuff = newReqStuff;
			resultStuff = newResultStuff;
		}
		
		// Use this for initialization
		void Start () {
		
		}
		
		// Update is called once per frame
		void Update () {
		
		}
		
		public string completeQuest(Player player) {
			string returnStr = "";
			
			if (player.stuffList.Count > 0) {
				foreach (Stuff item in player.stuffList) {
					if (item == requiredStuff) {
						//give resultItem to Player
						resultStuff.isWithPlayer = player;
						player.stuffList.Add(resultStuff);
						
						//remove reqItem from Player and return it to it's original room
						player.stuffList.Remove(item);
						item.isWithPlayer = null;
						item.isOnMap = item.originalMap;
						item.originalMap.stuffHere.Add(item);
						
						returnStr = "Voce obteve " + resultStuff.stuffName;
						break;
					} else {
						returnStr = "Voce nao possui o item '" + requiredStuff.stuffName + "', necessario para completar esta quest.";
					}
				}
			} else {
				returnStr = "Voce nao possui o item '" + requiredStuff.stuffName + "', necessario para completar esta quest.";
			}
			
			
			return returnStr;
		}
	}
}