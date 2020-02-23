using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarSpawner : MonoBehaviour
{
    public GameObject[] starGO;
    public float maxX = 220;
    public float minX = -220;

    private void Awake()
    {

    }
    private void Start()
    {
        StartCoroutine(SpawnStar());
    }
    IEnumerator SpawnStar()
    {
        int randNum = Random.Range(0, starGO.Length);
        
        Instantiate(starGO[randNum], new Vector3(Random.Range(minX, maxX), this.transform.position.y, this.transform.position.z), this.transform.rotation, this.transform);
        yield return new WaitForSeconds(1);
        StartCoroutine(SpawnStar());
    }
}
