using MUD;
using UnityEngine;
using System.Collections;

namespace MUD {
	public class Key : Stuff {
		public Door keyToDoor;
		
		public Key(stuffTypes newType, Map newMap, Door newKeyToDoor) : base (newType, newMap) {
			keyToDoor = newKeyToDoor;
			
			switch (stuffType) {
				case stuffTypes.OfidioDicionario :
					stuffName = "OFIDICIONARIO";
					stuffDescription = "Um grosso dicionario de Ofidioglossia, para que qualquer bruxo possa abrir as portas para a Camara Secreta, mesmo que nao seja um Ofidioglota nato.";
					failedToUse = "Voce pronunciou as palavras estranhas contidas no dicionario, mas nada aconteceu.";
					break;
			}
		}
	
		// Use this for initialization
		void Start () {
		
		}
		
		// Update is called once per frame
		void Update () {
		
		}
	}
}