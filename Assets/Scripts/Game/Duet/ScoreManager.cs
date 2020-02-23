using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ScoreManager : MonoBehaviour
{
    public Text currentScoreText;
    public Text highestScoreText;

    public static int score;
    public static Text scoreText;
    // Start is called before the first frame update
    void Awake()
    {
        score = 0;
        scoreText = GetComponent<Text>();
    }

    public static void IncrementScore()
    {
        score++;
        scoreText.text = "Score: "+score.ToString();
    }

    public static void IncrementStarScore()
    {
        score+=5;
        scoreText.text = "Score: " + score.ToString();
    }

    private void Update()
    {
        currentScoreText.text = "Current "+scoreText.text;
        if (AppManager.instance.currentScene==ActivityScene.Training_ISayScene)
            highestScoreText.text = "Highest Score: " + AppManager.instance.currentUser.highestScoreISays;
        else if (AppManager.instance.currentScene == ActivityScene.Training_DuetScene)
            highestScoreText.text = "Highest Score: " + AppManager.instance.currentUser.highestScoreDuet;
    }


}
