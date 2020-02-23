using MongoDB.Bson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceRecord
{
    public string targetName;
    public string targetRelationship;
    public List<byte[]> targetFace;
    
    public FaceRecord(string n, string r)
    {
        targetName = n;
        targetRelationship = r;
    }

    

    



}
