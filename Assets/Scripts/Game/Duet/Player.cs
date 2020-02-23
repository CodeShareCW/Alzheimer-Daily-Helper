using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public AudioSource coinCollectSound;
    public GameObject gameOverPanel;
    public GameObject particleEffect;
    public static bool isGameOver=false;
    // Start is called before the first frame update

    private void Awake()
    {
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!isGameOver)
        {
            if (col.tag == "Obstacle")
            {
                Debug.Log("Collide");
                Destroy(gameObject);
                particleEffect.SetActive(true);
                isGameOver = true;
                gameOverPanel.SetActive(true);
                gameOverPanel.GetComponent<Animator>().Play("GameOverPanelAnim");

            }
            if (col.tag == "Star")
            {
                coinCollectSound.Play();
                ScoreManager.IncrementStarScore();
                GetComponent<Animator>().Play("PlayerCollectStarAnim", 0, -1f);
                Destroy(col.gameObject);
            }
        }
    }
}
