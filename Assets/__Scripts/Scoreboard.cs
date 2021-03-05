using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scoreboard : MonoBehaviour
{
    public static Scoreboard S;

    [Header("Set in Inspector")]
    public GameObject prefabFloatingScore;

    [Header("Set Dynamically")]
    [SerializeField] private int _score = 0;
    [SerializeField] private string _scoreString;

    private Transform canvasTransform;

    public int score {
        get { return _score; }
        set { 
            _score = value;
            scoreString = _score.ToString("N0");
        }
    }

    public string scoreString {
        get { return _scoreString; }
        set {
            _scoreString = value;
            GetComponent<Text>().text = _scoreString;
        }
    }

    private void Awake() {
        if (S == null) { S = this; }
        else { Debug.LogError("ERR: Scoreboard.Awake(): S is already set."); }

        canvasTransform = transform.parent;
    }

    public void FSCallback(FloatingScore fs) {
        score += fs.score;
    }

    // Instantiates a FloatingScore game object and returns its reference
    public FloatingScore CreateFloatingScore(int amt, List<Vector2> pts) {
        GameObject go = Instantiate<GameObject>(prefabFloatingScore);
        go.transform.SetParent(canvasTransform);
        
        FloatingScore fs = go.gameObject.GetComponent<FloatingScore>();
        fs.score = amt;
        fs.reportFinishTo = this.gameObject;
        fs.Init(pts);
        
        return fs;
    }
}
