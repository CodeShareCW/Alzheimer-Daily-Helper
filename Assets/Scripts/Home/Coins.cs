using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Coins : MonoBehaviour
{
    public int coins;
    private void Awake()
    {
        if (AppManager.instance.currentUser != null)
            GetComponent<Text>().text = AppManager.instance.currentUser.coins.ToString();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (AppManager.instance.currentUser != null)
            GetComponent<Text>().text = AppManager.instance.currentUser.coins.ToString();
    }
}
