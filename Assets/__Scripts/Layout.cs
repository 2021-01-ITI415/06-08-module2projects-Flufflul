using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SlotDef 
{
    public float x, y;
    public bool faceUp = false;
    public string layerName = "Default";
    public int layer_id = 0, id;
    public List<int> hiddenBy = new List<int>();
    public string type = "slot";
    public Vector2 stagger;
}

public class Layout : MonoBehaviour 
{
    public PT_XMLReader xmlR;
    public PT_XMLHashtable xml;
    public Vector2 multiplier;
    public List<SlotDef> slotDefs;
    public SlotDef drawPile, discardPile;
    public string[] sortingLayerNames = new string[] {"Row0","Row1","Row2","Row3","Discard","Draw"};

    public void ReadLayout (string xmlTxt) {
        xmlR = new PT_XMLReader();
        xmlR.Parse(xmlTxt);
        xml = xmlR.xml["xml"][0];

        multiplier.x = float.Parse(xml["multiplier"][0].att("x"));
        multiplier.y = float.Parse(xml["mulitplier"][0].att("y"));

        SlotDef tSD;
        PT_XMLHashList slotsX = xml["slot"];

        for (int i = 0; i < slotsX.Count; i++) {
            tSD = new SlotDef();
            
            if (slotsX[i].HasAtt("type")) { tSD.type = slotsX[i].att("type"); }
            else { tSD.type = "slot"; }

            tSD.x = float.Parse(slotsX[i].att("x"));
            tSD.y = float.Parse(slotsX[i].att("y"));
            tSD.layer_id = int.Parse(slotsX[i].att("layer"));
            tSD.layerName = sortingLayerNames[tSD.layer_id];

            // Additional attributes based on its SlotDef type
            switch (tSD.type) {
                case "slot":
                    tSD.faceUp = (slotsX[i].att("faceup") == "1");
                    tSD.id = int.Parse(slotsX[i].att("id"));
                    
                    if (slotsX[i].HasAtt("hiddenby")) {
                        string[] hiding = slotsX[i].att("hiddenby").Split(',');
                        foreach (string s in hiding) { tSD.hiddenBy.Add(int.Parse(s)); }
                    }

                    slotDefs.Add(tSD);

                    break;
                case "drawpile":
                    tSD.stagger.x = float.Parse(slotsX[i].att("xstagger"));
                    drawPile = tSD;

                    break;
                case "discardpile":
                    discardPile = tSD;

                    break;
            }
        }
    }
}
