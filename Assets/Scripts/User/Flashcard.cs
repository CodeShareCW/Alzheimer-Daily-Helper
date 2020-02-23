using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flashcard
{
    public string flashcard_title;
    public string flashcard_description;
    public byte[] flashcard_image;
    public DateTime createdOn;

    public Flashcard(string t, string d)
    {
        flashcard_title = t;
        flashcard_description = d;
    }

}
