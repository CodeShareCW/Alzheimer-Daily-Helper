using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartTraining : MonoBehaviour
{
    public GameObject TrainingInfoPanel;
    public GameObject playerController;
    public static bool isTrainStart=false;
    int count = 4;
    public GameObject startCountDownText;

    private void Awake()
    {
        if (!isTrainStart)
            TrainingInfoPanel.SetActive(true);
    }
    public void TrainStart_Callback()
    {
        TrainingInfoPanel.SetActive(false);
        startCountDownText.SetActive(true);
        
        StartCoroutine(countdown());

    }

    IEnumerator countdown()
    {
        if (count != 0)
        {
            startCountDownText.GetComponent<UnityEngine.UI.Text>().text = (count-1).ToString();
            count--;
        }
        else
        {
            startCountDownText.SetActive(false);
            playerController.SetActive(true);
            isTrainStart = true;
        }

        yield return new WaitForSeconds(count);
        StartCoroutine(countdown());

    }

    



}
