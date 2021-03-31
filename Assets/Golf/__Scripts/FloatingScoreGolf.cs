using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum eFSStateGolf {
    idle,
    pre,
    active,
    post
}

public class FloatingScoreGolf : MonoBehaviour
{
    [Header("Set Dynamically")]
    public eFSStateGolf         state = eFSStateGolf.idle;

    [SerializeField]
    protected int           _score = 0;
    public string           scoreString;

    public int score {
        get { return _score; }
        set { 
            _score = value; 
            scoreString = _score.ToString("N0");
            GetComponent<Text>().text = scoreString;
        }
    }

    public List<Vector2>    bezierPts;
    public List<float>      fontSizes;
    public float            timeStart = -1f;
    public float            timeDuration = 1f;
    public string           easingCurve = Easing.InOut; // Util.cs Easing
    
    public GameObject       reportFinishTo = null;

    private RectTransform   rectTransform;
    private Text            text;

    public void Init(List<Vector2> ePts, float eTimeS = 0, float eTimeD = 1) {
        rectTransform = GetComponent<RectTransform>();
        rectTransform.anchoredPosition = Vector2.zero;

        text = GetComponent<Text>();
        
        bezierPts = new List<Vector2>(ePts);
        if (ePts.Count == 1) {
            transform.position = ePts[0];
            return;
        }

        if (eTimeS == 0) { eTimeS = Time.time; }
        timeStart = eTimeS;
        timeDuration = eTimeD;

        state = eFSStateGolf.pre;
    }

    public void FSCallback(FloatingScoreGolf fs) {
        score += fs.score;
    }

    private void Update() {
        if (state == eFSStateGolf.idle) { return; }

        float u = (Time.time - timeStart) / timeDuration;
        float uC= Easing.Ease(u, easingCurve);

        if (u < 0) {
            state = eFSStateGolf.pre;
            text.enabled = false;
        }
        else {
            if (u >= 1) {
                uC = 1;
                state = eFSStateGolf.post;
                
                if (reportFinishTo != null) {
                    reportFinishTo.SendMessage("FSCallback", this);
                    Destroy(gameObject);
                }
                else {
                    state = eFSStateGolf.active;
                    text.enabled = true;
                }
            }
            else {
                Vector2 pos = Utils.Bezier(uC, bezierPts);

                rectTransform.anchorMin = rectTransform.anchorMax = pos;

                if (fontSizes != null && fontSizes.Count > 0) {
                    int size = Mathf.RoundToInt(Utils.Bezier(uC, fontSizes));
                    GetComponent<Text>().fontSize = size;
                }
            }
        }
    }
}
