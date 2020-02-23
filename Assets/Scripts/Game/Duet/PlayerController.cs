using MongoDB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{

    public Text congratText;
    private int congratIndex;

    public GameObject TrainingInfoPanel;
    public GameObject obsSpawner;
    public GameObject starSpawner;
    public GameObject gameOverPanel;
    public GameObject player;
    public GameObject BG_particleSystem;

    public int rotateScale = 2;

    public GameObject sendingDataPanel;

    // Start is called before the first frame update
    void Start()
    {
        congratIndex = UnityEngine.Random.Range(0, 3);
        Player.isGameOver = false;
    }

    // Update is called once per frame
    void Update()
    {

        if (!StartTraining.isTrainStart)
        {
            Debug.Log("Havenot start training");
            Camera.main.GetComponent<AudioSource>().enabled = false;
            player.SetActive(false);
            BG_particleSystem.SetActive(false);
            return;
        }
        else
        {

            Debug.Log("Start training");
            Camera.main.GetComponent<AudioSource>().enabled = true;
            player.SetActive(true);
            BG_particleSystem.SetActive(true);
            obsSpawner.SetActive(true);
            starSpawner.SetActive(true);
        }
        if (Player.isGameOver)
        {
            
            UpdateHighestScore();


            switch (congratIndex)
            {
                case 0:
                    congratText.text = "It was a good try!";
                    break;
                case 1:
                    congratText.text = "You did your best!";
                    break;
                case 2:
                    congratText.text = "You could be better!";
                    break;

            }
            return;
        }


        if (Input.GetMouseButton(0))
        {
            Control();
        }
    }

    private void Control()
    {
        Vector3 mousePos = (Input.mousePosition);
        Vector3 temp = player.transform.localEulerAngles;
        if (mousePos.x<Screen.width/2)
        {
            temp.z+=rotateScale;
        }
        else
        {
            temp.z-=rotateScale;
        }
        
        player.transform.localEulerAngles = temp;
    }


    public void RestartGame()
    {
        AppManager.instance.updatingMongoDB = true;
        Player.isGameOver = false;
        gameOverPanel.SetActive(false);
        UpdateHighestScore();
        Player.isGameOver = false;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    private async void UpdateHighestScore()
    {
        AppManager.instance.updatingMongoDB = true;
        UserAccount temp = AppManager.instance.currentUser;
        if (ScoreManager.score > AppManager.instance.currentUser.highestScoreDuet)
        {
            AppManager.instance.currentUser.highestScoreDuet = ScoreManager.score;
            try
            {
                await Task.Run(() => MongoUpdateHighestScore());
            }
            catch (Exception e)
            {
                AppManager.instance.currentUser = temp;
                Debug.Log("Some connection problem");

            }
            sendingDataPanel.SetActive(false);
        }

        AppManager.instance.updatingMongoDB = false;
    }



    public void Quit()
    {
        Application.Quit();
    }

    private async Task MongoUpdateHighestScore()
    {
        await AppManager.instance.userCollection.UpdateOneAsync(user => user._id == AppManager.instance.currentUser._id, Builders<UserAccount>.Update.Set(user => user.highestScoreDuet, AppManager.instance.currentUser.highestScoreDuet));

    }
}
