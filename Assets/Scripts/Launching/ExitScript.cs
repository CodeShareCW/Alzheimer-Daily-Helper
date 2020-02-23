using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitScript : MonoBehaviour
{
    private const string exitConfirmTextAnim = "ExitConfirmText";
    int QuitPressedCount = 0;
    float QuitPressedStart = 0;
    float QuitPressedEnd = 0;
    void Update()
    {
        //if the duration of pressed back button greater than 3, reset
        if (QuitPressedEnd - QuitPressedStart > 3)
        {
            QuitPressedCount = 0;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitPressedCount++;
            if (QuitPressedCount == 2)
            {
                UnityEngine.Application.Quit();
                Debug.Log("Quit App");
            }
            else
            {
                GameObject.Find(exitConfirmTextAnim).GetComponent<Animator>().Play(AnimationName.ExitConfirmTextAnimation, -1, 0f);
                QuitPressedStart = Time.time;
            }
        }

        QuitPressedEnd = Time.time;

    }

}
