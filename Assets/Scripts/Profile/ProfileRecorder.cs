using MongoDB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ProfileRecorder : MonoBehaviour
{
    public RawImage profilePicture;
    public Text image_placeholderText;

    public Button UploadFromDiskBtn;
    public Button removeBtn;
    public Button Okbtn;


    public Text nameInputField, phoneNoInputField;

    public GameObject msg_panel;
    public Text msg_title;
    public Text msg_description;

    public GameObject sendingDataPanel;
    public Text createdOnText;


    private void Awake()
    {
        if (AppManager.instance.currentUser != null)
        {
            nameInputField.text = "Name:\n"+AppManager.instance.currentUser.username;
            phoneNoInputField.text = "Phone Number/FB Email:\n"+AppManager.instance.currentUser.phoneNumber;
            createdOnText.text = "Created On:\n" + AppManager.instance.currentUser.CreateOn.ToString("dddd, dd MMMM yyyy hh:mm tt");

            if (AppManager.instance.currentUser.profile_picture != null)
            {
                byte[] picturebyte = AppManager.instance.currentUser.profile_picture;

                Texture2D tex = new Texture2D(128, 128, TextureFormat.ARGB32, false);
                tex.LoadImage(AppManager.instance.currentUser.profile_picture);
                tex.Apply();

                profilePicture.texture = tex as Texture2D;
                removeBtn.gameObject.SetActive(true);
                image_placeholderText.gameObject.SetActive(false);

            }
            else
            {
                image_placeholderText.gameObject.SetActive(true);
            }
        }
    }

    public void UploadFromDiskBtn_Callback()
    {
        if (AndroidRuntimePermissions.CheckPermission("android.permission.READ_EXTERNAL_STORAGE") != AndroidRuntimePermissions.Permission.Granted)
        {
            AndroidRuntimePermissions.Permission result = AndroidRuntimePermissions.RequestPermission("android.permission.READ_EXTERNAL_STORAGE");
            if (result == AndroidRuntimePermissions.Permission.Granted)
            {
                if (NativeGallery.IsMediaPickerBusy())
                    return;

                PickImage(512);

                Debug.Log("We have permission to access external storage!");
            }
            else if (result == AndroidRuntimePermissions.Permission.ShouldAsk)
            {

                msg_panel.SetActive(true);
                msg_title.text = "Result";
                msg_description.text = "Permission state: " + result;
                Debug.Log("Permission state: " + result);


            }
            else if (result == AndroidRuntimePermissions.Permission.Denied)
            {
                AndroidRuntimePermissions.OpenSettings();
                msg_panel.SetActive(true);
                msg_title.text = "Result";
                msg_description.text = "Permission state: " + result;
                Debug.Log("Permission state: " + result);
            }
        }
        else
        {
            if (NativeGallery.IsMediaPickerBusy())
                return;

            PickImage(512);
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
                profilePicture.texture = texture as Texture2D;
                removeBtn.gameObject.SetActive(true);
                UploadFromDiskBtn.gameObject.SetActive(true);
                Okbtn.gameObject.SetActive(true);
                image_placeholderText.gameObject.SetActive(false);
                // If a procedural texture is not destroyed manually, 
                // it will only be freed after a scene change
                //Destroy(texture, 5f);
            }
        }, "Select an image", "image/png;image/jpg");

        Debug.Log("Permission result: " + permission);
    }


    public void RemoveBtn_Callback()
    {
        profilePicture.texture = null;
        image_placeholderText.gameObject.SetActive(true);
        removeBtn.gameObject.SetActive(false);
        Okbtn.gameObject.SetActive(false);
        UploadFromDiskBtn.gameObject.SetActive(true);
    }

    public async void OkBtn_Callback()
    {
        AppManager.instance.updatingMongoDB = true;
        UserAccount temp = AppManager.instance.currentUser;
        bool isDeletePicture = false;
        //save into mongodb
        if (AppManager.instance.currentUser != null)
        {
            if (profilePicture.texture == null)
            {
                isDeletePicture = true;
                AppManager.instance.currentUser.profile_picture = null;
            }
            else
            {
                isDeletePicture = false;
                Texture2D tex = (Texture2D)profilePicture.texture;

                AppManager.instance.currentUser.profile_picture = duplicateTexture(tex).EncodeToPNG();
            }
            try {
                sendingDataPanel.SetActive(true);
                await Task.Run(() => MongoUpdateUserProfilePicture());
                Okbtn.gameObject.SetActive(false);
                msg_panel.SetActive(true);
                if (!isDeletePicture)
                {
                    msg_title.text = "Upload Success";
                    msg_description.text = "Successfully update the profile picture...";
                }
                else
                {
                    msg_title.text = "Remove Picture Success";
                    msg_description.text = "Successfully remove the profile picture...";
                }
            } catch(Exception e)
            {
                AppManager.instance.currentUser.profile_picture = temp.profile_picture;
                if(profilePicture.texture==null)
                {
                    UploadFromDiskBtn.gameObject.SetActive(true);
                    removeBtn.gameObject.SetActive(false);
                    image_placeholderText.gameObject.SetActive(true);
                }
                else
                {
                    removeBtn.gameObject.SetActive(true);
                    UploadFromDiskBtn.gameObject.SetActive(false);

                    image_placeholderText.gameObject.SetActive(false);
                }

                msg_panel.SetActive(true);
                msg_title.text = "Upload Failed";
                msg_description.text = "Error: " + e.ToString();
                Debug.Log("Error: " + e);
            }
          

        }


        sendingDataPanel.SetActive(false);

        AppManager.instance.updatingMongoDB = false;
    }



    private async Task MongoUpdateUserProfilePicture()
    {
        await AppManager.instance.userCollection.UpdateOneAsync(user => user._id == AppManager.instance.currentUser._id, Builders<UserAccount>.Update.Set(user => user.profile_picture, AppManager.instance.currentUser.profile_picture));

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
