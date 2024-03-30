using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;
public class Ladron : MonoBehaviour
{

    Rigidbody rb;
    NavMeshAgent agent;
    RaycastHit hit;
    enum Estados { Caminando, Huyendo, Atrapado, Escondido, Encerrado}

    GameObject[] puntosRecorrido;

    [SerializeField]
    Estados estadoActual;
    GameObject jaula;
    GameObject enemigos;
    GameObject caja;
    Transform[] rayCasters = new Transform[5];

    // Start is called before the first frame update

    public string getEstadoActual()
    {
        return estadoActual.ToString();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        puntosRecorrido = GameObject.FindGameObjectsWithTag("Recorrido");
        jaula = GameObject.FindGameObjectWithTag("Jaula");
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
    void Update()
    {
        switch (estadoActual)
        {
            case Estados.Caminando:
                Caminando();
                break;
            case Estados.Atrapado:
                Atrapado();
                break;
            case Estados.Encerrado:
                Encerrado();
                break;
            case Estados.Huyendo:
                Huyendo();
                break;
            case Estados.Escondido:
                Escondiendo();
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
        // Tambien por si detecta con raycast un agente cambiar a huyendo
       
    }

    void Atrapado()
    {
        if(agent.remainingDistance <= agent.stoppingDistance)
        {
            estadoActual = Estados.Encerrado;
            agent.ResetPath();
        }



    }

    void Encerrado()
    {
        if(jaula.GetComponent<Jaula>().getJaulaAbierta())
        {

            
            GetComponent<SphereCollider>().enabled = true; // PROBAR CON CAMBIAR LAYER???
            GetComponent<CapsuleCollider>().enabled = true;
            CambiarDestino();
            estadoActual = Estados.Caminando;
        }
    }
    public void Detectado(GameObject enemigo)
    {
        enemigos = enemigo;
        NavMeshPath destino = CaminoHuida(enemigo, puntosRecorrido.ToList());
        agent.path = destino;
        estadoActual = Estados.Huyendo;
    }
    void Huyendo()
    {
        if(agent.remainingDistance <= agent.stoppingDistance)
        {
            CaminoHuida(enemigos, puntosRecorrido.ToList());
        }
        if(enemigos.GetComponent<Agente>().getEstadoActual() != "Persiguiendo" && enemigos.GetComponent<Agente>().getEstadoActual() != "Rastreando")
        {
            estadoActual = Estados.Caminando;
            CambiarDestino();
            enemigos = null;
        }
       
        foreach (Transform t in rayCasters)
        {
            if (Physics.Raycast(gameObject.transform.position, t.transform.forward, out hit) && hit.transform.CompareTag("Caja"))
            {
                estadoActual = Estados.Escondido;
                agent.SetDestination(hit.transform.position);
            }
        }
    }

    void Escondiendo()
    {
        if(Vector3.Distance(transform.position,enemigos.transform.position) >= 20) // CAMBIAR PARA MOD EN INSPECTOR
        {
            estadoActual = Estados.Caminando;
            CambiarDestino();
        }
    }

    NavMeshPath CaminoHuida(GameObject enemigo,List<GameObject> recorrido)
    {
        if(recorrido.Count == 0)
        {
            Debug.LogError("RECORRIDO DE CAMINOHUIDA VACIO");
            return null;
        }
        NavMeshPath path = new NavMeshPath();
        GameObject puntoElegido = puntosRecorrido[Random.Range(0, recorrido.Count - 1)];
        NavMesh.CalculatePath(gameObject.transform.position,puntoElegido.transform.position,NavMesh.AllAreas, path);
        if(Vector3.Angle(gameObject.transform.position - path.corners[0],gameObject.transform.position - enemigo.transform.position) <= 15)
        {
            recorrido.Remove(puntoElegido);
            return CaminoHuida(enemigo, recorrido);
        }
        return path;
    }

    void CambiarDestino()
    {
        agent.SetDestination(puntosRecorrido[Random.Range(0, puntosRecorrido.Length - 1)].transform.position);
    }

    private void OnTriggerEnter(Collider other)
    {
       
        if (other.gameObject.CompareTag("Agente") && estadoActual != Estados.Escondido)
        {
            if(other.gameObject.GetComponent<Agente>().getEstadoActual() != "Encerrando") { 
            estadoActual = Estados.Atrapado;
            agent.SetDestination(jaula.transform.position);
            GetComponent<SphereCollider>().enabled = false;
            GetComponent<CapsuleCollider>().enabled = false;
            }
        }

        
    }

    
}
