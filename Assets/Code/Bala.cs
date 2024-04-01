using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bala : MonoBehaviour
{

    [SerializeField]
    float tiempoDesaparecer;
    [SerializeField]  float velocidad;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, tiempoDesaparecer);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * velocidad * Time.deltaTime);
    }



    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("AEFE");
        Destroy(gameObject);
    }
}
