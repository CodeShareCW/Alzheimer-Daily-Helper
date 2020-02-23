using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecretTunnel_PSController : MonoBehaviour
{

    void Awake()
    {
        if (AppManager.instance.currentUser.brainStatus == 10000)
            transform.GetChild(0).gameObject.SetActive(false);
    }
}
