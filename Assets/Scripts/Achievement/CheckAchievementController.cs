using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;
using UnityEngine.UI;
using System;

public class CheckAchievementController : MonoBehaviour
{
    public GameObject sendingDataPanel;
    public GameObject[]awardPanel;
    public Button[] okBtn;


    private UserAccount temp_Backup;
    // Update is called once per frame
    void Awake()
    {
        temp_Backup = AppManager.instance.currentUser;

        okBtn[0].onClick.AddListener(delegate { OKBtn_Callback(0); });
        okBtn[1].onClick.AddListener(delegate { OKBtn_Callback(1); });
        okBtn[2].onClick.AddListener(delegate { OKBtn_Callback(2); });
        okBtn[3].onClick.AddListener(delegate { OKBtn_Callback(3); });
        okBtn[4].onClick.AddListener(delegate { OKBtn_Callback(4); });
        okBtn[5].onClick.AddListener(delegate { OKBtn_Callback(5); });
        okBtn[6].onClick.AddListener(delegate { OKBtn_Callback(6); });
        okBtn[7].onClick.AddListener(delegate { OKBtn_Callback(7); });


        if (AppManager.instance!=null)
        {
            if (AppManager.instance.currentUser!=null)
            {
                AppManager.instance.updatingMongoDB = true;
                sendingDataPanel.SetActive(true);
                InteractableInput(false);

                if (!AppManager.instance.currentUser.awardRecord.Contains(UserAward.FIRST_DAY_AWARD))
                {
                    awardPanel[0].SetActive(true);

                    AppManager.instance.currentUser.awardRecord.Add(UserAward.FIRST_DAY_AWARD);
                    AppManager.instance.currentUser.coins += 10;

                    WaitForMongoUpdate();
                }
                //7 days login award
                if (AppManager.instance.currentUser.loginDayCount>=7&&!AppManager.instance.currentUser.awardRecord.Contains(UserAward.CONT_7_DAYS_LOGIN_AWARD))
                {
                    //pop out the panel
                    awardPanel[1].SetActive(true);

                    AppManager.instance.currentUser.awardRecord.Add(UserAward.CONT_7_DAYS_LOGIN_AWARD);
                    AppManager.instance.currentUser.coins += 20;
                    WaitForMongoUpdate();
                }
                //30 day login award
                if (AppManager.instance.currentUser.loginDayCount>=30&& !AppManager.instance.currentUser.awardRecord.Contains(UserAward.CONT_30_DAYS_LOGIN_AWARD))
                {
                    //pop out the panel
                    awardPanel[2].SetActive(true);

                    AppManager.instance.currentUser.awardRecord.Add(UserAward.CONT_30_DAYS_LOGIN_AWARD);
                    AppManager.instance.currentUser.coins += 50;
                    WaitForMongoUpdate();
                }

                if (AppManager.instance.currentUser.flashCard!=null&&AppManager.instance.currentUser.flashCard.Count>=20&& !AppManager.instance.currentUser.awardRecord.Contains(UserAward.FLASHCARD_20_MAKER_AWARD))
                {
                    //send flashcard award
                    awardPanel[3].SetActive(true);

                    AppManager.instance.currentUser.awardRecord.Add(UserAward.FLASHCARD_20_MAKER_AWARD);
                    AppManager.instance.currentUser.coins += 50;
                    WaitForMongoUpdate();
                }

                if (AppManager.instance.currentUser.highestScoreDuet>=100&& !AppManager.instance.currentUser.awardRecord.Contains(UserAward.MEMORY_MASTER_AWARD))
                {
                    awardPanel[4].SetActive(true);

                    AppManager.instance.currentUser.awardRecord.Add(UserAward.MEMORY_MASTER_AWARD);
                    AppManager.instance.currentUser.coins += 100;
                    WaitForMongoUpdate();
                }

                if (AppManager.instance.currentUser.highestScoreISays>=100&& !AppManager.instance.currentUser.awardRecord.Contains(UserAward.MEMORY_MASTER_AWARD))
                {
                    awardPanel[4].SetActive(true);

                    AppManager.instance.currentUser.awardRecord.Add(UserAward.MEMORY_MASTER_AWARD);
                    AppManager.instance.currentUser.coins += 100;
                    WaitForMongoUpdate();

                }
                if (AppManager.instance.currentUser.hasNameOnBoardBefore == true && !AppManager.instance.currentUser.awardRecord.Contains(UserAward.NAME_ON_BOARD_AWARD))
                {
                    awardPanel[5].SetActive(true);

                    AppManager.instance.currentUser.awardRecord.Add(UserAward.NAME_ON_BOARD_AWARD);
                    AppManager.instance.currentUser.coins += 150;
                    WaitForMongoUpdate();
                }


                if (AppManager.instance.currentUser.highestScoreISays >= 500 && !AppManager.instance.currentUser.awardRecord.Contains(UserAward.MEMORY_KING_AWARD))
                {
                    awardPanel[6].SetActive(true);

                    AppManager.instance.currentUser.awardRecord.Add(UserAward.MEMORY_KING_AWARD);
                    AppManager.instance.currentUser.coins += 300;
                    WaitForMongoUpdate();

                }
                if (AppManager.instance.currentUser.highestScoreDuet >= 500 && !AppManager.instance.currentUser.awardRecord.Contains(UserAward.MEMORY_KING_AWARD))
                {
                    awardPanel[6].SetActive(true);

                    AppManager.instance.currentUser.awardRecord.Add(UserAward.MEMORY_KING_AWARD);

                    AppManager.instance.currentUser.coins += 300;
                    WaitForMongoUpdate();

                }


                if (AppManager.instance.currentUser.faceRecord != null && AppManager.instance.currentUser.faceRecord.Count >= 50 && !AppManager.instance.currentUser.awardRecord.Contains(UserAward.PEOPLE_50_COLLECTION_AWARD))
                {
                    awardPanel[7].SetActive(true);

                    AppManager.instance.currentUser.awardRecord.Add(UserAward.PEOPLE_50_COLLECTION_AWARD);

                    AppManager.instance.currentUser.coins += 500;
                    WaitForMongoUpdate();
                }
                sendingDataPanel.SetActive(false);
                InteractableInput(true);
                AppManager.instance.updatingMongoDB = false;
            }
        }
    }

    private async void WaitForMongoUpdate()
    {
        try
        {
            await Task.Run(() => MongoUpdateUserAwardListAndCoins());
        }
        catch(Exception e)
        {
            AppManager.instance.currentUser.coins = temp_Backup.coins;
            Debug.Log("Some error");
        }
    }

    private async Task MongoUpdateUserAwardListAndCoins()
    {
        //this 2 operation is done asynchronously, can save our time...
        await AppManager.instance.userCollection.UpdateOneAsync(user => user._id == AppManager.instance.currentUser._id, Builders<UserAccount>.Update.Set(user => user.awardRecord, AppManager.instance.currentUser.awardRecord));
        await AppManager.instance.userCollection.UpdateOneAsync(user => user._id == AppManager.instance.currentUser._id, Builders<UserAccount>.Update.Set(user => user.coins, AppManager.instance.currentUser.coins));

    }

    public void InteractableInput(bool inter)
    {
        foreach(var b in FindObjectsOfType<Button>())
        {
            b.interactable = inter;
        }
    }

    public void OKBtn_Callback(int i)
    {
        Debug.Log("Panel: "+i);
        awardPanel[i].SetActive(false);
    }
}
