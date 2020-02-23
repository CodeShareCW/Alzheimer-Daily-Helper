using MongoDB.Bson;
using MongoDB.Driver;
using OpenCvSharp;
using OpenCvSharp.Demo;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class FaceRecordNameComparer: IComparer<FaceRecord>
{
    public FaceRecordNameComparer()
    { }
    public int Compare(FaceRecord f1, FaceRecord f2)
    {
        if (f1.targetName.CompareTo(f2.targetName)==1)
        {
            return 1;
        }
        else
        {
            return 0;
        }

    }
}


public class UploadImageController : WebCamScript
{
    public TextAsset faces;
    private CascadeClassifier cascadeface;

    public InputField targetName;
    public InputField targetRelationship;

    public Button uploadBtn;
    public Button fromDiskBtn;
    public Button captureBtn;
    public Button switchCamBtn;
    public Button removeBtn;

    public GameObject msg_panel;
    public Text msg_title;
    public Text msg_description;

    public GameObject sendingDataPanel;

    private FaceProcessorLive<WebCamTexture> processor;


    void checkOrAskCameraPermission()
    {
        if (AndroidRuntimePermissions.CheckPermission("android.permission.CAMERA") != AndroidRuntimePermissions.Permission.Granted)
        {
            AndroidRuntimePermissions.Permission result = AndroidRuntimePermissions.RequestPermission("android.permission.CAMERA");


            if (result == AndroidRuntimePermissions.Permission.ShouldAsk)
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


                //load to previous scene if rejected
                UnityEngine.SceneManagement.SceneManager.LoadScene(ActivityScene.Caregiver_MainScene);

            }
        }
    }

    protected override void Awake()
    {
        checkOrAskCameraPermission();

        base.Awake();

        cascadeface = new CascadeClassifier();
        FileStorage storageFaces = new FileStorage(faces.text, FileStorage.Mode.Read | FileStorage.Mode.Memory);
        if (!cascadeface.Read(storageFaces.GetFirstTopLevelNode()))
            throw new System.Exception("FaceProcessor.Initialize: Failed to load faces cascade classifier");

        processor = new FaceProcessorLive<WebCamTexture>();
        processor.Initialize(faces.text, null, null);
        //processor.Initialize(faces.text, eyes.text, shapes.bytes);

        // performance data - some tricks to make it work faster
        processor.Performance.Downscale = 256;          // processed image is pre-scaled down to N px by long side
        processor.Performance.SkipRate = 0;             // we actually process only each Nth frame (and every frame for skipRate = 0)

    }



    // Start is called before the first frame update
    void Start()
    {
        uploadBtn.onClick.AddListener(UploadBtn_Callback);
        fromDiskBtn.onClick.AddListener(FromDiskBtn_Callback);
        captureBtn.onClick.AddListener(CaptureBtn_Callback);
        switchCamBtn.onClick.AddListener(SwitchCamBtn_Callback);
        removeBtn.onClick.AddListener(RemoveBtn_Callback);
    }

    // Update is called once per frame
    void Update()
    {

        RenderingNormalScene();
    }

    public async Task MongoUpdateFaceRecord()
    {
        await AppManager.instance.userCollection.UpdateOneAsync(user => user._id == AppManager.instance.currentUser._id, Builders<UserAccount>.Update.Set(user => user.faceRecord, AppManager.instance.currentUser.faceRecord));
    }

    #region Callback
    public async void UploadBtn_Callback()
    {
        AppManager.instance.updatingMongoDB = true;
        UserAccount temp = AppManager.instance.currentUser;
        removeBtn.gameObject.SetActive(false);
        try
        {
            if (!webcamtexture.isPlaying && displaySurface != null)
            {
                if (targetName.text != "" && targetRelationship.text != "")
                {
                    if (targetName.text[0] == ' ' || targetRelationship.text[0] == ' ')
                    {
                        msg_panel.SetActive(true);
                        msg_title.text = "Upload Failed";
                        msg_description.text = "Please don't put \"SPACE\" at the first letter...";
                        return;
                    }


                    Texture2D tex = (Texture2D)displaySurface.GetComponent<RawImage>().texture;


                    if (AppManager.instance.currentUser != null)
                    {

                        FaceRecord fr = new FaceRecord(targetName.text, targetRelationship.text);
                        byte[] b = duplicateTexture(tex).EncodeToPNG();

                        fr.targetFace = new List<byte[]>();
                        fr.targetFace.Add(b);


                        if (AppManager.instance.currentUser.faceRecord != null)
                        {
                            bool isTargetFaceExist = false;
                            for (int i = 0; i < AppManager.instance.currentUser.faceRecord.Count; i++)
                            {
                                if (fr.targetName.ToUpper() == AppManager.instance.currentUser.faceRecord[i].targetName.ToUpper() && fr.targetRelationship.ToUpper() == AppManager.instance.currentUser.faceRecord[i].targetRelationship.ToUpper())
                                {
                                    //if exist we dont create a new face record but to insert the image into the existing one
                                    AppManager.instance.currentUser.faceRecord[i].targetFace.Add(b);


                                    //name the new face with the current total number of image of this person
                                    //save image to disk
                                    //NativeGallery.SaveImageToGallery(tex, "/Alzheimer Daily Helper/" + fr.targetName + "_" + fr.targetRelationship, "/Face " + AppManager.instance.currentUser.faceRecord[i].targetFace.Count + ".png");

                                    isTargetFaceExist = true;
                                    break;
                                }
                            }
                            if (isTargetFaceExist == false)
                            {
                                AppManager.instance.currentUser.faceRecord.Add(fr);
                                //NativeGallery.SaveImageToGallery(tex, "/Alzheimer Daily Helper/" + fr.targetName + "_" + fr.targetRelationship, "/Face 0.png");

                            }
                            AppManager.instance.currentUser.faceRecord.Sort(new FaceRecordNameComparer());
                        }
                        else
                        {

                            AppManager.instance.currentUser.faceRecord = new List<FaceRecord>();
                            AppManager.instance.currentUser.faceRecord.Add(fr);

                            //save image to disk
                            //NativeGallery.SaveImageToGallery(tex, "/Alzheimer Daily Helper/" + fr.targetName + "_" + fr.targetRelationship, "/Face 0.png");

                        }
                        try
                        {
                            int i = fr.targetFace.Count;
                            sendingDataPanel.SetActive(true);
                            InteractableInput(false);
                            await Task.Run(() => MongoUpdateFaceRecord());
                            StartCamera();


                            SSTools.ShowMessage("Upload Success", SSTools.Position.bottom, SSTools.Time.twoSecond);

                        }
                        catch (Exception ex)
                        {
                            AppManager.instance.currentUser.faceRecord = temp.faceRecord;
                            SSTools.ShowMessage("Some errors occurred", SSTools.Position.bottom, SSTools.Time.twoSecond);


                            removeBtn.gameObject.SetActive(false);
                            StartCamera();

                        }
                    }

                }

                else
                {
                    msg_panel.SetActive(true);
                    msg_title.text = "Upload Failed";
                    msg_description.text = "Please fill in name and relationship...";
                }


            }
            else
            {
                SSTools.ShowMessage("Please capture the image", SSTools.Position.bottom, SSTools.Time.twoSecond);

            }
            InteractableInput(true);
            sendingDataPanel.SetActive(false);
            AppManager.instance.updatingMongoDB = false;
        }
                    catch(Exception e)
        {
            SSTools.ShowMessage("Some errors occurred", SSTools.Position.bottom, SSTools.Time.twoSecond);


        }

    }



    public void FromDiskBtn_Callback()
    {
        //first check whether we gain the permission
        if (AndroidRuntimePermissions.CheckPermission("android.permission.READ_EXTERNAL_STORAGE") == AndroidRuntimePermissions.Permission.Granted)
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
            return;
        }

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
        else if (result== AndroidRuntimePermissions.Permission.Denied)
        {
            msg_panel.SetActive(true);
            msg_title.text = "Permission Storage";
            msg_description.text = "Please enable storage read/write permission in the setting if you wish to upload the image...";
            AndroidRuntimePermissions.OpenSettings();
            Debug.Log("Permission state: " + result);
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
                StopCamera();
                //assign texture to display surface
                displaySurface.GetComponent<RawImage>().texture= texture as Texture2D;
                removeBtn.gameObject.SetActive(true);
                // If a procedural texture is not destroyed manually, 
                // it will only be freed after a scene change
                //Destroy(texture, 5f);
            }
        }, "Select an image", "image/png;image/jpg");

        Debug.Log("Permission result: " + permission);
    }


    public void CaptureBtn_Callback()
    {
        Debug.Log("Capture Camera");
        displaySurface.GetComponent<Animator>().Play(AnimationName.CameraClickEffectAnimation, 0, -1f);
        StopCamera();

        //disable the  3 buttons
        fromDiskBtn.interactable = false;
        captureBtn.interactable = false;
        switchCamBtn.interactable = false;
        removeBtn.gameObject.SetActive(true);
    }
    public void SwitchCamBtn_Callback()
    {
        SwitchCamera();
    }

    public void RemoveBtn_Callback()
    {
        StartCamera();

        //enable 3 buttons
        fromDiskBtn.interactable = true;
        captureBtn.interactable = true;
        switchCamBtn.interactable = true;

        removeBtn.gameObject.SetActive(false);
    }
    #endregion

    protected override bool ProcessTexture(WebCamTexture input, ref Texture2D output)
    {
        if (webcamtexture.isPlaying)
        {
            // detect everything we're interested in
            processor.ProcessTexture(input, TextureParameters);

            // mark detected objects
            processor.MarkDetected();
           

            // processor.Image now holds data we'd like to visualize
            output = OpenCvSharp.Unity.MatToTexture(processor.Image, output);   // if output is valid texture it's buffer will be re-used, otherwise it will be re-created

            return true;
        }
        return false;
    }


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

    protected override void RunRecognizer() {/*no use in this script */}

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
