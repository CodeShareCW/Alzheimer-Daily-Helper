using MongoDB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ISaysScoreComparer : IComparer<UserAccount>
{

    public ISaysScoreComparer() { }
    public int Compare(UserAccount u1, UserAccount u2)
    {
        return u1.highestScoreISays.CompareTo(u2.highestScoreISays);
    }
}


public class LeaderboardISaysController : MonoBehaviour
{
    public Text contentPanel_placeholder;
    public Text myNameAppearAt;

    public GameObject[] top5_Panel;
    public Text[] top5_names;
    public Text[] top5_scores;
    private List<UserAccount> allUsers;

    private List<UserAccount> peopleOnBoard;

    public GameObject sendingDataPanel;

    async void Start()
    {
        AppManager.instance.updatingMongoDB = true;
        FindObjectOfType<Button>().interactable = false;
        StartCoroutine(WaitUntilDataFetched());
        try
        {
            allUsers = await AppManager.instance.userCollection.Find(_ => true).ToListAsync();
            peopleOnBoard = new List<UserAccount>();

            for (int i = 0; i < allUsers.Count; i++)
            {
                if (allUsers[i].highestScoreISays != 0)
                {
                    peopleOnBoard.Add(allUsers[i]);
                }
            }
            if (peopleOnBoard.Count != 0)
            {
                peopleOnBoard.Sort(new ISaysScoreComparer());
                peopleOnBoard.Reverse();

                contentPanel_placeholder.gameObject.SetActive(false);
                if (peopleOnBoard.Count < 5)
                    for (int i = 0; i < peopleOnBoard.Count; i++)
                    {
                        top5_Panel[i].SetActive(true);
                        top5_names[i].text = peopleOnBoard[i].username;
                        top5_scores[i].text = peopleOnBoard[i].highestScoreISays.ToString();

                        if (peopleOnBoard[i]._id == AppManager.instance.currentUser._id)
                        {
                            try
                            {
                                if (!AppManager.instance.currentUser.hasNameOnBoardBefore)
                                {
                                    AppManager.instance.updatingMongoDB = true;
                                    sendingDataPanel.SetActive(true);
                                    AppManager.instance.currentUser.hasNameOnBoardBefore = true;
                                    await Task.Run(() => TaskUpdateHasNameAppearedOnBoard());
                                }
                                top5_Panel[i].GetComponent<Image>().color = Color.red;
                                top5_Panel[i].transform.GetChild(3).gameObject.SetActive(true);
                                myNameAppearAt.text = "";
                            }
                            catch (Exception e)
                            {
                                AppManager.instance.currentUser.hasNameOnBoardBefore = false;

                            }

                            AppManager.instance.updatingMongoDB = false;
                            sendingDataPanel.SetActive(false);

                        }
                    }
                else
                    for (int i = 5; i < peopleOnBoard.Count; i++)
                    {
                        if (peopleOnBoard[i]._id == AppManager.instance.currentUser._id)
                        {
                            myNameAppearAt.text = "You are at " + (i + 1) + "th";
                        }
                    }
            }
            else
            {
                contentPanel_placeholder.gameObject.SetActive(true);
            }
        }
        catch (Exception e)
        {
            SSTools.ShowMessage("Some errors occurred", SSTools.Position.bottom, SSTools.Time.twoSecond);
        }
        AppManager.instance.updatingMongoDB = false;
        FindObjectOfType<Button>().interactable = true;
        sendingDataPanel.SetActive(false);
    }


    private async Task TaskUpdateHasNameAppearedOnBoard()
    {
        await AppManager.instance.userCollection.UpdateOneAsync(user => user._id == AppManager.instance.currentUser._id, Builders<UserAccount>.Update.Set(user => user.hasNameOnBoardBefore, AppManager.instance.currentUser.hasNameOnBoardBefore));
    }
    IEnumerator WaitUntilDataFetched()
    {
        sendingDataPanel.SetActive(true);
        yield return new WaitUntil(() => allUsers != null);
    }
}
