using MongoDB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class AddFlashcardController : MonoBehaviour
{
    public RawImage flashcardImage;
    public Text flashcardImage_Placeholder;
    public InputField flashcardTitle_inputfield;
    public InputField flashcardDescription_inputfield;
    public Button fromDiskBtn, removeImageBtn, addBtn;

    public GameObject msg_panel;
    public Text msg_title;
    public Text msg_description;

    public GameObject sendingDataPanel;





    public async void AddBtn_Callback()
    {
        if (AppManager.instance != null)
        {
            if (AppManager.instance.currentUser != null)
            {
                if (flashcardTitle_inputfield.text == "" && flashcardDescription_inputfield.text == "")
                {
                    msg_panel.SetActive(true);
                    msg_title.text = "Add Flashcard Fail";
                    msg_description.text = "Please fill in both title and note of flashcard...";
                    return;
                }
                try {
                    //backup to restore app manager data when error occur
                    UserAccount temp = AppManager.instance.currentUser;
                    Flashcard fc = new Flashcard(flashcardTitle_inputfield.text, flashcardDescription_inputfield.text);

                    //set time
                    fc.createdOn = DateTime.SpecifyKind(System.DateTime.Now, DateTimeKind.Utc);

                    if (flashcardImage.texture != null)
                    {
                        Texture2D tex = flashcardImage.texture as Texture2D;
                        fc.flashcard_image=duplicateTexture(tex).EncodeToPNG();
                    }

                    if (AppManager.instance.currentUser.flashCard == null)
                    {
                        AppManager.instance.currentUser.flashCard = new List<Flashcard>();
                        AppManager.instance.currentUser.flashCard.Add(fc);
                    }
                    else
                    {
                        AppManager.instance.currentUser.flashCard.Add(fc);
                    }
                    try
                    {
                        AppManager.instance.updatingMongoDB = true;
                        sendingDataPanel.SetActive(true);
                        InteractableInput(false);
                        await Task.Run(() => MongoUpdateFlashcardRecord());
                        msg_panel.SetActive(true);
                        msg_title.text = "Add Flashcard Success";
                        msg_description.text = "Just add a new flashcard to your record...";
                    }
                    catch (Exception e)
                    {
                        AppManager.instance.currentUser.flashCard = temp.flashCard;
                        msg_panel.SetActive(true);
                        msg_title.text = "Add Flashcard Failed";
                        msg_description.text = "Error: " + e.ToString();
                    }

                }
                catch (Exception ex)
                {
                    msg_panel.SetActive(true);
                    msg_title.text = "Error";
                    msg_description.text = "Error: " + ex.ToString();
                }
            }

        }
        InteractableInput(true);
        
        AppManager.instance.updatingMongoDB = false;
        sendingDataPanel.SetActive(false);
    }

    public async Task MongoUpdateFlashcardRecord()
    {
        await AppManager.instance.userCollection.UpdateOneAsync(user => user._id == AppManager.instance.currentUser._id, Builders<UserAccount>.Update.Set(user => user.flashCard, AppManager.instance.currentUser.flashCard));
    }


    #region Callback


    public void FromDiskBtn_Callback()
    {
        //first check whether we gain the permission
        if (AndroidRuntimePermissions.CheckPermission("android.permission.READ_EXTERNAL_STORAGE") != AndroidRuntimePermissions.Permission.Granted)
        {
            AndroidRuntimePermissions.Permission result = AndroidRuntimePermissions.RequestPermission("android.permission.READ_EXTERNAL_STORAGE");


            if (result == AndroidRuntimePermissions.Permission.Granted)
            {
                if (NativeGallery.IsMediaPickerBusy())
                    return;

                try
                {
                    PickImage(512);
                }
                catch (Exception e)
                {
                    msg_panel.SetActive(true);
                    msg_title.text = "Result";
                    msg_description.text = "Error: " + e.ToString();
                }

                Debug.Log("We have permission to access external storage!");
            }

            else if (result == AndroidRuntimePermissions.Permission.ShouldAsk)
            {
                msg_panel.SetActive(true);
                msg_title.text = "Permission Storage";
                msg_description.text = "Please allow the permission for accessing your gallery...";
                Debug.Log("Permission state: " + result);


            }
            else if (result == AndroidRuntimePermissions.Permission.Denied)
            {
                msg_panel.SetActive(true);
                msg_title.text = "Permission Storage";
                msg_description.text = "Please enable storage read/write permission in the setting if you wish to upload the image...";
                AndroidRuntimePermissions.OpenSettings();
                Debug.Log("Permission state: " + result);
            }
        }
        else
        {
            if (NativeGallery.IsMediaPickerBusy())
                return;

            try
            {
                PickImage(512);
            }
            catch (Exception e)
            {
                msg_panel.SetActive(true);
                msg_title.text = "Result";
                msg_description.text = "Error: " + e.ToString();
            }

        }
    }


    private void PickImage(int maxSize)
    {
        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) =>
        {
            Debug.Log("Image path: " + path);
            if (path != null)
            {
                // Create Texture from selected image
                Texture2D texture = NativeGallery.LoadImageAtPath(path, maxSize);
                if (texture == null)
                {
                    msg_panel.SetActive(true);
                    msg_title.text = "Result";
                    msg_description.text = "texture is null";
                    Debug.Log("Couldn't load texture from " + path);

                    return;
                }


                /*
                // Assign texture to a temporary quad and destroy it after 5 seconds
                GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quad.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 2.5f;
                quad.transform.forward = Camera.main.transform.forward;
                quad.transform.localScale = new Vector3(1f, texture.height / (float)texture.width, 1f);

                Material material = quad.GetComponent<Renderer>().material;
                if (!material.shader.isSupported) // happens when Standard shader is not included in the build
                    material.shader = Shader.Find("Legacy Shaders/Diffuse");

                material.mainTexture = texture;

                Destroy(quad, 5f);*/

                //assign texture to display surface
                flashcardImage.texture = texture as Texture2D;
                removeImageBtn.gameObject.SetActive(true);
                fromDiskBtn.gameObject.SetActive(false);
                flashcardImage_Placeholder.gameObject.SetActive(false);
                // If a procedural texture is not destroyed manually, 
                // it will only be freed after a scene change
                //Destroy(texture, 5f);
            }
        }, "Select an image", "image/png;image/jpg");

        Debug.Log("Permission result: " + permission);
    }

    public void RemoveBtn_Callback()
    {
        flashcardImage.texture = null;
        fromDiskBtn.gameObject.SetActive(true);
        removeImageBtn.gameObject.SetActive(false);

    }



    #endregion


    public void InteractableInput(bool inter)
    {
        foreach (var b in FindObjectsOfType<Button>())
        {
            b.interactable = inter;
        }
        foreach (var i in FindObjectsOfType<InputField>())
        {
            i.interactable = inter;
        }
    }

    Texture2D duplicateTexture(Texture2D source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
                    source.width,
                    source.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);

        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readableText = new Texture2D(source.width, source.height);
        readableText.ReadPixels(new UnityEngine.Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableText;
    }

}
