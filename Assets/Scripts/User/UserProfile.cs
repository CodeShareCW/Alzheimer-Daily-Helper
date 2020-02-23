using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class UserProfile : MonoBehaviour
{
    public RawImage img;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(UserLogin());
    }
    IEnumerator UserLogin()
    {
        WWWForm form = new WWWForm();
    
        using (UnityWebRequest www = UnityWebRequest.Post(URIName.PROFILE_URI, form))
        {

            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log("Error:");
                Debug.Log(www.error);
            }
            else
            {

                byte[] bytes = System.Convert.FromBase64String(www.downloadHandler.text);
                Texture2D tex = new Texture2D(64, 64);
                tex.LoadImage(bytes);
         
                img.texture = tex as Texture2D;
                Debug.Log(www.downloadHandler.text);
                
            }
        }
    }



}
