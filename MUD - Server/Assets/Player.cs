using MUD;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace MUD {
	public class Player {
		public string playerName;
		public Map onMap;
		public NetworkPlayer networkPlayer;
		public NetworkPeerType peerType;
		public List<Stuff> stuffList = new List<Stuff>();
		public int health, mana;
		public enum statusTypes { Paralyze };
		public statusTypes status;
		private Chat world;
		
		public Player (string newName, Map newMap, NetworkPlayer newNetworkPlayer, NetworkPeerType newPeerType, Chat newWorld) {
			world = newWorld;
			playerName = newName;
			onMap = newMap;
			networkPlayer = newNetworkPlayer;
			peerType = newPeerType;
			health = 100;
			mana = 100;
		}
		
		// Use this for initialization
		void Start () {
			
		}
		
		// Update is called once per frame
		void Update () {
			
		}
		
		public string playerInfo() {
			string info;
			info = this.playerName + Environment.NewLine;
			info = info + "HP: " + this.health + "   ";
			info = info + "MP: " + this.mana + Environment.NewLine;
			info = info + "Voce esta em: " + this.onMap.name + Environment.NewLine;
			info = info + "Inventario: ";
			foreach (Stuff item in stuffList) {
				info = info + item.stuffName + ", ";
			}
			
			return info;
		}
		
		public string grabStuff(Stuff theStuff) {
			string returnMsg;
			//testa todos os tipos de "portas" e objetos nao pegaveis
			if (theStuff is Door) {
				returnMsg = "Voce nao pode pegar uma porta.";
			} else {
				theStuff.isOnMap = null;
				this.onMap.stuffHere.Remove(theStuff);
				theStuff.isWithPlayer = this;
				stuffList.Add(theStuff);
				returnMsg = "Voce obteve: " + theStuff.stuffName + "!";
			}
			
			return returnMsg;
		}
		
		public string dropStuff(Stuff theStuff) {
			theStuff.isOnMap = this.onMap;
			this.onMap.stuffHere.Add (theStuff);
			theStuff.isWithPlayer = null;
			stuffList.Remove(theStuff);
			
			return "Voce largou: " + theStuff.stuffName + "!";
		}
		
		public string moveTo(Map destination, string direction) {
			string returnStr = null;
			Map prevMap = this.onMap;
			
			//testar se existe um mapa nesta direcao
			if (destination != null) {
				//testar se o mapa tem uma "porta" ou se a "porta" esta trancada
				if (destination.requiredDoorToEnter == null || destination.requiredDoorToEnter.doorOpen) {
					//testar se o mapa requer que o jogador tenha um item no seu inventario
					if (destination.requiredItemToEnter == null) {
						this.onMap.playersHere.Remove(this);
						this.onMap = destination;
						destination.playersHere.Add(this);
						
						returnStr = "Voce esta em: " + destination.name + "\n";
						returnStr = returnStr + destination.description;
						world.EventAnnouncementMessages(this.playerName + " moveu-se para o " + direction, prevMap);
					} else {
						//testar se o jogador possui o item requerido no seu inventario
						if (stuffList.Contains(destination.requiredItemToEnter)) {
							this.onMap.playersHere.Remove(this);
							this.onMap = destination;
							destination.playersHere.Add(this);
							
							//remover o item do jogador e devolve-lo para sua sala de origem
							destination.requiredItemToEnter.isWithPlayer = null;
							destination.requiredItemToEnter.isOnMap = destination.requiredItemToEnter.originalMap;
							destination.requiredItemToEnter.originalMap.stuffHere.Add(destination.requiredItemToEnter);
							this.stuffList.Remove(destination.requiredItemToEnter);
							
							returnStr = "Voce usou " + destination.requiredItemToEnter.stuffName + " para entrar." + Environment.NewLine;
							returnStr = returnStr + "Voce esta em: " + destination.name + ": " + destination.description;
							world.EventAnnouncementMessages(this.playerName + " moveu-se para o " + direction, prevMap);
						} else {
							returnStr = "Voce precisa de " + destination.requiredItemToEnter.stuffName + " para entrar em " + destination.name;
						}
					}
				} else {
					returnStr = destination.name + " esta trancado por " + destination.requiredDoorToEnter.stuffName + " e voce nao pode passar.";
				}
			} else {
				returnStr = "Voce nao pode ir nesta direcao.";
			}
			
			return returnStr;
		}
		
		public string processRemoteMessage (string msg) {
			List<Stuff> stuffListClone;
			string firstTerm, secondTerm, thirdTerm, returnMsg;
			string[] messageArray = msg.Split(' ');
			returnMsg = msg;
			
			if (messageArray.Length > 0 && messageArray[0] != null && messageArray[0] != string.Empty) {
				firstTerm = messageArray[0];
			} else {
				firstTerm = "none";
			}
			
			if (messageArray.Length > 1 && messageArray[1] != null && messageArray[1] != string.Empty) { 
				secondTerm = messageArray[1]; 
			} else {
				secondTerm = "none";
			}
			
			if (messageArray.Length > 2 && messageArray[2] != null && messageArray[2] != string.Empty) { 
				thirdTerm = messageArray[2]; 
			} else {
				thirdTerm = "none";
			}
			
			switch (firstTerm) {
			case "MOVER" :
				if (secondTerm == "NORTE") {
					returnMsg = moveTo(onMap.getNorth(), "norte");
				}
				else if (secondTerm == "SUL") {
					returnMsg = moveTo(onMap.getSouth(), "sul");
				}
				else if (secondTerm == "LESTE") {
					returnMsg = moveTo(onMap.getEast(), "leste");
				}
				else if (secondTerm == "OESTE") {
					returnMsg = moveTo(onMap.getWest(), "oeste");
				}
				else {
					returnMsg = "Localizacao desconhecida.";
				}
				break;
				
			case "EXAMINAR" :
				if (secondTerm == "none") {
					returnMsg = this.onMap.Analyze();
				} else {
					foreach (Stuff aStuff in world.stuffList) {
						if (aStuff != null && aStuff.stuffName == secondTerm && 
							(aStuff.isOnMap == this.onMap || aStuff.isWithPlayer == this)) {
							returnMsg = aStuff.Analyze();
							break;
						} else {
							returnMsg = "Voce nao possui este item e nao ha este item nesta sala.";
						}
					}
				}
				break;
				
			case "PEGAR" :
				stuffListClone = this.onMap.stuffHere;
				
				if (stuffListClone.Count > 0) {
					if (secondTerm != "none") {
						foreach (Stuff someStuff in stuffListClone) {
							if (someStuff.stuffName == secondTerm) {
								returnMsg = grabStuff(someStuff);
								break;
							} else {
								returnMsg = "Item inexistente.";
							}
						}
					} else {
						returnMsg = "Seja mais especifico.";
					}
				} else {
					returnMsg = "Nao ha itens neste mapa.";
				}
				
				break;
				
			case "LARGAR" :
				stuffListClone = this.stuffList;
				
				if (stuffListClone.Count > 0) {
					if (secondTerm != "none") {
						foreach (Stuff someStuff in stuffListClone) {
							if (someStuff.stuffName == secondTerm) {
								returnMsg = dropStuff(someStuff);
								break;
							} else {
								returnMsg = "Item inexistente.";
							}
						}
					} else {
						returnMsg = "Seja mais especifico.";
					}
				} else {
					returnMsg = "Voce nao possui itens no seu inventario.";
				}
				break;
				
			case "NPC" :
				if (secondTerm != "none") {
					foreach (NPC theNPC in this.onMap.npcsHere) {
						if (theNPC.npcName == secondTerm) {
							if (thirdTerm == "none") {
								returnMsg = theNPC.Analyze();
								break;
							} else {
								foreach (Quest aQuest in theNPC.availableQuests) {
									if (thirdTerm == aQuest.questName) {
										returnMsg = aQuest.completeQuest(this);
										break;
									} else {
										returnMsg = "Quest indisponivel.";
									}
								}
							}
						} else {
							returnMsg = "Este NPC nao se encontra nesta sala.";
						}
					}
				} else {
					returnMsg = "Seja mais especifico.";
				}
				break;
				
			case "INVENTARIO" :
				stuffListClone = this.stuffList;
				
				if (stuffListClone.Count > 0) {
					returnMsg = "";
					foreach (Stuff someStuff in stuffListClone) {
						returnMsg = returnMsg + someStuff.stuffName + ": " + someStuff.stuffDescription + Environment.NewLine;
					}
				} else {
					returnMsg = "Voce nao possui itens no seu inventario.";
				}
				break;
				
			case "USAR" :
				stuffListClone = this.stuffList;
				
				//testa se o jogador possui itens
				if (stuffListClone.Count > 0) {
					//testa de o jogador especificou o item
					if (secondTerm != "none") {
						//procura e testa se o jogador possui o item
						foreach (Stuff someStuff in stuffListClone) {
							if (someStuff.stuffName == secondTerm) {
								Key theKey = someStuff as Key;
								if (theKey != null) {
									if (theKey.keyToDoor.stuffName == thirdTerm) {
										returnMsg = theKey.keyToDoor.toggleDoor(this, world);
										break;
									} else {
										returnMsg = theKey.failedToUse;
									}
								} else {
									Stuff theStuff = someStuff;
									returnMsg = theStuff.Use(this, world);
									break;
								}
							} else {
								returnMsg = "Voce nao possui um " + secondTerm + ".";
							}
						}
					} else { 
						returnMsg = "Seja mais especifico."; 
					}
				} else {
					returnMsg = "Voce nao possui itens no seu inventario.";
				}
				break;
				
			case "COCHICHAR" :
				if (world.playerList.Count > 1) {
					foreach (Player aPlayer in world.playerList) {
						if (aPlayer.playerName == secondTerm) {
							if (aPlayer.onMap == this.onMap) {
								string newMsg = "";
								
								foreach (string word in messageArray) {
									if (word != firstTerm && word != secondTerm) {
										newMsg = newMsg + word + " ";
									}
								}

								world.SendPrivateMessage(aPlayer, this, newMsg);
								returnMsg = "Para " + aPlayer.playerName + ": " + newMsg;
								break;
							} else {
								returnMsg = "O jogador nao se encontra na mesma sala que voce.";
								break;
							}
						} else {
							returnMsg = "Jogador off-line.";
						}
					}
				} else {
					returnMsg = "Nao existem outros jogadores on-line.";
				}
				
				break;
			
			case "AJUDA" :
				returnMsg = "AJUDA:" + Environment.NewLine;
				returnMsg = returnMsg + "~*~*~*~*~*~*~*~*~*~*~" + Environment.NewLine;
				returnMsg = returnMsg + "MOVER <NORTE/SUL/LESTE/OESTE>: Move o seu personagem na direcao escolhida." + Environment.NewLine;
				returnMsg = returnMsg + "EXAMINAR: Exibe uma descricao detalhada da sala onde o seu personagem se encontra." + Environment.NewLine;
				returnMsg = returnMsg + "EXAMINAR <OBJETO>: Exibe uma descricao detalhada do objeto mencionado." + Environment.NewLine;
				returnMsg = returnMsg + "PEGAR <OBJETO>: Pega o objeto mencionado e adiciona ao inventario do seu personagem." + Environment.NewLine;
				returnMsg = returnMsg + "LARGAR <OBJETO>: Solta o objeto mencionado no mapa onde o seu personagem se encontra." + Environment.NewLine;
				returnMsg = returnMsg + "NPC <NOME>: Exibe uma descricao detalhada do NPC mencionado, incluindo as suas quests disponiveis." + Environment.NewLine;
				returnMsg = returnMsg + "NPC <NOME> <QUEST>: Para completar uma quest." + Environment.NewLine;
				returnMsg = returnMsg + "INVENTARIO: Lista todos os objetos no inventario do seu personagem e as suas descricoes." + Environment.NewLine;
				returnMsg = returnMsg + "USAR <OBJETO> <ALVO>: Usa o objeto mencionado no alvo de sua escolha." + Environment.NewLine;
				returnMsg = returnMsg + "COCHICHAR <JOGADOR>: Envia uma mensagem privada para o jogador mencionado." + Environment.NewLine;
				returnMsg = returnMsg + "AJUDA: Exibe a lista de comandos. :)" + Environment.NewLine;
				break;
			}
			
			return returnMsg;
		}
			
	}
}