using MongoDB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class FillMyBrainController : MonoBehaviour
{
    public Image BrainFiller;
    public GameObject BrainFiller_PS;
    public Text statusText;
    public Text promptText;
    public Button AddBtn;
    public ParticleSystem Add_PS_Effect;
    private float totalFillAmount;
    public GameObject sendingDataPanel;
    private float amountFill;
    private void Awake()
    {
        totalFillAmount = 10000;
        amountFill = 10f;
        //if (AppManager.instance!=null&&AppManager.instance.currentUser!=null)
        {
            //show current status
            statusText.text = AppManager.instance.currentUser.brainStatus + "/"+totalFillAmount.ToString();
            fillBrainImage(AppManager.instance.currentUser.brainStatus);

            if (AppManager.instance.currentUser.brainStatus==totalFillAmount)
            {
                BrainFiller_PS.SetActive(true);
                AddBtn.gameObject.SetActive(false);
                promptText.text="Congratulation! You have filled up your brain";
            }

        }
    }

    public async void AddBtn_Callback()
    {
        if (AppManager.instance.currentUser.coins < (int)amountFill)
        {
            SSTools.ShowMessage("No enough coins", SSTools.Position.bottom, SSTools.Time.twoSecond);
            return;
        }

        AppManager.instance.currentUser.coins -= (int)amountFill;
        AppManager.instance.currentUser.brainStatus += amountFill;
        fillBrainImage(AppManager.instance.currentUser.brainStatus);

        if (AppManager.instance.currentUser.brainStatus == totalFillAmount)
        {
            BrainFiller_PS.SetActive(true);
            AddBtn.gameObject.SetActive(false);
            promptText.text = "Congratulation! You have filled up your brain";
        }

        AppManager.instance.updatingMongoDB = true;
        sendingDataPanel.SetActive(true);
        AddBtn.interactable = false;
        try
        {
            await Task.Run(() => TaskUpdateBrainStatus());
            Add_PS_Effect.Play();
        }
        catch(Exception e)
        {
            //resume coin if mongo db error
            AppManager.instance.currentUser.coins += (int)amountFill;
            AppManager.instance.currentUser.brainStatus -= amountFill;
            SSTools.ShowMessage("Some errors occurred", SSTools.Position.bottom, SSTools.Time.twoSecond);
        }
        statusText.text = AppManager.instance.currentUser.brainStatus + "/" + totalFillAmount.ToString();
        AddBtn.interactable = true;
        sendingDataPanel.SetActive(false);
        AppManager.instance.updatingMongoDB = false;
    }

    private void fillBrainImage(float amt)
    {
        BrainFiller.fillAmount = amt / totalFillAmount;
    }


    private async Task TaskUpdateBrainStatus()
    {
        await AppManager.instance.userCollection.UpdateOneAsync(user => user._id == AppManager.instance.currentUser._id, Builders<UserAccount>.Update.Set(user => user.coins, AppManager.instance.currentUser.coins));
        await AppManager.instance.userCollection.UpdateOneAsync(user => user._id == AppManager.instance.currentUser._id, Builders<UserAccount>.Update.Set(user => user.brainStatus, AppManager.instance.currentUser.brainStatus));
    }

}
