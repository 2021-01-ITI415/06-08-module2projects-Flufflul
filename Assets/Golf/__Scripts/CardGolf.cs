using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eCardStateGolf {
    drawpile,
    tableau,
    target,
    discard
}

public class CardGolf : Card
{
    [Header("Set Dynamically: CardGolf")]
    public eCardStateGolf state = eCardStateGolf.drawpile; // Editing the game state
    public List<CardGolf> hiddenBy = new List<CardGolf>(); // Cascading list that determines the side of cards beneath it
    public int layoutID; // Matches card to tableau XML iff tableau
    public SlotDefGolf slotDef; // Info from LayoutXML<slot>
    // public bool isGold = false;

    /*
    public override void OnMouseUpAsButton() {
        Prospector.S.CardClicked(this);
        base.OnMouseUpAsButton(); // base = super, virtual ~ abstract
    }
    */
}
