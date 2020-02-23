using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PageController: MonoBehaviour
{
    public GameObject loadingScreen;
    public Image Loadingbar;
    public Text loadingPercent;

    public void QuitApp()
    {
        UnityEngine.Debug.Log("Quit App");
        UnityEngine.Application.Quit();
    }


    public void LoadEntryScene()
    {
        string sceneName = LauchingInterfaceScene.EntryScene;

        //navigate to a new scene
        StartCoroutine(LoadAsynchronously(sceneName));
    }

    public void LoadLoginScene()
    {
        string sceneName = LauchingInterfaceScene.LoginScene;
        //navigate to a new scene
        StartCoroutine(LoadAsynchronously(sceneName));
    }

    public void LoadRegisterScene()
    {
        string sceneName = LauchingInterfaceScene.RegisterScene;
        //navigate to a new scene
        StartCoroutine(LoadAsynchronously(sceneName));
    }
    public void LoadHomeScene()
    {
        string sceneName = ActivityScene.HomeScene;

        //navigate to a new scene
        StartCoroutine(LoadAsynchronously(sceneName));
    }
    public void LoadFillMyBrainScene()
    {
        string sceneName = ActivityScene.FillMyBrainScene;

        //navigate to a new scene
        StartCoroutine(LoadAsynchronously(sceneName));
    }

    public void LoadWhoAreYouScene()
    {

        if (AppManager.instance != null)
            if (AppManager.instance.currentUser != null)
            {
                if (AppManager.instance.currentUser.faceRecord == null || AppManager.instance.currentUser.faceRecord.Count == 0)
                {
                    StartCoroutine(DisplayErrorMessageForNoFaceRecord());
                    return;
                }
                else
                {
                    string sceneName = ActivityScene.WhoAreYouScene;

                    //navigate to a new scene
                    //UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
                    StartCoroutine(LoadAsynchronously(sceneName));
                }
            }



    }
    IEnumerator DisplayErrorMessageForNoFaceRecord()
    {
        SSTools.ShowMessage("Access Denied! No Face Record", SSTools.Position.top, SSTools.Time.twoSecond);
        yield return new WaitForSeconds(2);
        SSTools.ShowMessage("Please go to Caregiver", SSTools.Position.top, SSTools.Time.twoSecond);
        yield return new WaitForSeconds(2);
        SSTools.ShowMessage("and select \"Upload Image\"", SSTools.Position.top, SSTools.Time.twoSecond);
        yield return new WaitForSeconds(2);
        SSTools.ShowMessage("to upload the face data", SSTools.Position.top, SSTools.Time.twoSecond);

    }

    public void LoadHelloTodayScene()
    {
        string sceneName = ActivityScene.HelloTodayScene;

        //navigate to a new scene
        StartCoroutine(LoadAsynchronously(sceneName));
    }


    public void LoadAddFlashcardScene()
    {
        string sceneName = ActivityScene.Add_FlashcardScene;

        //navigate to a new scene
        StartCoroutine(LoadAsynchronously(sceneName));
    }

    public void LoadShowFlashcardScene()
    {
        string sceneName = ActivityScene.Show_FlashcardScene;

        //navigate to a new scene
        StartCoroutine(LoadAsynchronously(sceneName));
    }

    public void LoadTrainingScene()
    {
        string sceneName = ActivityScene.TrainingScene;


        StartCoroutine(LoadAsynchronously(sceneName));
        //navigate to a new scene
        //UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    public void LoadTrainingDuetScene()
    {
        string sceneName = ActivityScene.Training_DuetScene;

        //navigate to a new scene
        StartCoroutine(LoadAsynchronously(sceneName));
    }

    public void LoadTrainingISayScene()
    {
        string sceneName = ActivityScene.Training_ISayScene;

        //navigate to a new scene
        StartCoroutine(LoadAsynchronously(sceneName));
    }

    public void LoadCaregiverMainScene()
    {
        string sceneName = ActivityScene.Caregiver_MainScene;

        //navigate to a new scene
        StartCoroutine(LoadAsynchronously(sceneName));
    }
    public void LoadCaregiverUploadImageScene()
    {
        string sceneName = ActivityScene.Caregiver_UploadImage_Scene;

        //navigate to a new scene
        StartCoroutine(LoadAsynchronously(sceneName));
    }

    public void LoadCaregiverShowReminderScene()
    {
        string sceneName = ActivityScene.Caregiver_ShowReminder_Scene;

        //navigate to a new scene
        StartCoroutine(LoadAsynchronously(sceneName));
    }

    public void LoadCaregiverAddReminderScene()
    {
        string sceneName = ActivityScene.Caregiver_AddReminder_Scene;

        //navigate to a new scene
        StartCoroutine(LoadAsynchronously(sceneName));
    }

    public void LoadCaregiverShowImageUploadedScene()
    {
        FindObjectOfType<WebCamScript>().StopCamera();
        string sceneName = ActivityScene.Caregiver_ShowImageUploaded_Scene;

        //navigate to a new scene
        StartCoroutine(LoadAsynchronously(sceneName));
    }
    public void LoadProfileScene()
    {
        string sceneName = ActivityScene.ProfileScene;

        //navigate to a new scene
        StartCoroutine(LoadAsynchronously(sceneName));
    }
    public void LoadAchievementScene()
    {
        string sceneName = ActivityScene.AchievementScene;

        //navigate to a new scene
        StartCoroutine(LoadAsynchronously(sceneName));
    }


    public void LoadLeaderboardDuetScene()
    {
        string sceneName = ActivityScene.LeaderboardScene_Duet;

        //navigate to a new scene
        StartCoroutine(LoadAsynchronously(sceneName));
    }
    public void LoadLeaderboardISayScene()
    {
        string sceneName = ActivityScene.LeaderboardScene_ISay;

        //navigate to a new scene
        StartCoroutine(LoadAsynchronously(sceneName));
    }




    IEnumerator LoadAsynchronously (string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        loadingScreen.SetActive(true);

        while (!operation.isDone&&!AppManager.instance.updatingMongoDB)
        {
            float progress = Mathf.Clamp01(operation.progress / .9f);

            Loadingbar.fillAmount = progress;
            loadingPercent.text = ((int)progress*100).ToString()+"%";
            Debug.Log(operation.progress);
            yield return null;
        }
    }

}
