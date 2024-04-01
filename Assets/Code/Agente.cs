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
    Transform[] rayCasters = new Transform[5];
    GameObject arma;
    [SerializeField]
    GameObject prefabBala;

    [SerializeField]
    Text debugEstadoActual;
    [SerializeField]
    float encerrandoStopingDistance;
    [SerializeField]
    float velocidadCorriendo;

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
        if (agent.remainingDistance <= agent.stoppingDistance) // Si llega al destino se mueve hacia otro  / ¿PROBLEMAS CON ARMA?
        {
            CambiarDestino();
        }
        
        foreach(Transform t in rayCasters)
        {
            if (Physics.Raycast(transform.position, t.forward, out hit) && hit.transform.gameObject.CompareTag("Ladron") && estadoActual == Estados.Patruyando) // Si detecta un ladron, cambia al estado persiguiendo / Mejorar detección (que sean varios raycast)
            {//En vez de tag, mejor layer?
                target = hit.transform.gameObject;
                estadoActual = Estados.Persiguiendo;
                agent.speed = velocidadCorriendo;
                hit.transform.SendMessage("Detectado", gameObject);
            }
        }

        
    }

    void Persiguiendo() // EN PERSIGUIENDO AUMENTAR DISTANCIA DE PARADA
    {
        Debug.DrawRay(transform.position, target.transform.position - transform.position);
        Seguir(target);
        
        if (Physics.Raycast(transform.position, target.transform.position - transform.position, out hit) && !hit.transform.gameObject.CompareTag("Ladron")) // Si el objetivo desaparece, sigue hasta la ultima posicion vista / ¿CAMBIAR PARA QUE NO LO VEA?
        {
            
            ultimaPosicion = target.transform.position;
            agent.SetDestination(ultimaPosicion + target.transform.forward*0.3f);
            target = null;
            estadoActual = Estados.Rastreando;
        }

        foreach (Transform t in rayCasters)
        {
            if (Physics.Raycast(transform.position, t.forward, out hit) && estadoActual == Estados.Persiguiendo) // 
            {
                if (hit.transform.gameObject.CompareTag("Ladron") && !hit.transform.gameObject.Equals(target)) // Probar que solo lo hace con ladrones 
                {

                    //hit.transform.SendMessage("Detectado", gameObject); CAMBIAR ESTADO HUIDO PARA QUE TAMBIEN SALGA SI DEJA DE DETECTAR
                    if (Vector3.Distance(transform.position, target.transform.position) > Vector3.Distance(transform.position, hit.transform.position))
                    {
                        hit.transform.SendMessage("Detectado", gameObject);
                        target = hit.transform.gameObject;
                    }
                }
            }
        }

        if(llevandoArma)
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
        yield return new WaitForSeconds(1);
        Instantiate(prefabBala, arma.transform.position + (arma.transform.forward), arma.transform.rotation);
        llevandoArma = false;
        disparando = false;
        Destroy(arma);
    }

    void Rastreando()
    {
        foreach(Transform t in rayCasters)
        {
            if (Physics.Raycast(transform.position, t.forward, out hit) && estadoActual == Estados.Rastreando) // Si detecta un ladron, cambia al estado persiguiendo
            {
                if (hit.transform.gameObject.CompareTag("Ladron")) // Probar que solo lo hace con ladrones 
                {
                    target = hit.transform.gameObject;
                    estadoActual = Estados.Persiguiendo;
                }
            }
        }

        if(agent.remainingDistance <= agent.stoppingDistance) 
        {
            target = null;
            estadoActual = Estados.Patruyando;
            agent.speed = 3.5f; // SERIALIZEDFIELD
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
                agent.speed = 3.5f;
            }  
                

        }
        if (other.gameObject.CompareTag("Arma") && estadoActual != Estados.Encerrando && !llevandoArma)
        {
            arma = other.gameObject;
            llevandoArma = true;
            arma.GetComponent<SphereCollider>().enabled = false;
            arma.transform.SetParent(transform);
            arma.transform.localPosition = Vector3.right*0.6f; // LERP?
            arma.transform.rotation = transform.rotation;
        }
    }
}
