using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CaregiverProfile
{
    public string caregiverName;
    public string caregiverPhoneNo;

    public CaregiverProfile(string n, string p)
    {
        caregiverName = n;
        caregiverPhoneNo = p;
    }

}
