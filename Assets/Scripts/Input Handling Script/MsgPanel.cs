using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MsgPanel : MonoBehaviour
{
    public bool isButtonPressed;


    public void ClosePanel()
    {
        isButtonPressed = true;
        gameObject.SetActive(false);
        EnableInput();
    }
    


    public void EnableInput()
    {
        Button[] btns = GameObject.FindObjectsOfType<Button>();
        foreach (var btn in btns)
        {
            btn.interactable = true;
        }
    }


}
