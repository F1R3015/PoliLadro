using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Agente : MonoBehaviour // TENDRIA QUE IR POR ARMA
{

    Rigidbody rb;
    NavMeshAgent agent;
    RaycastHit hit;

    enum Estados { Patrullando, Persiguiendo, Rastreando, Encerrando }
    Estados estadoActual;

    GameObject[] puntosRecorrido;
    GameObject target;
    Vector3 ultimaPosicion;
    GameObject jaula;
    Transform[] rayCasters = new Transform[7];
    GameObject arma;
    [SerializeField]
    GameObject prefabBala;

    [SerializeField] float distanciaDisparo;
    [SerializeField] float paradaBase;
    [SerializeField] float encerrandoStopingDistance;
    [SerializeField] float velocidadBase;
    [SerializeField] float velocidadCorriendo;

    bool llevandoArma;
    bool disparando;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        agent = rb.GetComponent<NavMeshAgent>();
        puntosRecorrido = GameObject.FindGameObjectsWithTag("Recorrido");
        jaula = GameObject.FindGameObjectWithTag("Jaula");
        llevandoArma = false;
        disparando = false;
        int cont = 0;
        Transform[] t = GetComponentsInChildren<Transform>();
        for (int i = 0; i < t.Length; i++)
        {
            if (t[i].CompareTag("RayCaster"))
            {
                rayCasters[cont] = t[i];
                cont++;
            }
        }
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
            case Estados.Patrullando:
                Patrullando();
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


    }

    void Patrullando()
    {
        if (agent.remainingDistance <= agent.stoppingDistance) // Si llega al destino se mueve hacia otro
        {
            CambiarDestino();
        }
        
        foreach(Transform t in rayCasters)
        {
            if (Physics.Raycast(transform.position, t.forward, out hit) && hit.transform.gameObject.CompareTag("Ladron") && estadoActual == Estados.Patrullando) // Si detecta un ladron, cambia al estado persiguiendo
            {
                target = hit.transform.gameObject;
                estadoActual = Estados.Persiguiendo;
                agent.speed = velocidadCorriendo;
                
                hit.transform.SendMessage("Detectado", gameObject);
            }
        }

        
    }

    void Persiguiendo()
    {
        Debug.DrawRay(transform.position, target.transform.position - transform.position);
        Seguir(target);
        
        if (Physics.Raycast(transform.position, target.transform.position - transform.position, out hit) && !hit.transform.gameObject.CompareTag("Ladron")) 
        {
            
            ultimaPosicion = target.transform.position;
            agent.SetDestination(ultimaPosicion + target.transform.forward*0.3f);
            target = null;
            agent.stoppingDistance = 1;
            estadoActual = Estados.Rastreando;
        }

        foreach (Transform t in rayCasters)
        {
            if (Physics.Raycast(transform.position, t.forward, out hit) && estadoActual == Estados.Persiguiendo)
            {
                if (hit.transform.gameObject.CompareTag("Ladron") && !hit.transform.gameObject.Equals(target)) 
                {

                    
                    if (Vector3.Distance(transform.position, target.transform.position) > Vector3.Distance(transform.position, hit.transform.position))
                    {
                        hit.transform.SendMessage("Detectado", gameObject);
                        target = hit.transform.gameObject;
                    }
                }
            }
        }

        if(target != null && llevandoArma && Vector3.Distance(transform.position,target.transform.position) <= distanciaDisparo && Physics.Raycast(transform.position, target.transform.position - transform.position, out hit) && hit.transform.gameObject.CompareTag("Ladron"))
        {
            arma.transform.LookAt(target.transform.position);
            if (!disparando) {
                arma.transform.localPosition -= Vector3.right * 0.6f;
                arma.transform.localPosition += Vector3.forward * 0.7f;
                disparando = true;
                StartCoroutine(Dispara());
            }
        }

    }

    IEnumerator Dispara()
    {
        
        arma.transform.LookAt(target.transform.position+target.transform.position*0.5f);
        yield return new WaitForSeconds(0.5f);
        Instantiate(prefabBala, arma.transform.position + (arma.transform.forward)*0.5f, arma.transform.rotation);
        yield return new WaitForSeconds(1);
        llevandoArma = false;
        disparando = false;
        Destroy(arma);
    }

    void Rastreando()
    {
        foreach(Transform t in rayCasters)
        {
            if (Physics.Raycast(transform.position, t.forward, out hit) && estadoActual == Estados.Rastreando)
            {
                if (hit.transform.gameObject.CompareTag("Ladron")) 
                {
                    target = hit.transform.gameObject;
                    estadoActual = Estados.Persiguiendo;
                }
            }
        }

        if(agent.remainingDistance <= agent.stoppingDistance) 
        {
            target = null;
            estadoActual = Estados.Patrullando;
            agent.speed = velocidadBase; 
        }
    }

    void Encerrando()
    {
        if(agent.remainingDistance <= agent.stoppingDistance)
        {
            CambiarDestino();
            estadoActual = Estados.Patrullando;
            agent.stoppingDistance = paradaBase;
        }
    }

    void CambiarDestino()
    {
        agent.SetDestination(puntosRecorrido[Random.Range(0, puntosRecorrido.Length - 1)].transform.position);
    }

    void Seguir(GameObject obj)
    {
        agent.SetDestination(obj.transform.position + obj.transform.forward*0.5f);
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
                agent.speed = velocidadBase;
                
            }  
                

        }
        if (other.gameObject.CompareTag("Arma") && estadoActual != Estados.Encerrando && !llevandoArma)
        {
            arma = other.gameObject;
            llevandoArma = true;
            arma.GetComponent<SphereCollider>().enabled = false;
            arma.transform.SetParent(transform);
            arma.transform.localPosition = Vector3.right*0.6f;
            arma.transform.rotation = transform.rotation;
        }
    }
}
