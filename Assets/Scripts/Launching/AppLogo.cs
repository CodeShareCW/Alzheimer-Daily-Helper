using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppLogo : MonoBehaviour
{
    private float floatAmplitudeScale = 0.005f;
    private float floatAmplitudePos = 0.1f;

    private float floatFrequency = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        Vector3 temp = GetComponent<RectTransform>().localScale;
        Vector3 temp2 = transform.position;

        temp.y += Mathf.Sin(Time.fixedTime * Mathf.PI * floatFrequency) * floatAmplitudeScale;
        temp.x += Mathf.Sin(Time.fixedTime * Mathf.PI * floatFrequency) * floatAmplitudeScale;

        temp2.y += Mathf.Sin(Time.fixedTime * Mathf.PI * floatFrequency) * floatAmplitudePos;

        GetComponent<RectTransform>().localScale = temp;
        transform.position=temp2;

    }
}
