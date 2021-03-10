using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eScoreEvent { 
      draw, 
      mine, 
      mineGold, 
      gameWin, 
      gameLoss 
}

public class ScoreManager : MonoBehaviour
{
    private static ScoreManager S; // Private Singleton

    public static int   SCORE_FROM_PREV_ROUND = 0;
    public static int   HIGH_SCORE = 0;

    [Header("Set Dynamically")]
    public int          chain = 0;
    public int          scoreRun = 0;
    public int          score = 0;

    private void Awake() {
        if (S == null) { S = this; }
        else { Debug.LogError("ERR: ScoreManager.Awake(): S is already set."); }

        if (PlayerPrefs.HasKey("ProspectorHighScore")) {
            HIGH_SCORE = PlayerPrefs.GetInt("ProspectorHighScore");
        }

        score += SCORE_FROM_PREV_ROUND;
        SCORE_FROM_PREV_ROUND = 0;
    }

    public static void EVENT(eScoreEvent evt, bool isGold) {
        try {
            S.Event(evt, isGold);
        }
        catch(System.NullReferenceException nre) {
            Debug.LogError("ScoreManager:EVENT() called while S=null.\n" + nre);
        }
    }

    void Event(eScoreEvent evt, bool isGold) {
        // Draw/Win/Loss equal handling
        switch(evt) {
            case eScoreEvent.draw:
            case eScoreEvent.gameWin:
            case eScoreEvent.gameLoss:
                chain = 0;
                score += scoreRun;
                scoreRun = 0;
                
                break;
            case eScoreEvent.mine:
                chain++;

                Debug.Log("isGold: " + isGold);
                if (isGold) { scoreRun += (2 * chain); }
                else { scoreRun += chain; }
                Debug.Log("Current score: " + scoreRun);

                break;
        }

        // Draw/Win/Loss unique handling
        switch(evt) {
            case eScoreEvent.gameWin:
                SCORE_FROM_PREV_ROUND = score;
                print("You won this round! Round score: " + score);
                
            break;
            case eScoreEvent.gameLoss:
                if (HIGH_SCORE <= score) {
                    print("You got the high score! High score: " + score);
                    HIGH_SCORE = score;
                    PlayerPrefs.SetInt("ProspectorHighScore", score);
                }
                else {
                    print("Your final score for this game was: " + score);
                }
                
            break;
            default:
                print("score: " + score + " scoreRun: " + scoreRun + " chain: " + chain);

            break;
        }
    }

    public static int CHAIN { get { return S.chain; } }
    public static int SCORE { get { return S.score; } }
    public static int SCORE_RUN { get { return S.scoreRun; } }
}
