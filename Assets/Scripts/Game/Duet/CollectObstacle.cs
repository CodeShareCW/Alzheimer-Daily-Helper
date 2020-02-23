using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectObstacle : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag=="Obstacle")
        {
            if (!Player.isGameOver)
            ScoreManager.IncrementScore();
            Destroy(collision.gameObject);
        }
        if (collision.tag == "Star")
        {
            Destroy(collision.gameObject);
        }

    }
}
