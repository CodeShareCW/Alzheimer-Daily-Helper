using MongoDB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class CaregiverShowFaceListScript : MonoBehaviour
{
    public GameObject ContentPanel;
    public Text ContentPanelPlaceholder;
    public GameObject PageIndicatorPanel;

    public InputField faceName, faceRelationship;

    public Button editBtn, okBtn, cancelEditBtn, delBtn, delImgBtn;

    public GameObject confirmDeletePanel;
    public Button confirmDelBtn, cancelDelBtn;
    public Text confirmDeleteDescription;

    public Button imagePreviousBtn, imageNextBtn, previousPageBtn, nextPageBtn;
    public Text pageIndicatorText;

    private int currentPageIndex, currentImageIndex;
    private int totalPage;

    public RawImage display;

    public GameObject msg_panel;
    public Text msg_title;
    public Text msg_description;

    public GameObject sendingDataPanel;

    private void Awake()
    {
        currentPageIndex = 0;
        currentImageIndex = 0;
        totalPage = 0;
        if (AppManager.instance.currentUser != null)
        {
            if (AppManager.instance.currentUser.faceRecord.Count != 0)
            {
                faceName.text = AppManager.instance.currentUser.faceRecord[currentPageIndex].targetName;
                faceRelationship.text = AppManager.instance.currentUser.faceRecord[currentPageIndex].targetRelationship;

                faceName.interactable = false;
                faceRelationship.interactable = false;

                editBtn.gameObject.SetActive(true);
                delBtn.gameObject.SetActive(true);
                okBtn.gameObject.SetActive(false);
                cancelEditBtn.gameObject.SetActive(false);
                delImgBtn.gameObject.SetActive(true);

                ContentPanel.SetActive(true);
                ContentPanelPlaceholder.gameObject.SetActive(false);

                PageIndicatorPanel.SetActive(true);

                
                totalPage = AppManager.instance.currentUser.faceRecord.Count;
                if (AppManager.instance.currentUser.faceRecord.Count==1)  //mean we only get 1 target
                {
                    nextPageBtn.gameObject.SetActive(false);
                    previousPageBtn.gameObject.SetActive(false);
                }
                else
                {
                    nextPageBtn.gameObject.SetActive(true);
                    previousPageBtn.gameObject.SetActive(true);
                    imageNextBtn.gameObject.SetActive(true);
                    imagePreviousBtn.gameObject.SetActive(true);
                }

                if (AppManager.instance.currentUser.faceRecord[currentPageIndex].targetFace.Count == 1)
                {
                    imageNextBtn.gameObject.SetActive(false);
                    imagePreviousBtn.gameObject.SetActive(false);
                }
                else
                {
                    imageNextBtn.gameObject.SetActive(true);
                    imagePreviousBtn.gameObject.SetActive(true);
                }


                

                pageIndicatorText.text = (currentPageIndex + 1).ToString() + "/" + totalPage.ToString();
                DisplayCurrentFaceImage(currentPageIndex, currentImageIndex);
            }
            else
            {
                PageIndicatorPanel.SetActive(false);
                ContentPanel.SetActive(false);
                ContentPanelPlaceholder.gameObject.SetActive(true);
                
            }
        }
    }

    void DisplayCurrentFaceImage(int currPgIndex, int currImgIndex)
    {
        //show first image
        var currentFace = AppManager.instance.currentUser.faceRecord[currPgIndex].targetFace[currImgIndex];


        var tex = new Texture2D(128, 128, TextureFormat.ARGB32, false);

        tex.LoadImage(currentFace); //.no longer.this will auto-resize the texture dimensions.
        tex.Apply();

        display.texture = tex as Texture2D;
    }

    public void EditBtn_Callback()
    {
        editBtn.gameObject.SetActive(false);
        okBtn.gameObject.SetActive(true);
        cancelEditBtn.gameObject.SetActive(true);
        delBtn.gameObject.SetActive(false);


        faceName.interactable = true;
        faceRelationship.interactable = true;
    }
    public async void OKBtn_Callback()
    {
        AppManager.instance.updatingMongoDB = true;
        UserAccount temp = AppManager.instance.currentUser;
        if (AppManager.instance.currentUser!=null)
        {
            if (AppManager.instance.currentUser.faceRecord!=null)
            {
                if (faceName.text != "" && faceRelationship.text != "")
                {
                    if (faceName.text.ToUpper() != AppManager.instance.currentUser.faceRecord[currentPageIndex].targetName.ToUpper() || faceRelationship.text.ToUpper() != AppManager.instance.currentUser.faceRecord[currentPageIndex].targetRelationship.ToUpper())
                    {
                        try
                        {
                            AppManager.instance.currentUser.faceRecord[currentPageIndex].targetName = faceName.text;
                            AppManager.instance.currentUser.faceRecord[currentPageIndex].targetRelationship = faceRelationship.text;

                            //update in mongo
                            sendingDataPanel.SetActive(true);
                            await Task.Run(() => MongoUpdateTargetImageNameAndRelationship());


                            SSTools.ShowMessage("Edit Success", SSTools.Position.bottom, SSTools.Time.twoSecond);

                        }
                        catch (Exception e)
                        {
                            AppManager.instance.currentUser.faceRecord = temp.faceRecord;
                            SSTools.ShowMessage("Some error occurred", SSTools.Position.bottom, SSTools.Time.twoSecond);

                        }

                    }
                    else
                    {


                        SSTools.ShowMessage("No change", SSTools.Position.bottom, SSTools.Time.twoSecond);

                    }
                }
                else
                {
                    msg_panel.SetActive(true);
                    msg_title.text = "Edit Failed";
                    msg_description.text = "Please fill in both name and relationship...";


                }
            }
            else
            {
                SSTools.ShowMessage("No face record", SSTools.Position.bottom, SSTools.Time.twoSecond);

            }
        }
        okBtn.gameObject.SetActive(false);
        cancelEditBtn.gameObject.SetActive(false);
        editBtn.gameObject.SetActive(true);
        delBtn.gameObject.SetActive(true);
        faceName.interactable = false;
        faceRelationship.interactable = false;

        sendingDataPanel.SetActive(false);
        AppManager.instance.updatingMongoDB = false;
    }

    private async Task MongoUpdateTargetImageNameAndRelationship()
    {
        await AppManager.instance.userCollection.UpdateOneAsync(user => user._id == AppManager.instance.currentUser._id, Builders<UserAccount>.Update.Set(user => user.faceRecord[currentPageIndex].targetName, AppManager.instance.currentUser.faceRecord[currentPageIndex].targetName));
        await AppManager.instance.userCollection.UpdateOneAsync(user => user._id == AppManager.instance.currentUser._id, Builders<UserAccount>.Update.Set(user => user.faceRecord[currentPageIndex].targetRelationship, AppManager.instance.currentUser.faceRecord[currentPageIndex].targetRelationship));
    }

    bool isDeletePerson = true;

    public void DelBtn_Callback()
    {
        if (AppManager.instance.currentUser != null)
        {
            if (AppManager.instance.currentUser.faceRecord!=null)
            {
                isDeletePerson = true;
                //prompt confirm msg
                confirmDeletePanel.SetActive(true);
                confirmDeleteDescription.text = "Are you sure to delete all the images and relationship of this person?";
                DisableInput();
                confirmDelBtn.interactable = true;
                cancelDelBtn.interactable = true;

            }
        }
    }
    public void DeleleImageBtn_Callback()
    {
        if (AppManager.instance.currentUser != null)
        {
            if (AppManager.instance.currentUser.faceRecord != null)
            {
                isDeletePerson = false;
                //prompt confirm msg
                confirmDeletePanel.SetActive(true);
                confirmDeleteDescription.text = "Are you sure to delete this image?";
                DisableInput();
                confirmDelBtn.interactable = true;
                cancelDelBtn.interactable = true;

            }
        }
    }


    #region Interactable Input
    public void EnableInput()
    {
        InputField[] infields = GameObject.FindObjectsOfType<InputField>();
        foreach (var infield in infields)
        {
            if (infield!=faceName&&infield!=faceRelationship)
            infield.interactable = true;
        }
        Button[] btns = GameObject.FindObjectsOfType<Button>();
        foreach (var btn in btns)
        {
            btn.interactable = true;
        }
    }

    public void DisableInput()
    {
        InputField[] infields = GameObject.FindObjectsOfType<InputField>();
        foreach (var infield in infields)
        {
            infield.interactable = false;
        }
        Button[] btns = GameObject.FindObjectsOfType<Button>();
        foreach (var btn in btns)
        {
            btn.interactable = false;
        }
    }

    #endregion



    public async void ConfirmDeleteBtn_Callback()
    {
        AppManager.instance.updatingMongoDB = true;
        //backup
        UserAccount temp = AppManager.instance.currentUser;
        if (isDeletePerson == true)
        {
            AppManager.instance.currentUser.faceRecord.RemoveAt(currentPageIndex);
            totalPage = AppManager.instance.currentUser.faceRecord.Count;
          
            if (AppManager.instance.currentUser.faceRecord == null || AppManager.instance.currentUser.faceRecord.Count == 0)
            {
                ContentPanel.SetActive(false);
                PageIndicatorPanel.SetActive(false);
                ContentPanelPlaceholder.gameObject.SetActive(true);
            }
            else
            {
                NextPageBtn_Callback();
            }
        }
        else
        {
            AppManager.instance.currentUser.faceRecord[currentPageIndex].targetFace.RemoveAt(currentImageIndex);
            if (AppManager.instance.currentUser.faceRecord[currentPageIndex].targetFace.Count==0)
            {
                //we have to delete this person if null image of the person is resulted
                AppManager.instance.currentUser.faceRecord.RemoveAt(currentPageIndex);
                totalPage = AppManager.instance.currentUser.faceRecord.Count;
                
                if (AppManager.instance.currentUser.faceRecord==null|| AppManager.instance.currentUser.faceRecord.Count==0)
                {
                    ContentPanel.SetActive(false);
                    PageIndicatorPanel.SetActive(false);
                    ContentPanelPlaceholder.gameObject.SetActive(true);
                }
                else
                {
                    NextPageBtn_Callback();

                }
            }
            else
            {
                NextImageBtn_Callback();
            }
        }

        try
        {
            sendingDataPanel.SetActive(true);
            DisableInput();
            await Task.Run(() => MongoUpdateFaceRecord());

            SSTools.ShowMessage("Delete Success", SSTools.Position.bottom, SSTools.Time.twoSecond);

            //reload
            // UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
        catch (Exception e)
        {
            AppManager.instance.currentUser.faceRecord = temp.faceRecord;
            SSTools.ShowMessage("Some errors occurred", SSTools.Position.bottom, SSTools.Time.twoSecond);

        }


        confirmDeletePanel.SetActive(false);

        okBtn.gameObject.SetActive(false);
        cancelEditBtn.gameObject.SetActive(false);
        editBtn.gameObject.SetActive(true);
        delBtn.gameObject.SetActive(true);

        faceName.interactable = false;
        faceRelationship.interactable = false;



        EnableInput();
        sendingDataPanel.SetActive(false);
        AppManager.instance.updatingMongoDB = false;
    }
    private async Task MongoUpdateFaceRecord()
    {
        await AppManager.instance.userCollection.UpdateOneAsync(user => user._id == AppManager.instance.currentUser._id, Builders<UserAccount>.Update.Set(user => user.faceRecord, AppManager.instance.currentUser.faceRecord));
    }

    public void CancelDeleteBtn_Callback()
    {
        confirmDeletePanel.SetActive(false);
        EnableInput();
    }

    public void CancelEditBtn_Callback()
    {
        editBtn.gameObject.SetActive(true);
        delBtn.gameObject.SetActive(true);
        okBtn.gameObject.SetActive(false);
        cancelEditBtn.gameObject.SetActive(false);

        //unchange it if cancel
        faceName.text = AppManager.instance.currentUser.faceRecord[currentPageIndex].targetName;
        faceRelationship.text = AppManager.instance.currentUser.faceRecord[currentPageIndex].targetRelationship;

        faceName.interactable = false;
        faceRelationship.interactable = false;
    }

    public void PreviousImageBtn_Callback()
    {
        currentImageIndex--; 

        if (currentImageIndex == -1)
            currentImageIndex= AppManager.instance.currentUser.faceRecord[currentPageIndex].targetFace.Count-1;
        DisplayCurrentFaceImage(currentPageIndex, currentImageIndex);
    }


    public void NextImageBtn_Callback()
    {

        currentImageIndex = (currentImageIndex + 1) % AppManager.instance.currentUser.faceRecord[currentPageIndex].targetFace.Count;
        DisplayCurrentFaceImage(currentPageIndex, currentImageIndex);
    }


    public void PreviousPageBtn_Callback()
    {
        faceName.interactable = false;
        faceRelationship.interactable = false;

        editBtn.gameObject.SetActive(true);
        delBtn.gameObject.SetActive(true);
        okBtn.gameObject.SetActive(false);
        cancelEditBtn.gameObject.SetActive(false);

            currentPageIndex--;
        if (currentPageIndex == -1)
            currentPageIndex = AppManager.instance.currentUser.faceRecord.Count - 1;

        if (AppManager.instance.currentUser.faceRecord[currentPageIndex].targetFace.Count == 1)
        {
            imageNextBtn.gameObject.SetActive(false);
            imagePreviousBtn.gameObject.SetActive(false);
        }
        else
        {
            imageNextBtn.gameObject.SetActive(true);
            imagePreviousBtn.gameObject.SetActive(true);
        }


            //set to first image when turn to a new page
            currentImageIndex = 0;



        faceName.text = AppManager.instance.currentUser.faceRecord[currentPageIndex].targetName;
        faceRelationship.text = AppManager.instance.currentUser.faceRecord[currentPageIndex].targetRelationship;

        DisplayCurrentFaceImage(currentPageIndex, currentImageIndex);
        pageIndicatorText.text = (currentPageIndex + 1).ToString() + "/" + totalPage.ToString();
        faceName.interactable = false;
        faceRelationship.interactable = false;
        editBtn.gameObject.SetActive (true);
        delBtn.gameObject.SetActive(true);
        okBtn.gameObject.SetActive(false);
        cancelEditBtn.gameObject.SetActive(false);

    }


    public void NextPageBtn_Callback()
    {
        faceName.interactable = false;
        faceRelationship.interactable = false;

        editBtn.gameObject.SetActive(true);
        delBtn.gameObject.SetActive(true);
        okBtn.gameObject.SetActive(false);
        cancelEditBtn.gameObject.SetActive(false);

        currentPageIndex = (currentPageIndex + 1) % AppManager.instance.currentUser.faceRecord.Count;
            
            
            
        if (AppManager.instance.currentUser.faceRecord[currentPageIndex].targetFace.Count == 1)
        {
            imageNextBtn.gameObject.SetActive(false);
            imagePreviousBtn.gameObject.SetActive(false);
        }
        else
        {
            imageNextBtn.gameObject.SetActive(true);
            imagePreviousBtn.gameObject.SetActive(true);
        }

        currentImageIndex = 0;


        faceName.text = AppManager.instance.currentUser.faceRecord[currentPageIndex].targetName;
        faceRelationship.text = AppManager.instance.currentUser.faceRecord[currentPageIndex].targetRelationship;
            
        DisplayCurrentFaceImage(currentPageIndex, currentImageIndex);
        pageIndicatorText.text = (currentPageIndex + 1).ToString() + "/" + totalPage.ToString();
        faceName.interactable = false;
        faceRelationship.interactable = false;
        editBtn.gameObject.SetActive(true);
        delBtn.gameObject.SetActive(true);
        okBtn.gameObject.SetActive(false);
        cancelEditBtn.gameObject.SetActive(false);

    }

}
