using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Agente : MonoBehaviour
{

    Rigidbody rb;
    NavMeshAgent agent;
    RaycastHit hit;

    enum Estados { Patruyando, Persiguiendo, Rastreando, Encerrando }
    Estados estadoActual;

    GameObject[] puntosRecorrido;
    GameObject target;
    Vector3 ultimaPosicion;
    GameObject jaula;


    [SerializeField]
    Text debugEstadoActual;
    [SerializeField]
    float encerrandoStopingDistance;
    [SerializeField]
    float velocidadCorriendo;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        agent = rb.GetComponent<NavMeshAgent>();
        puntosRecorrido = GameObject.FindGameObjectsWithTag("Recorrido");
        jaula = GameObject.FindGameObjectWithTag("Jaula");
    }

    // Update is called once per frame

    public string getEstadoActual()
    {
        return estadoActual.ToString();
    }
    void Update()
    {
        switch (estadoActual)
        {
            case Estados.Patruyando:
                Patruyando();
                break;

            case Estados.Persiguiendo:
                Persiguiendo();
                break;
            
            case Estados.Rastreando:
                Rastreando();
                break;

            case Estados.Encerrando:
                Encerrando();
                break;

            default:
                Debug.Log(gameObject.name + "ERROR: ESTADO DESCONOCIDO");
                break;
        }


        debugEstadoActual.text = estadoActual.ToString();
    }

    void Patruyando()
    {
        if (agent.remainingDistance <= agent.stoppingDistance) // Si llega al destino se mueve hacia otro 
        {
            CambiarDestino();
        }
        
        if(Physics.Raycast(transform.position,transform.forward,out hit) && hit.transform.gameObject.CompareTag("Ladron")) // Si detecta un ladron, cambia al estado persiguiendo / Mejorar detección (que sean varios raycast)
        {//En vez de tag, mejor layer?
                target = hit.transform.gameObject;
                estadoActual = Estados.Persiguiendo;
                agent.speed = velocidadCorriendo;
                hit.transform.SendMessage("Detectado",gameObject);
        }

        
    }

    void Persiguiendo() // EN PERSIGUIENDO AUMENTAR DISTANCIA DE PARADA
    {
        Debug.DrawRay(transform.position, target.transform.position - transform.position);
        Seguir(target.transform.position);
        
        if (Physics.Raycast(transform.position, target.transform.position - transform.position, out hit) && !hit.transform.gameObject.CompareTag("Ladron")) // Si el objetivo desaparece, sigue hasta la ultima posicion vista / ¿CAMBIAR PARA QUE NO LO VEA?
        {
            ultimaPosicion = target.transform.position;
            agent.SetDestination(ultimaPosicion);
            estadoActual = Estados.Rastreando;
        }

    }

    void Rastreando()
    {
        if (Physics.SphereCast(transform.position,10f, transform.forward, out hit)) // Si detecta un ladron, cambia al estado persiguiendo / Mejorar detección
        {
            if (hit.transform.gameObject.CompareTag("Ladron")) // Probar que solo lo hace con ladrones / Ver si no es el mismo para decidir que hacer
            {
                    target = hit.transform.gameObject;
                    estadoActual = Estados.Persiguiendo;
            }
        }

        if(agent.remainingDistance <= agent.stoppingDistance) 
        {
            target = null;
            estadoActual = Estados.Patruyando;
            agent.speed = 3.5f;
        }
    }

    void Encerrando()
    {
        if(agent.remainingDistance <= agent.stoppingDistance)
        {
            CambiarDestino();
            estadoActual = Estados.Patruyando;
            agent.stoppingDistance = 0f;
            agent.speed = 3.5f;
        }
    }

    void CambiarDestino()
    {
        agent.SetDestination(puntosRecorrido[Random.Range(0, puntosRecorrido.Length - 1)].transform.position);
    }

    void Seguir(Vector3 obj)
    {
        agent.SetDestination(obj);
    }


    private void OnTriggerEnter(Collider other)
    {
        
        if (other.gameObject.CompareTag("Ladron") && estadoActual != Estados.Encerrando)
        {
              if(other.gameObject.GetComponent<Ladron>().getEstadoActual() != "Escondido")
            {
                agent.SetDestination(jaula.transform.position);
                estadoActual = Estados.Encerrando;
                target = null;
                agent.stoppingDistance = encerrandoStopingDistance;
                agent.speed = 3.5f;
            }  
                

        }
    }
}
