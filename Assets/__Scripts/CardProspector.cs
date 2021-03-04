using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eCardState {
    drawpile,
    tableau,
    target,
    discard
}

public class CardProspector : Card
{
    [Header("Set Dynamically: CardProspector")]
    public eCardState state = eCardState.drawpile; // Editing the game state
    public List<CardProspector> hiddenBy = new List<CardProspector>(); // Cascading list that determines the side of cards beneath it
    public int layoutID; // Matches card to tableau XML iff tableau
    public SlotDef slotDef; // Info from LayoutXML<slot>

    public override void OnMouseUpAsButton() {
        Prospector.S.CardClicked(this);
        base.OnMouseUpAsButton(); // base = super, virtual ~ abstract
    }
}
