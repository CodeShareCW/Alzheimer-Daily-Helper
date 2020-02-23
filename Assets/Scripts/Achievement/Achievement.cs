using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Achievement : MonoBehaviour
{
    public Text totalReceivedAwardText;
    public GameObject[] receivedImage;
    private void Awake()
    {
        if (AppManager.instance!=null)
        {
            if (AppManager.instance.currentUser!=null)
            {
                totalReceivedAwardText.text = "Total Received Award: " + AppManager.instance.currentUser.awardRecord.Count;
                if (AppManager.instance.currentUser.awardRecord.Contains(UserAward.FIRST_DAY_AWARD))
                {
                    receivedImage[0].SetActive(true);
                }
                if (AppManager.instance.currentUser.awardRecord.Contains(UserAward.CONT_7_DAYS_LOGIN_AWARD))
                {
                    receivedImage[1].SetActive(true);
                }
                if (AppManager.instance.currentUser.awardRecord.Contains(UserAward.CONT_30_DAYS_LOGIN_AWARD))
                {
                    receivedImage[2].SetActive(true);
                }
                if (AppManager.instance.currentUser.awardRecord.Contains(UserAward.FLASHCARD_20_MAKER_AWARD))
                {
                    receivedImage[3].SetActive(true);
                }
                if (AppManager.instance.currentUser.awardRecord.Contains(UserAward.MEMORY_MASTER_AWARD))
                {
                    receivedImage[4].SetActive(true);
                }
                if (AppManager.instance.currentUser.awardRecord.Contains(UserAward.NAME_ON_BOARD_AWARD))
                {
                    receivedImage[5].SetActive(true);
                }
                if (AppManager.instance.currentUser.awardRecord.Contains(UserAward.MEMORY_KING_AWARD))
                {
                    receivedImage[6].SetActive(true);
                }
                if (AppManager.instance.currentUser.awardRecord.Contains(UserAward.PEOPLE_50_COLLECTION_AWARD))
                {
                    receivedImage[7].SetActive(true);
                }
            }
        }
    }
}
