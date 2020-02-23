using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EnterLoaderController : MonoBehaviour
{
    public GameObject loadingScreen;
    public Image Loadingbar;
    public Text loadingPercent;


    private void Start()
    {
        LoadEntryScene();
    }

    public void LoadEntryScene()
    {
        string sceneName = LauchingInterfaceScene.EntryScene;

        //navigate to a new scene
        StartCoroutine(LoadAsynchronously(sceneName));
    }

    IEnumerator LoadAsynchronously(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        loadingScreen.SetActive(true);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / .9f);

            Loadingbar.fillAmount = progress;
            loadingPercent.text = ((int)progress * 100).ToString() + "%";
            Debug.Log(operation.progress);
            yield return null;
        }
    }
}
