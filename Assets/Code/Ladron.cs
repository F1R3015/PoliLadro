using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class Ladron : MonoBehaviour
{

    Rigidbody rb;
    NavMeshAgent agent;
    enum Estados { Caminando, Huyendo, Atrapado, Escondido}
    GameObject[] puntosRecorrido;
    Estados estadoActual;
    GameObject jaula;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        agent = rb.GetComponent<NavMeshAgent>();
        puntosRecorrido = GameObject.FindGameObjectsWithTag("Recorrido");
        jaula = GameObject.FindGameObjectWithTag("Jaula");

    }

    // Update is called once per frame
    void Update()
    {
        switch (estadoActual)
        {
            case Estados.Caminando:
                Caminando();
                break;
            case Estados.Atrapado:

                break;


            default:
                Debug.Log(gameObject.name+"ERROR: ESTADO DESCONOCIDO");
                break;
        }
    }

    void Caminando()
    {
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            CambiarDestino();
        }
    }

    void CambiarDestino()
    {
        agent.SetDestination(puntosRecorrido[Random.Range(0, puntosRecorrido.Length - 1)].transform.position);
    }

    private void OnTriggerEnter(Collider other)
    {
       
        if (other.gameObject.CompareTag("Agente"))
        {
            estadoActual = Estados.Atrapado;
            agent.ResetPath();
            agent.SetDestination(jaula.transform.position);
            GetComponent<SphereCollider>().enabled = false;
        }
    }

    
}
