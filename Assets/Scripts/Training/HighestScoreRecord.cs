using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HighestScoreRecord : MonoBehaviour
{
    public Text HighestScore_ISays, HighestScore_Duet;

    private void Awake()
    {
        if (AppManager.instance!=null)
        {
            if (AppManager.instance.currentUser!=null)
            {
                HighestScore_ISays.text = "Highest Score\n"+AppManager.instance.currentUser.highestScoreISays.ToString();
                HighestScore_Duet.text = "Highest Score\n"+AppManager.instance.currentUser.highestScoreDuet.ToString();
            }
        }
    }
}
