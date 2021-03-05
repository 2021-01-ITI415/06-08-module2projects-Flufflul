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
	public Vector2 				fsPosMid = new Vector2(0.5f, 0.9f),
								fsPosRun = new Vector2(0.5f, 0.75f),
								fsPosMid2 = new Vector2(0.4f, 1f),
								fsPosEnd = new Vector2(0.5f, 0.95f);

	[Header("Set Dynamically")]
	public Deck					deck;
	public Layout				layout;
	public List<CardProspector> drawPile;
	public Transform			layoutAnchor;
	public CardProspector		target;
	public List<CardProspector>	tableau;
	public List<CardProspector>	discardPile;
	public FloatingScore		fsRun;

	void Awake(){
		S = this;
	}

	void Start() {
		Scoreboard.S.score = ScoreManager.SCORE;

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

		foreach (CardProspector tCP in tableau) {
			foreach (int hid in tCP.slotDef.hiddenBy) {
				cp = FindCardByLayoutID(hid);
				tCP.hiddenBy.Add(cp);
			}
		}

		MoveToTarget(Draw());
		UpdateDrawPile();
	}

	CardProspector FindCardByLayoutID(int layoutID) {
		foreach (CardProspector tCP in tableau) {
			if (tCP.layoutID == layoutID) { return tCP; }
		}

		return null;
	}

	// Determine card face up or down
	void SetTableauFaces() {
		foreach (CardProspector card in tableau) {
			bool faceUp = true;

			foreach (CardProspector cover in card.hiddenBy) {
				if (cover.state == eCardState.tableau) { faceUp = false; }
			}

			card.faceUp = faceUp;
		}
	}

	public void CardClicked(CardProspector card) {
		switch (card.state) {
			case eCardState.target:
				

			break;
			case eCardState.drawpile:
				MoveToDiscard(target);
				MoveToTarget(Draw());
				UpdateDrawPile();

				ScoreManager.EVENT(eScoreEvent.draw);
				FloatingScoreHandler(eScoreEvent.draw);
				
			break;
			case eCardState.tableau:
				bool validMatch = true;

				if (!card.faceUp) { validMatch = false; }
				if (!AdjacentRank(card, target)) { validMatch = false; }
				if(!validMatch) { return; }

				tableau.Remove(card);
				MoveToTarget(card);
				SetTableauFaces();

				ScoreManager.EVENT(eScoreEvent.mine);
				FloatingScoreHandler(eScoreEvent.mine);

			break;
		}

		CheckForGameOver();
	}

	void CheckForGameOver() {
		if (tableau.Count == 0) { GameOver(true); return; } // If there are no more cards in the mine, end.
		if (drawPile.Count > 0) { return; } // If there are still cards in the draw pile, stay.

		// If there is still a possible play to be made, stay.
		foreach (CardProspector card in tableau) {
			if (AdjacentRank(card, target)) { return; }
		}

		GameOver(false);
	}

	void GameOver(bool done) {
		if (done) { 
			ScoreManager.EVENT(eScoreEvent.gameWin);
			FloatingScoreHandler(eScoreEvent.gameWin);
		}
		else { 
			ScoreManager.EVENT(eScoreEvent.gameLoss);
			FloatingScoreHandler(eScoreEvent.gameLoss);
		}

		SceneManager.LoadScene("__Prospector_Scene_0");
	}

	void FloatingScoreHandler(eScoreEvent evt) {
		List<Vector2> fsPts;
		
		switch(evt) {
			case eScoreEvent.draw:
			case eScoreEvent.gameWin:
			case eScoreEvent.gameLoss:
				if (fsRun != null) {
					fsPts = new List<Vector2>();
					fsPts.Add(fsPosRun);
					fsPts.Add(fsPosMid2);
					fsPts.Add(fsPosEnd);

					fsRun.reportFinishTo = Scoreboard.S.gameObject;
					fsRun.Init(fsPts, 0, 1);
					fsRun.fontSizes = new List<float>(new float[] {28, 36, 4});
					fsRun = null;
				}
			break;
			case eScoreEvent.mine:
				FloatingScore fs;

				Vector2 p0 = Input.mousePosition;
				p0.x /= Screen.width;
				p0.y /= Screen.height;

				fsPts = new List<Vector2>();
				fsPts.Add(p0);
				fsPts.Add(fsPosMid);
				fsPts.Add(fsPosRun);

				fs = Scoreboard.S.CreateFloatingScore(ScoreManager.CHAIN, fsPts);
				fs.fontSizes = new List<float>(new float[] {4, 50, 28});

				if (fsRun == null) {
					fsRun = fs;
					fsRun.reportFinishTo = null;
				}
				else {
					fs.reportFinishTo = fsRun.gameObject;
				}
			
			break;
		}
	}

	public bool AdjacentRank(CardProspector c0, CardProspector c1) {
		if (!c0.faceUp || !c1.faceUp) { return false; }

		if (Mathf.Abs(c0.rank - c1.rank) == 1) { return true; } // Adjacent numerics
		if ((c0.rank == 1 && c1.rank == 13) 
		|| (c0.rank == 13 && c1.rank == 1)) { return true; } // Edge cases (King / Ace)

		return false;
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
