using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] float tiempoEspera = 30f;
    [SerializeField] GameObject arma;
    bool corrutinaEmpezada = false;
 

    // Update is called once per frame
    void Update()
    {
        if(transform.childCount == 0 && !corrutinaEmpezada)
        {
            corrutinaEmpezada = true;
            StartCoroutine(Spawn());
        }
    }

    IEnumerator Spawn()
    {
        yield return new WaitForSeconds(tiempoEspera);
        Instantiate(arma,transform);
        corrutinaEmpezada = false;
    }
}
