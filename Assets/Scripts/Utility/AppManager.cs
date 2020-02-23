
using Assets.SimpleAndroidNotifications;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AppManager : MonoBehaviour
{
    public static AppManager instance;
    public string currentScene; //record current scene
    public UserAccount currentUser;
    public string currentUserName;
    public string currentUserPhone;
    public MongoClient client;
    public IMongoDatabase db;
    public IMongoCollection<UserAccount> userCollection;
    public bool updatingMongoDB = false;

    public GameObject reminderPopOutPanel;

    public GameObject msg_panel;

    public GameObject sendingDataPanel;

    public void ConfigMongoDB()
    {
        System.Net.ServicePointManager.Expect100Continue = false;
        client = new MongoClient(MONGODB_CONSTANTS.MONGO_URI);
        db = client.GetDatabase(MONGODB_CONSTANTS.DATABASE_NAME);
        userCollection = db.GetCollection<UserAccount>(MONGODB_CONSTANTS.USER_COLLECTION);
    }

    private void CheckOrAskInternetPermission()
    {
        if (AndroidRuntimePermissions.CheckPermission("android.permission.INTERNET") != AndroidRuntimePermissions.Permission.Granted)
        {
            AndroidRuntimePermissions.Permission result = AndroidRuntimePermissions.RequestPermission("android.permission.INTERNET");

            if (result == AndroidRuntimePermissions.Permission.Denied)
            {
                AndroidRuntimePermissions.OpenSettings();

            }
        }
    }

    private void CheckOrAskCameraPermission()
    {
        if (AndroidRuntimePermissions.CheckPermission("android.permission.CAMERA") != AndroidRuntimePermissions.Permission.Granted)
        {
            AndroidRuntimePermissions.Permission result = AndroidRuntimePermissions.RequestPermission("android.permission.CAMERA");
            if (result == AndroidRuntimePermissions.Permission.Denied)
            {
                AndroidRuntimePermissions.OpenSettings();
            }
        }
    }
    private void CheckOrAskWriteStoragePermission()
    {
        if (AndroidRuntimePermissions.CheckPermission("android.permission.WRITE_EXTERNAL_STORAGE") != AndroidRuntimePermissions.Permission.Granted)
        {
            AndroidRuntimePermissions.Permission result = AndroidRuntimePermissions.RequestPermission("android.permission.WRITE_EXTERNAL_STORAGE");
            if (result == AndroidRuntimePermissions.Permission.ShouldAsk)
            {
                Debug.Log("Permission state: " + result);
            }
            else if (result == AndroidRuntimePermissions.Permission.Denied)
            {
                AndroidRuntimePermissions.OpenSettings();
                Debug.Log("Permission state: " + result);

            }
        }
    }

    private void Awake()
    {
        CheckOrAskInternetPermission();
        CheckOrAskCameraPermission();
        CheckOrAskWriteStoragePermission();

       
        MakeSingleton();
        ConfigMongoDB();

    }

    public void SetCurrentUser(UserAccount user)
    {
        currentUser = user;
        currentUserName = currentUser.username;
        currentUserPhone = currentUser.phoneNumber;
        Debug.Log("Username: " + currentUser.username);
        Debug.Log("Phone Number: " + currentUser.phoneNumber);
        Debug.Log("Hash Password: " + currentUser.hashPassword);
        Debug.Log("Status: " + currentUser.status);
        Debug.Log("Last Login: " + currentUser.lastLogin);
        Debug.Log("Score: " + currentUser.coins);
    }
    void MakeSingleton()
    {
        if (instance != null)
            Destroy (gameObject);
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //every 10 second check for network
        if (Time.time % 10 == 1)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                msg_panel.SetActive(true);
            }
            else
            {
                msg_panel.SetActive(false);
            }
        }




        //dont allow return when processing db
        if (updatingMongoDB) return;

        currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ReturnToPreviousPage();
            
        }
    }
    bool hasNotified = false;
    private void FixedUpdate()
    {

        if (currentUser != null && currentUser.reminderRecord != null && currentUser.reminderRecord.Count != 0)
        {

            for (int i = 0; i < currentUser.reminderRecord.Count; i++)
            {
                if (!hasNotified && currentUser.reminderRecord[i].reminderTime.Hour == DateTime.SpecifyKind(System.DateTime.Now, DateTimeKind.Utc).Hour
                    && currentUser.reminderRecord[i].reminderTime.Minute == DateTime.SpecifyKind(System.DateTime.Now, DateTimeKind.Utc).Minute
                    )
                {
                    ScheduleCustomAndroidNotification(i);
                    //pop out to screen panel
                    var pop=Instantiate(reminderPopOutPanel, GameObject.FindGameObjectWithTag("BG").transform);
                    
                    pop.transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<Text>().text = currentUser.reminderRecord[i].reminderTime.ToShortTimeString();
                    pop.transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<Text>().text = currentUser.reminderRecord[i].reminder_content;
                    pop.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(ReminderOKBtn_Callback);
                    hasNotified = true;
                }
            }

            for (int i = 0; i < currentUser.reminderRecord.Count; i++)
            {
                if (!hasNotified && currentUser.reminderRecord[i].reminderTime.Hour > DateTime.SpecifyKind(System.DateTime.Now, DateTimeKind.Utc).Hour
                    || currentUser.reminderRecord[i].reminderTime.Minute > DateTime.SpecifyKind(System.DateTime.Now, DateTimeKind.Utc).Minute
                    )
                {
                    hasNotified = false;
                }
            }

        }
    }

    //android notification
    public void ScheduleCustomAndroidNotification(int index)
    {
        var notificationParams = new NotificationParams
        {
            Id = UnityEngine.Random.Range(0, int.MaxValue),
            Delay = TimeSpan.FromSeconds(5),
            Title = "Hey, you got to do this now",
            Message =  AppManager.instance.currentUser.reminderRecord[index].reminder_content + " at " + AppManager.instance.currentUser.reminderRecord[index].reminderTime.ToShortTimeString(),
            Ticker = "Ticker",
            Sound = true,
            Vibrate = true,
            Light = true,
            SmallIcon = NotificationIcon.Heart,
            SmallIconColor = new Color(0, 0.5f, 0),
            LargeIcon = "app_icon"
        };

        NotificationManager.SendCustom(notificationParams);
    }


    void ReturnToPreviousPage()
    {
        currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == LauchingInterfaceScene.LoginScene || currentScene == LauchingInterfaceScene.RegisterScene)
        {
            SceneManager.LoadScene(LauchingInterfaceScene.EntryScene);
        }

        else if (currentScene==ActivityScene.FillMyBrainScene)
        {
            SceneManager.LoadScene(ActivityScene.HomeScene);
        }

        else if (currentScene == ActivityScene.WhoAreYouScene)
        {
            FindObjectOfType<WebCamScript>().StopCamera();
            SceneManager.LoadScene(ActivityScene.HomeScene);

        }

        else if(currentScene == ActivityScene.HelloTodayScene || currentScene == ActivityScene.TrainingScene)
        {
            SceneManager.LoadScene(ActivityScene.HomeScene);
        }
        else if (currentScene == ActivityScene.Caregiver_MainScene|| currentScene == ActivityScene.AchievementScene|| currentScene == ActivityScene.ProfileScene)
        {
            SceneManager.LoadScene(ActivityScene.HomeScene);
        }
        else if (currentScene == ActivityScene.Caregiver_UploadImage_Scene)
        {
            FindObjectOfType<WebCamScript>().StopCamera();
            SceneManager.LoadScene(ActivityScene.Caregiver_MainScene);
        }

        else if (currentScene == ActivityScene.Caregiver_ShowReminder_Scene)
        {
            SceneManager.LoadScene(ActivityScene.Caregiver_MainScene);
        }

        else if (currentScene==ActivityScene.Caregiver_ShowImageUploaded_Scene)
        {
            SceneManager.LoadScene(ActivityScene.Caregiver_UploadImage_Scene);
        }

        else if (currentScene == ActivityScene.Caregiver_AddReminder_Scene)
        {
            SceneManager.LoadScene(ActivityScene.Caregiver_ShowReminder_Scene);
        }
        else if (currentScene == ActivityScene.Add_FlashcardScene)
        {
            SceneManager.LoadScene(ActivityScene.Show_FlashcardScene);
        }
        else if (currentScene == ActivityScene.Show_FlashcardScene)
        {
            SceneManager.LoadScene(ActivityScene.HomeScene);
        }
        else if (currentScene == ActivityScene.Training_DuetScene || currentScene == ActivityScene.Training_ISayScene)
        {
            SceneManager.LoadScene(ActivityScene.TrainingScene);
        }
        else if (currentScene == ActivityScene.LeaderboardScene_Duet)
        {
            SceneManager.LoadScene(ActivityScene.TrainingScene);
        }
        else if (currentScene == ActivityScene.LeaderboardScene_ISay)
        {
            SceneManager.LoadScene(ActivityScene.TrainingScene);
        }
    }

    public void ReminderOKBtn_Callback()
    {
        reminderPopOutPanel.SetActive(false);
    }

    public List<UserAccount> usersList;
    bool isFBUserExist = false;

    //facebook create user
    public async Task setupFBUser(string fbID, string name, string email, byte[] picture)
    {
        if (!isFBUserExist)
        {
            UserAccount newUser = new UserAccount(name, email, "FBPass");
            newUser.FBID = fbID;
            if (picture != null)
                newUser.profile_picture = picture;

            newUser.CreateOn = DateTime.SpecifyKind(System.DateTime.Now, DateTimeKind.Utc);

            //update mongodb
 
            usersList.Add(newUser);

            await Task.Run(() => TaskRegisterFBUser(newUser));
            currentUser = newUser;

            //login new user
            //update mongod last login and status
            await Task.Run(() => TaskUpdateStatusAndLastLogin(email, "FBPass"));
            currentUser.lastLogin = DateTime.SpecifyKind(System.DateTime.Now, DateTimeKind.Utc);
            currentUser.status = 1;

        }

        else
        {
            //login the existing user
            foreach (var t in usersList)
            {
                if (t.FBID==fbID)
                {
                    currentUser = t;
                    //login new user
                    //update mongod last login and status

                    await Task.Run(() => TaskUpdateStatusAndLastLogin(email, "FBPass"));
                    currentUser.lastLogin = DateTime.SpecifyKind(System.DateTime.Now, DateTimeKind.Utc);
                    currentUser.status = 1;

                }
            }
        }

    }

    public async Task checkFBUserExist(string FBUserID)
    {
        usersList = new List<UserAccount>();
        usersList=await Task.Run(()=>TaskFetchUser());

        if (usersList.Count != 0)
        {
            foreach(var t in usersList)
            {
                if (FBUserID == t.FBID)
                {
                    isFBUserExist = true;
                    break;
                }
            }
        }


    }


    private async Task<List<UserAccount>> TaskFetchUser()
    {
        return await userCollection.Find(_ => true).ToListAsync();   
    }

    private async Task TaskRegisterFBUser(UserAccount u)
    {
        await userCollection.InsertOneAsync(u);
    }



    private async Task TaskUpdateStatusAndLastLogin(string phNo, string pwd)
    {
        await userCollection.UpdateOneAsync(user => user._id == currentUser._id, Builders<UserAccount>.Update.Set(user => user.lastLogin, DateTime.SpecifyKind(System.DateTime.Now, DateTimeKind.Utc)));
        await userCollection.UpdateOneAsync(user => user._id == currentUser._id, Builders<UserAccount>.Update.Set(user => user.status, 1));
    }


}
