using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class Prospector : MonoBehaviour {

	static public Prospector 	S;

	[Header("Set in Inspector")]
	public TextAsset			deckXML;
	public TextAsset			layoutXML;
	public float 				xOffset = 3f, yOffset = -2.5f;
	public Vector3 				layoutCenter;

	[Header("Set Dynamically")]
	public Deck					deck;
	public Layout				layout;
	public List<CardProspector> drawPile;
	public Transform			layoutAnchor;
	public CardProspector		target;
	public List<CardProspector>	tableau;
	public List<CardProspector>	discardPile;

	void Awake(){
		S = this;
	}

	void Start() {
		deck = GetComponent<Deck> ();
		deck.InitDeck (deckXML.text);

		Deck.Shuffle(ref deck.cards);

		/*
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
		*/

		layout = GetComponent<Layout>();
		layout.ReadLayout(layoutXML.text);

		drawPile = ConvertListCardsToListCardProspectors(deck.cards);

		LayoutGame();
	}

	// Draw a card from the top of the draw pile
	CardProspector Draw() {
		CardProspector cd = drawPile[0];
		drawPile.RemoveAt(0);
		return cd;
	}

	// Organizes the cascading card layout
	void LayoutGame() {
		// Create an anchor for the tableau (a.k.a. the "mine")
		if (layoutAnchor == null) {
			GameObject tGO = new GameObject("_LayoutAnchor");
			layoutAnchor = tGO.transform;
			layoutAnchor.transform.position = layoutCenter;
		}
		
		CardProspector cp;
		foreach (SlotDef tSD in layout.slotDefs) {
			// Draw a card 
			cp = Draw();
			cp.faceUp = tSD.faceUp;

			// Initialize its position relative to the tableau
			cp.transform.parent = layoutAnchor;
			cp.transform.localPosition = new Vector3(
				layout.multiplier.x * tSD.x,
				layout.multiplier.y * tSD.y,
				-tSD.layer_id
			);

			cp.layoutID = tSD.id;
			cp.slotDef = tSD;
			cp.state = eCardState.tableau;

			cp.SetSortingLayerName(tSD.layerName);

			// After initialization, add it
			tableau.Add(cp);
		}
	}

	void MoveToDiscard(CardProspector card) {
		card.state = eCardState.discard;	
		discardPile.Add(card);
		card.transform.parent = layoutAnchor;

		card.transform.localPosition = new Vector3(
			layout.multiplier.x * layout.discardPile.x,
			layout.multiplier.y * layout.discardPile.y,
			-layout.discardPile.layer_id + 0.5f
		);

		card.faceUp = true;
		card.SetSortingLayerName(layout.discardPile.layerName);
		card.SetSortOrder(-100 + discardPile.Count);
	}

	void MoveToTarget(CardProspector card) {
		if (target != null) { MoveToDiscard(target); }
		
		target = card;
		card.state = eCardState.target;
		card.transform.parent = layoutAnchor;
		card.transform.localPosition = new Vector3(
			layout.multiplier.x * layout.discardPile.x,
			layout.multiplier.y * layout.discardPile.y,
			-layout.discardPile.layer_id
		);

		card.faceUp = true;
		
		card.SetSortingLayerName(layout.discardPile.layerName);
		card.SetSortOrder(0);
	}

	void UpdateDrawPile() {
		CardProspector card;
		for (int i = 0; i < drawPile.Count; i++) {
			card = drawPile[i];
			card.transform.parent = layoutAnchor;

			Vector2 dpStagger = layout.drawPile.stagger;
			card.transform.localPosition = new Vector3(
				layout.multiplier.x * (layout.drawPile.x + i * dpStagger.x),
				layout.multiplier.y * (layout.drawPile.y + i * dpStagger.y),
				-layout.drawPile.layer_id + 0.1f * i
			);

			card.faceUp = false;
			card.state = eCardState.drawpile;
			card.SetSortingLayerName(layout.drawPile.layerName);
			card.SetSortOrder(-10 * i);
		}
	}

	// Parse Card to CardProspector
	List<CardProspector> ConvertListCardsToListCardProspectors(List<Card> lCD) {
		List<CardProspector> lCP = new List<CardProspector>();
		CardProspector tCP;

		// Type cast each card to CardProspector
		foreach (Card tCD in lCD) {
			tCP = tCD as CardProspector;
			lCP.Add(tCP);
		}

		return lCP;
	}
}
