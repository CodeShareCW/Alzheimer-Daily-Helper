using MongoDB.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UserAccount
{
    public ObjectId _id;

    public string FBID;

    public string username;
    public string phoneNumber;
    public string hashPassword;
    public int coins;
    public float brainStatus;
    public int highestScoreISays;
    public int highestScoreDuet;
    public byte[] profile_picture;

    public DateTime CreateOn;
    public DateTime lastLogin;
    public int loginDayCount;
    public bool isLoginDayCountDone;
    public int status;

    public DateTime DailyAwardReceivedTime;


    

    public CaregiverProfile caregiver;

    public List<FaceRecord> faceRecord;
    public List<Flashcard> flashCard;
    public List<ReminderRecord> reminderRecord;


    public List<UserAward> awardRecord;
    public bool hasNameOnBoardBefore;

    public UserAccount(string un="", string phNo="", string pwd="")
    {
        username = un;
        phoneNumber = phNo;
        hashPassword = pwd;

        isLoginDayCountDone = false;
        loginDayCount = 0;
        status = 0;
        coins = 0;
        highestScoreDuet = 0;
        highestScoreISays = 0;
        hasNameOnBoardBefore = false;

        faceRecord = new List<FaceRecord>();
        flashCard = new List<Flashcard>();

        awardRecord = new List<UserAward>();

    }

    

}
