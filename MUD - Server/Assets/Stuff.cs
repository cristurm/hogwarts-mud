using MUD;
using UnityEngine;
using System.Collections;

namespace MUD {
	public class Stuff {
		public enum stuffTypes { EspadaGryff, PiasMoveis, Losna, CascaWiggentree, PotWigg, PotMortoVivo, Mandragora, OfidioDicionario };
		public stuffTypes stuffType;
		public Map isOnMap;
		public readonly Map originalMap;
		public Player isWithPlayer;
		public string stuffName, stuffDescription, failedToUse, succededToUse;
		public float spawnTime;
		public bool isUsable;
		
		public Stuff (stuffTypes newType, Map newMap) {
			stuffName = "Std Name";
			stuffDescription = "Std Desc";
			failedToUse = "Std Msg";
			stuffType = newType;
			isOnMap = newMap;
			originalMap = newMap;
			isWithPlayer = null;
			spawnTime = 10.0f;
			isUsable = false;
			
			switch (stuffType) {
			case stuffTypes.EspadaGryff :
				stuffName = "ESPADA-DE-GRYFFINDOR";
				stuffDescription = "A espada tem o nome de Godric Griffyndor escrito nela, somente quem a possuir pode entrar no Quarto de Gryffindor.";
				failedToUse = "Voce tenta balancar a espada no ar mas nada especial acontece.";
				break;
			case stuffTypes.CascaWiggentree :
				stuffName = "CASCA-DE-WIGGENTREE";
				stuffDescription = "Pedaco da dura casca de uma Wiggentree.";
				failedToUse = "Voce esfregou a casca no alvo mas nada de especial aconteceu.";
				break;
			case stuffTypes.Losna :
				stuffName = "LOSNA";
				stuffDescription = "Planta esverdeada e com odor forte.";
				failedToUse = "Voce esfregou a planta no alvo mas nada de especial aconteceu.";
				break;
			case stuffTypes.Mandragora :
				stuffName = "MUDA-DE-MANDRAGORA";
				stuffDescription = "Uma muda nova e dorminhoca de Mandragora. Nao tente acorda-la!";
				failedToUse = "Voce chacoalhou a mandragora e ela deu um grito ensurdecedor mas logo voltou a dormir.";
				succededToUse = failedToUse;
				isUsable = true;
				break;
			case stuffTypes.PotWigg :
				stuffName = "POCAO-WIGGENWELD";
				stuffDescription = "Pocao de facil manuseio e cor verde. Tem o poder de restaurar as forças de uma pessoa que esta demasiada fraca. Restaura 50 de HP";
				failedToUse = "Voce ensopou o alvo com a pocao mas nada de especial aconteceu.";
				succededToUse = "Voce bebeu a pocao e teve seu HP restaurado.";
				isUsable = true;
				break;
			case stuffTypes.PotMortoVivo :
				stuffName = "POCAO-MORTO-VIVO";
				stuffDescription = "Uma pocao poderozissima, capaz de fazer uma criatura adormecer por muito tempo.";
				failedToUse = "Voce ensopou o alvo com a pocao mas nada de especial aconteceu.";
				succededToUse = "Vice bebeu a pocao e ficou paralizado.";
				isUsable = true;
				break;
			}
		}
		
		public string Analyze() {
			return this.stuffName + ": " + this.stuffDescription;
		}
		
		public string Use(Player player, Chat world) {
			string returnStr = "";
			
			if (this.isUsable) {
				returnStr = succededToUse;
				switch (stuffType) {
				case stuffTypes.PotWigg :
					player.health = player.health + 50;
					player.stuffList.Remove(this);
				this.isWithPlayer = null;
					break;
				case stuffTypes.PotMortoVivo :
					player.status = Player.statusTypes.Paralyze;
					player.stuffList.Remove(this);
				this.isWithPlayer = null;
					break;
				case stuffTypes.Mandragora : 
					world.EventAnnouncementMessages("!!!AAAAAaaaAAAAAaaaAAAAAaaaaa!!!AAAAAAaaaaaAAAAAAAAAaaaa!!!", player.onMap);
					break;
				}
			} else {
				returnStr = this.failedToUse;
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