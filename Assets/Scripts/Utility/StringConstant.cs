public class LauchingInterfaceScene
{
    public const string EntryScene = "Entry Scene";
    public const string LoginScene = "Login Scene";
    public const string RegisterScene = "Register Scene";
    //public const string RegiserUserLevelScene = "Register User Level Scene";
    //public const string ForgetPasswordScene = "Forget Password Scene";

}

public class ActivityScene
{
    public const string HomeScene = "Home Scene";

    public const string FillMyBrainScene = "FillMyBrain Scene";


    public const string WhoAreYouScene = "WhoAreYou Scene";
    public const string HelloTodayScene = "HelloToday Scene";

    public const string Add_FlashcardScene = "Add Flashcard Scene";
    public const string Show_FlashcardScene = "Flashcard List Scene";

    public const string TrainingScene = "Training Scene";
    public const string Training_DuetScene = "Duet Scene";
    public const string Training_ISayScene = "ISay Scene";

    //Caregiver scene
    public const string Caregiver_MainScene = "Caregiver Main Scene";
    public const string Caregiver_ShowImageUploaded_Scene= "Caregiver Show Image Uploaded Scene";
    public const string Caregiver_UploadImage_Scene = "Caregiver Upload Image Scene";
    public const string Caregiver_AddReminder_Scene = "Caregiver Add Reminder Scene";
    public const string Caregiver_ShowReminder_Scene = "Caregiver Show Reminder Scene";


    public const string AchievementScene = "Achievement Scene";
    public const string ProfileScene = "Profile Scene";
    public const string LeaderboardScene_Duet = "Leaderboard Duet Scene";
    public const string LeaderboardScene_ISay = "Leaderboard ISay Scene";
}


public class AnimationName
{
    public const string ExitConfirmTextAnimation = "ExitConfirmTextAnim";
    public const string CameraClickEffectAnimation = "ClickEffectAnim";

    public const string NextImageTurningAnimation = "NextImageTurningAnim";
    public const string PreviousImageTurningAnimation = "PreviousImageTurningAnim";


    public const string NextPageTurningAnimation = "NextPageTurningAnim";
    public const string PreviousPageTurningAnimation = "PreviousPageTurningAnim";
}

public class URIName
{
    public const string LOGIN_URI= "https://alzheimerdailyhelper.000webhostapp.com/AlzheimerDailyHelper/Login.php";
    public const string REGISTER_URI = "http://gmm-student.fc.utm.my/~ccw/Register.php";
    public const string PROFILE_URI = "https://alzheimerdailyhelper.000webhostapp.com/AlzheimerDailyHelper/Profile.php";
}

public class FIREBASE_DB_NAME
{
    public const string DATABASE_URL = "https://alzheimer-daily-helper.firebaseio.com/";
}
public class MONGODB_CONSTANTS
{
    public const string MONGO_URI = "mongodb+srv://chai:wen19970803@mycluster-fjphx.mongodb.net/test?retryWrites=true&w=majority";
    public const string DATABASE_NAME = "AlzheimerDailyHelper";
    public const string USER_COLLECTION = "Users";
    public const string FACE_COLLECTION = "Face Record";
    public const string CAREGIVER_COLLECTION = "Caregiver";
}

public class StringPattern
{
    public const string PHONE_PATTERN = @"^(\+?6?01)[0-46-9]-*[0-9]{7,8}$";

    public const string USERNAME_AND_DISCRIMINATOR_PATTERN = @"^[a-zA-Z0-9]{4,20}#[0-9]{4}$";
    public const string USERNAME_PATTERN = @"^[a-zA-Z0-9]{4,20}$";
    public const string PASSWORD_PATTERN = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#\$%\^&\*])(?=.{8,})";

    public const string RANDOM_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
}