using MongoDB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ShowFlashCardController : MonoBehaviour
{
    public GameObject contentPanel;
    public Text contentPanelPlaceholder;

    public RawImage flashCardImage;
    public Text flashcardImage_Placeholder;
    public InputField flashCardTitle;
    public InputField flashCardDescription;
    public Text createdOnText;

    public GameObject pageIndicator;
    public Text pageIndicatorText;
    private int currPageIndex;
    private int totalPage;

    public Button editBtn, okBtn, cancelBtn, delBtn;
    public Button addImgBtn;

    public GameObject confirmDeletePanel, confirmClearAllPanel;
    public Button confirmDelBtn, confirmClearAllBtn, cancelDelBtn, cancelClearAllBtn;
    public Button removeImageBtn;

    public GameObject previousBtn;
    public GameObject nextBtn;

    private bool hasRemoveBtnPressed = false;

    public GameObject msg_panel;
    public Text msg_title;
    public Text msg_description;

    public GameObject sendingDataPanel;

    private void Awake()
    {
        currPageIndex = 0;
    }

    public async Task MongoUpdateFlashcardRecord()
    {
        await AppManager.instance.userCollection.UpdateOneAsync(user => user._id == AppManager.instance.currentUser._id, Builders<UserAccount>.Update.Set(user => user.flashCard, AppManager.instance.currentUser.flashCard));
    }

    private void Start()
    {
        
        if (AppManager.instance != null)
        {
            if (AppManager.instance.currentUser!=null)
            {
                if (AppManager.instance.currentUser.flashCard != null && AppManager.instance.currentUser.flashCard.Count!=0)
                {
                    contentPanel.SetActive(true);
                    contentPanelPlaceholder.gameObject.SetActive(false);
                    pageIndicator.SetActive(true);
                    pageIndicatorText.text = (currPageIndex + 1).ToString() + "/" + AppManager.instance.currentUser.flashCard.Count;

                    if(AppManager.instance.currentUser.flashCard[currPageIndex].flashcard_image!=null)
                    {
                        Texture2D tex = new Texture2D(128, 128, TextureFormat.ARGB32, false);
                        tex.LoadImage(AppManager.instance.currentUser.flashCard[currPageIndex].flashcard_image);
                        flashCardImage.texture = tex as Texture2D;
                        removeImageBtn.gameObject.SetActive(true);
                        removeImageBtn.interactable = false;
                        flashCardDescription.interactable = false;
                        flashCardTitle.interactable = false;
                        
                        flashcardImage_Placeholder.gameObject.SetActive(false);
                    }
                    else
                    {
                        flashcardImage_Placeholder.gameObject.SetActive(true);
                        addImgBtn.gameObject.SetActive(true);
                        addImgBtn.interactable = false;
                        removeImageBtn.gameObject.SetActive(false);
                    }

                    flashCardTitle.text = AppManager.instance.currentUser.flashCard[currPageIndex].flashcard_title;
                    flashCardDescription.text = AppManager.instance.currentUser.flashCard[currPageIndex].flashcard_description;
                    createdOnText.text = AppManager.instance.currentUser.flashCard[currPageIndex].createdOn.ToString("dddd, dd MMMM yyyy hh:mm tt");

                    return;
                }
            }
        }
        contentPanel.SetActive(false);
        contentPanelPlaceholder.gameObject.SetActive(true);
        pageIndicator.SetActive(false);

    }

    public void nextFlashcard()
    {
        currPageIndex = (currPageIndex + 1) % AppManager.instance.currentUser.flashCard.Count;

        okBtn.gameObject.SetActive(false);
        editBtn.gameObject.SetActive(true);
        cancelBtn.gameObject.SetActive(false);
        delBtn.gameObject.SetActive(true);
        removeImageBtn.interactable = false;
        flashCardDescription.interactable = false;
        flashCardTitle.interactable = false;
        addImgBtn.interactable = false;


        flashCardTitle.text = AppManager.instance.currentUser.flashCard[currPageIndex].flashcard_title;
        flashCardDescription.text = AppManager.instance.currentUser.flashCard[currPageIndex].flashcard_description;
        if(AppManager.instance.currentUser.flashCard[currPageIndex].flashcard_image != null)
        {
            flashcardImage_Placeholder.gameObject.SetActive(false);
            Texture2D tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            tex.LoadImage(AppManager.instance.currentUser.flashCard[currPageIndex].flashcard_image);
            flashCardImage.texture = tex as Texture2D;
        }
        else
        {
            flashCardImage.texture = null;
            flashcardImage_Placeholder.gameObject.SetActive(true);
        }

        createdOnText.text = AppManager.instance.currentUser.flashCard[currPageIndex].createdOn.ToString("dddd, dd MMMM yyyy hh:mm tt");
        pageIndicatorText.text = (currPageIndex + 1).ToString() + "/" + AppManager.instance.currentUser.flashCard.Count;
    }

    public void previousFlashcard()
    {

        okBtn.gameObject.SetActive(false);
        editBtn.gameObject.SetActive(true);
        cancelBtn.gameObject.SetActive(false);
        delBtn.gameObject.SetActive(true);

        currPageIndex--;
        if (currPageIndex==-1)
            currPageIndex = AppManager.instance.currentUser.flashCard.Count-1;

        removeImageBtn.interactable = false;
        flashCardDescription.interactable = false;
        flashCardTitle.interactable = false;
        addImgBtn.interactable = false;


        flashCardTitle.text = AppManager.instance.currentUser.flashCard[currPageIndex].flashcard_title;
        flashCardDescription.text = AppManager.instance.currentUser.flashCard[currPageIndex].flashcard_description;

        if (AppManager.instance.currentUser.flashCard[currPageIndex].flashcard_image != null)
        {
            flashcardImage_Placeholder.gameObject.SetActive(false);
            Texture2D tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            tex.LoadImage(AppManager.instance.currentUser.flashCard[currPageIndex].flashcard_image);
            flashCardImage.texture = tex as Texture2D;
        }
        else
        {
            flashCardImage.texture = null;
            flashcardImage_Placeholder.gameObject.SetActive(true);
        }

        createdOnText.text = AppManager.instance.currentUser.flashCard[currPageIndex].createdOn.ToString("dddd, dd MMMM yyyy hh:mm tt");
        pageIndicatorText.text = (currPageIndex + 1).ToString() + "/" + AppManager.instance.currentUser.flashCard.Count;

    }


    public void editBtn_Callback()
    {
        flashCardDescription.interactable = true;
        flashCardTitle.interactable = true;
        removeImageBtn.interactable = true;
        addImgBtn.interactable = true;

        editBtn.gameObject.SetActive(false);
        okBtn.gameObject.SetActive(true);
        delBtn.gameObject.SetActive(false);
        cancelBtn.gameObject.SetActive(true);

        okBtn.interactable = true;
        cancelBtn.interactable = true;
    }


    public void deleteBtn_Callback()
    {
        confirmDeletePanel.SetActive(true);
        foreach (var b in FindObjectsOfType<Button>())
        {
            if (b != confirmDelBtn && b != cancelDelBtn)
            {
                b.interactable = false;
            }
        }
        foreach (var i in FindObjectsOfType<InputField>())
        {
            i.interactable = false;
        }
    }

    private void Update()
    {
        if (flashCardImage.texture == null)
        {
            removeImageBtn.gameObject.SetActive(false);
            addImgBtn.gameObject.SetActive(true);
            flashcardImage_Placeholder.gameObject.SetActive(true);
        }
        else
        {
            removeImageBtn.gameObject.SetActive(true);
            addImgBtn.gameObject.SetActive(false);

            flashcardImage_Placeholder.gameObject.SetActive(false);
        }
    }

    public void RemoveBtn_Callback()
    {
        hasRemoveBtnPressed = true;
        flashCardImage.texture = null;
        

        removeImageBtn.gameObject.SetActive(false);
        addImgBtn.gameObject.SetActive(true);
    }

    public void AddImageBtn_Callback()
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


    public async void OKBtn_Callback()
    {
        if (flashCardTitle.text==""&&flashCardDescription.text=="")
        {
            msg_panel.SetActive(true);
            msg_title.text = "Edit failed";
            msg_description.text = "Please fill in both field...";
        }

        else
        {
            AppManager.instance.currentUser.flashCard[currPageIndex].flashcard_title = flashCardTitle.text;
            AppManager.instance.currentUser.flashCard[currPageIndex].flashcard_description = flashCardDescription.text;

            if (flashCardImage.texture!=null&&hasRemoveBtnPressed)
            {
                Texture2D tex = flashCardImage.texture as Texture2D;
                AppManager.instance.currentUser.flashCard[currPageIndex].flashcard_image = duplicateTexture(tex).EncodeToPNG();
            }
            else
            {
                AppManager.instance.currentUser.flashCard[currPageIndex].flashcard_image=null;
            }
            UserAccount temp = AppManager.instance.currentUser;
            try
            {
                AppManager.instance.updatingMongoDB = true;
                sendingDataPanel.SetActive(true);
                InteractableInput(false);
                await Task.Run(() => MongoUpdateFlashcardRecord());

                flashCardDescription.interactable = false;
                flashCardTitle.interactable = false;

                editBtn.gameObject.SetActive(true);
                okBtn.gameObject.SetActive(false);
                delBtn.gameObject.SetActive(true);
                cancelBtn.gameObject.SetActive(false);

                SSTools.ShowMessage("Edit Success", SSTools.Position.bottom, SSTools.Time.twoSecond);
            }
            catch (Exception e)
            {
                AppManager.instance.currentUser.flashCard = temp.flashCard;
                SSTools.ShowMessage("Some errors occurred", SSTools.Position.bottom, SSTools.Time.twoSecond);

            }


            InteractableInput(true);
            addImgBtn.interactable = false;
            removeImageBtn.interactable = false;
            AppManager.instance.updatingMongoDB = false;
            sendingDataPanel.SetActive(false);
        }

    }

    public void cancelEditBtn_Callback()
    {
        removeImageBtn.interactable = false;
        flashCardDescription.interactable = false;
        flashCardTitle.interactable = false;
        addImgBtn.interactable = false;

        flashCardTitle.text = AppManager.instance.currentUser.flashCard[currPageIndex].flashcard_title;
        flashCardDescription.text = AppManager.instance.currentUser.flashCard[currPageIndex].flashcard_description;
        
        if (AppManager.instance.currentUser.flashCard[currPageIndex].flashcard_image!=null)
        {
            Texture2D tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            tex.LoadImage(AppManager.instance.currentUser.flashCard[currPageIndex].flashcard_image);
            flashCardImage.texture = tex as Texture2D;
        }


        flashCardDescription.interactable = false;
        flashCardTitle.interactable = false;
        editBtn.gameObject.SetActive(true);
        okBtn.gameObject.SetActive(false);
        delBtn.gameObject.SetActive(true);
        cancelBtn.gameObject.SetActive(false);
        removeImageBtn.interactable = false;
    }

    public void clearAllBtn_Callback()
    {
        confirmClearAllPanel.SetActive(true);
        foreach (var b in FindObjectsOfType<Button>())
        {
            if (b!=confirmClearAllBtn&&b!=cancelClearAllBtn)
            {
                b.interactable = false;
            }
        }
        foreach(var i in FindObjectsOfType<InputField>())
        {
            i.interactable = false;
        }

    }

    public async void confirmDeleteBtn_Callback()
    {
        confirmDeletePanel.SetActive(false);

        UserAccount temp = AppManager.instance.currentUser;

        AppManager.instance.currentUser.flashCard.RemoveAt(currPageIndex);

        try
        {
            AppManager.instance.updatingMongoDB = true;
            sendingDataPanel.SetActive(true);
            InteractableInput(false);
            await Task.Run(() => MongoUpdateFlashcardRecord());

            if (AppManager.instance.currentUser.flashCard.Count==0)
            {
                contentPanel.SetActive(false);
                pageIndicator.SetActive(false);
                contentPanelPlaceholder.gameObject.SetActive(true);
                addImgBtn.interactable = false;
                removeImageBtn.interactable = false;
                flashCardDescription.interactable = false;
                flashCardTitle.interactable = false;
            }
            else
            {
                nextFlashcard();
            }

            SSTools.ShowMessage("Delete Success", SSTools.Position.bottom, SSTools.Time.twoSecond);
        }
        catch (Exception e)
        {
            AppManager.instance.currentUser.flashCard = temp.flashCard;
            SSTools.ShowMessage("Some errors occurred", SSTools.Position.bottom, SSTools.Time.twoSecond);

        }

        InteractableInput(true);
        removeImageBtn.interactable = false;
        addImgBtn.interactable = false;
        AppManager.instance.updatingMongoDB = false;
        sendingDataPanel.SetActive(false);


    }

    public async void confirmClearAllBtn_Callback()
    {
        confirmClearAllPanel.SetActive(false);

        UserAccount temp = AppManager.instance.currentUser;
        if (AppManager.instance.currentUser.flashCard.Count!=0&& AppManager.instance.currentUser.flashCard!=null)
        {
            AppManager.instance.currentUser.flashCard = null;
        }
        try
        {
            AppManager.instance.updatingMongoDB = true;
            sendingDataPanel.SetActive(true);
            InteractableInput(false);
            await Task.Run(() => MongoUpdateFlashcardRecord());

            contentPanel.SetActive(false);
            pageIndicator.SetActive(false);
            contentPanelPlaceholder.gameObject.SetActive(true);

            SSTools.ShowMessage("Clear Up Success", SSTools.Position.bottom, SSTools.Time.twoSecond);

        }
        catch (Exception e)
        {
            AppManager.instance.currentUser.flashCard = temp.flashCard;
            SSTools.ShowMessage("Clear Up Failed", SSTools.Position.bottom, SSTools.Time.twoSecond);

        }

        InteractableInput(true);
        AppManager.instance.updatingMongoDB = false;
        sendingDataPanel.SetActive(false);

    }

    public void cancelDeleteBtn_Callback()
    {
        confirmDeletePanel.SetActive(false);
        foreach (var b in FindObjectsOfType<Button>())
        {
            if (b != confirmDelBtn && b != cancelDelBtn)
            {
                b.interactable = true;
            }
        }
        foreach (var i in FindObjectsOfType<InputField>())
        {
            i.interactable = false;
        }
    }


    public void cancelClearAllBtn_Callback()
    {
        confirmClearAllPanel.SetActive(false);
        foreach (var b in FindObjectsOfType<Button>())
        {
            if (b != confirmClearAllBtn && b != cancelClearAllBtn)
            {
                b.interactable = true;
            }
        }
        foreach (var i in FindObjectsOfType<InputField>())
        {
            i.interactable = false;
        }
    }

    private void InteractableInput(bool inter)
    {
        foreach(var b in FindObjectsOfType<Button>())
        {
            b.interactable = inter;
        }
        foreach(var i in FindObjectsOfType<InputField>())
        {
            if (i!=flashCardDescription&&i!=flashCardTitle)
                i.interactable = inter;
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
                flashCardImage.texture = texture as Texture2D;

                removeImageBtn.gameObject.SetActive(true);
                addImgBtn.gameObject.SetActive(false);
                flashcardImage_Placeholder.gameObject.SetActive(false);
                // If a procedural texture is not destroyed manually, 
                // it will only be freed after a scene change
                //Destroy(texture, 5f);
            }
        }, "Select an image", "image/png;image/jpg");

        Debug.Log("Permission result: " + permission);
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
