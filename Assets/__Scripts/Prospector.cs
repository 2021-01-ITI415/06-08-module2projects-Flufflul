using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class Prospector : MonoBehaviour {

	static public Prospector 	S;

	[Header("Set in Inspector")]
	public TextAsset			deckXML;


	[Header("Set Dynamically")]
	public Deck					deck;

	void Awake(){
		S = this;
	}

	void Start() {
		deck = GetComponent<Deck> ();
		deck.InitDeck (deckXML.text);

		Deck.Shuffle(ref deck.cards);

		Card c;
		// For all cards in deck, lay the cards evenly between:
		// 		x = cNum % 13 * 3 : Horizontal layer will never exceed all 13 values
		// 		y = cNum / 13 * 4 : Vertical layer will incrimentally raise at each cNum factor of 13
		for (int cNum = 0; cNum < deck.cards.Count; cNum++) {
			c = deck.cards[cNum];
			c.transform.localPosition = new Vector3(
				(cNum % 13) * 3, 
				cNum / 13 * 4, 
				0
			);
		}
	}

}
