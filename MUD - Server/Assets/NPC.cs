using MUD;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace MUD {
	public class NPC {
		public enum masterWizards { Dumbledore, Snape };
		public masterWizards wizard;
		public string npcName, npcQuote;
		public Map onMap;
		public List<Quest> availableQuests;
		
		public NPC(masterWizards newWizard, Map newNpcMap) {
			onMap = newNpcMap;
			availableQuests = new List<Quest>();
			
			switch(newWizard) {
			case masterWizards.Dumbledore :
				npcName = "ALBUS-DUMBLEDORE";
				npcQuote = "Nao vale a pena viver sonhando e esquecer de viver...";
				break;
			
			case masterWizards.Snape :
				npcName = "SEVERUS-SNAPE";
				npcQuote = "Eu posso lhe ensinar como engarrafar fama, cozinhar gloria, ate mesmo retardar a morte se voce nao for tao estupido quanto o bando de imbecis que eu tenho que ensinar.";
				break;
			}
		}
		
		public string Analyze() {
			string returnStr;
			
			returnStr = npcName + ": ";
			returnStr = returnStr + npcQuote + Environment.NewLine;
			
			if (this.availableQuests.Count > 0) {
				returnStr = returnStr + "QUESTS DISPONIVEIS:" + Environment.NewLine;
				foreach (Quest aQuest in this.availableQuests) {
					returnStr = returnStr + aQuest.questName + " - " + aQuest.questDescription + Environment.NewLine;
				}
			} else {
				returnStr = returnStr + "Nenhuma quest disponivel.";
			}
			
			return returnStr;
		}
	
		// Use this for initialization
		void Start () {
		
		}
		
		// Update is called once per frame
		void Update () {
		
		}
	}
}