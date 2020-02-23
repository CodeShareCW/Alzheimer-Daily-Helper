using MongoDB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    public GameObject sendingDataPanel;
    private bool ISays_STATE;

    public Text stateText;
    public Text stateNoteText;
    public Text timerText;
    public Text scoreText;

    public GameObject resultPanel;
    public Text resultText;
    public Text resultDescription;
   

    private float min_time = 10f;
    private float max_time = 15f;
    private float timer;



    public Button[] arc;
    private Color[] arc_color;

    private List<int> flickCount;

    public Text buttonText;

    bool isRestart = false;

    public void Flick(Button btn)
    {
        if (!ISays_STATE) return;
        Color temp = btn.GetComponent<Image>().color;
        temp.a = 1;
        btn.GetComponent<Image>().color = temp;
    }

    public void BackToOriginalColor()
    {
        for (int i=0; i<arc.Length; i++)
        {
            arc[i].GetComponent<Image>().color = arc_color[i];
        }
    }

    int btnIndexClicked;
    public void AddListenerToButton(Button[] btn)
    {
        foreach(var b in btn)
        {
            btnIndexClicked=(btnIndexClicked+1)%arc.Length;
            b.onClick.AddListener(delegate { btnEvent(b); });
        }
    }

    private void btnEvent(Button b)
    {
        if (ISays_STATE)
        {
            return;
        }

        Debug.Log(b.name);
        //tell the current button users had pressed

        checkFlickListRecord(b);
    }

    public void checkFlickListRecord(Button btn)
    {
        Debug.Log(arc[flickCount[0]].name);
        if (btn == arc[flickCount[0]])
        {
            ScoreManager.IncrementScore();
            //basically just remove the the list at index 0 when the right button is clicked
            flickCount.RemoveAt(0);
            if (flickCount.Count==0)
            {
                Debug.Log(flickCount.Count);
                resultPanel.SetActive(true);
                resultText.text = "Correct";
                resultDescription.text = "Congratulation. You're so brilliant.";
                buttonText.text = "Continue";

                UpdateHighestScore();

            }
            
        }
        else
        {
            resultPanel.SetActive(true);
            resultText.text = "Wrong";
            resultDescription.text = "Don't give up! A journey of a thousand miles begins with a single step.";
            buttonText.text = "Restart";

            UpdateHighestScore();

        }

    }




    private void Awake()
    {
        flickCount = new List<int>();

        //record original color
        arc_color = new Color[arc.Length];
        for(int i=0; i<arc.Length; i++)
        {
            arc_color[i] = arc[i].GetComponent<Image>().color;
        }
        AddListenerToButton(arc);
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ISays_Moment(1));
    }

    IEnumerator ISays_Moment(int duration)
    {
        //DisableAllButtonInteratable();
        ISays_STATE = true;

        int randomFlick = UnityEngine.Random.Range(3,8);
        for (int i = 0; i < randomFlick; i++)
        {
            int randNum = UnityEngine.Random.Range(0, arc.Length);
            Flick(arc[randNum]);
            flickCount.Add(randNum);
            Debug.Log(flickCount.Count);
            yield return new WaitForSeconds(duration);
            BackToOriginalColor();
            yield return new WaitForSeconds(duration);

        }

        //reverse the list
        flickCount.Reverse();
        int kk = 0;
        foreach (var f in flickCount)
        {    foreach (var b in arc)
            {
                if (arc[f] == b)
                    Debug.Log(b.name + ": " + kk);

            }
            kk++;
        }

        //set timer
        if (flickCount.Count < 5) timer = min_time;
        else timer = UnityEngine.Random.Range(min_time, max_time);

        ISays_STATE = false;

    }



    // Update is called once per frame
    void Update()
    {
        if (!StartTraining.isTrainStart) return;


        if(ISays_STATE)
        {
            timerText.gameObject.SetActive(false);
            stateText.text = "I Say...";
            stateNoteText.text = "Try to remember as much as you could!";
            foreach (var b in arc)
            {
                ColorBlock col = b.colors;
                col.pressedColor = new Color(1, 1, 1, 1);
                b.colors = col;
            }
        }

        if (!ISays_STATE)
        {   
            timerText.gameObject.SetActive(true);
            stateText.text = "Your turn...";
            stateNoteText.text = "Repeat Backward!";
            foreach (var b in arc)
            {
                ColorBlock col = b.colors;
                col.pressedColor = new Color(1, 1, 1, 0.5f);
                b.colors = col;
            }

            if (timer < 0)
            {
                Debug.Log(max_time);
                Debug.Log("Game Over");
                resultPanel.SetActive(true);
                resultText.text = "Timed Out";
                resultDescription.text = "It is okay to take your time for regenerate your brain.";
                buttonText.text = "Restart";

                UpdateHighestScore();

                return;
            }
            else
            {
                if (resultText.text != "Correct" && resultText.text != "Wrong" && resultText.text != "Game Over")
                {
                    timer -= 0.017f;
                    timerText.text = ((int)timer).ToString() + "s";
                }
            }
        }

        if (resultText.text=="Correct")
        {
            if (isRestart)
            {
                isRestart = false;
                resultPanel.SetActive(false);
                //repeat
                StartCoroutine(ISays_Moment(1));
            }
        }

    }

    public void Continue_Callback()
    {
        if (!AppManager.instance.updatingMongoDB)
        {
            if (resultText.text != "Correct")
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            else
            {
                isRestart = true;
            }
        }
    }


    private async void UpdateHighestScore()
    {

        AppManager.instance.updatingMongoDB = true;
        if (ScoreManager.score > AppManager.instance.currentUser.highestScoreISays)
        {
            sendingDataPanel.SetActive(true);
            UserAccount temp = AppManager.instance.currentUser;

            AppManager.instance.currentUser.highestScoreISays = ScoreManager.score;

            try
            {
                sendingDataPanel.SetActive(true);
                await Task.Run(() => MongoUpdateHighestScore());
            }
            catch (Exception e)
            {
                AppManager.instance.currentUser = temp;
            }
            sendingDataPanel.SetActive(false);
        }


        AppManager.instance.updatingMongoDB = false;
    }

    private async Task MongoUpdateHighestScore()
    {
        await AppManager.instance.userCollection.UpdateOneAsync(user => user._id == AppManager.instance.currentUser._id, Builders<UserAccount>.Update.Set(user => user.highestScoreISays, AppManager.instance.currentUser.highestScoreISays));

    }
}
