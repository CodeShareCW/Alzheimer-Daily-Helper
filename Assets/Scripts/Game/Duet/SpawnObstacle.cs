using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnObstacle : MonoBehaviour
{
    public GameObject[] obs;
    public GameObject collection;
    public float maxX=2;
    public float minX = -2;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Spawn());
    }

    IEnumerator Spawn()
    {
        int randNum = Random.Range(0, obs.Length);

        var go=Instantiate(obs[randNum].gameObject, new Vector3(Random.Range(minX, maxX), collection.transform.position.y, collection.transform.position.z), collection.transform.rotation, collection.transform);
        AdjustSize(go);
        Debug.Log(Random.Range(minX, maxX));
        yield return new WaitForSeconds(2);
        StartCoroutine(Spawn());
    }


    void AdjustSize(GameObject go)
    {
        Vector3 temp=go.transform.localScale;
        temp.x = Random.Range(40, 70);
        temp.y = Random.Range(50, 70);
        go.transform.localScale=temp;
    }
    void AdjustRotation(GameObject go)
    {
        Vector3 temp = go.transform.localEulerAngles;
        temp.z = Random.Range(0, 180);

        go.transform.localEulerAngles = temp;
    }

}
