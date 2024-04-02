using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.Android;
using static UnityEngine.GraphicsBuffer;
public class Ladron : MonoBehaviour
{

    NavMeshAgent agent;
    RaycastHit hit;
    enum Estados { Caminando, Huyendo, Atrapado, Escondido, Encerrado, Aturdido } // Evitando // Liberar

    GameObject[] puntosRecorrido;

    [SerializeField]
    Estados estadoActual;
    
    GameObject jaula;
    GameObject enemigos;
    Transform[] rayCasters = new Transform[5];

    Estados estadoPrevio;
    [SerializeField]  float duracionAturdido;
    float tiempoAturdido;

    [SerializeField] float velocidadBase = 3.5f;
    [SerializeField] float velocidadHuida = 4f;
    [SerializeField] float tiempoEscondido = 7;
    [SerializeField] float distanciaParadaBase = 0;
    [SerializeField] float distanciaParadaHuida = 2;
    [SerializeField] float anguloCamino = 30;
 
    // Start is called before the first frame update

    public string getEstadoActual()
    {
        return estadoActual.ToString();
    }

    void Start()
    {
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
            case Estados.Aturdido:
                Aturdido();
                break;
            default:
                Debug.Log(gameObject.name+"ERROR: ESTADO DESCONOCIDO");
                break;
        }
    }

    void Caminando() // REESTABLECER COSAS BASES AL VOLVER AQUI 
    {
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            CambiarDestino();
        }
        // Tambien por si detecta con raycast un agente cambiar a huyendo / PARA ESTO EVITANDO
        foreach (Transform t in rayCasters)
        {
            if (Physics.Raycast(gameObject.transform.position, t.transform.forward, out hit) && hit.transform.CompareTag("Agente") && Vector3.Angle(gameObject.transform.position - agent.path.corners[getActualCorner(agent.path.corners)], gameObject.transform.position - hit.point) >= 45 && Vector3.Angle(agent.transform.forward,agent.transform.position - transform.position) <= 30)
            {
                agent.path = CaminoHuida(hit.transform.gameObject,puntosRecorrido.ToList());
                

            }

            //AÑADIR PARA DETECTAR MULTILES ENEMIGOS
        }
    }

     int getActualCorner(Vector3[] corners)
    {
        int i;

        for(i = 0; i < corners.Length-1;i++)
        {
            if(Vector3.Angle(transform.forward,transform.position - corners[i]) <= 5)
            {
                return i;
            }
        }
        return i;
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

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            agent.isStopped = true;
        }

        if (jaula.GetComponent<Jaula>().getJaulaAbierta())
        {

            agent.isStopped = false;
            GetComponent<SphereCollider>().enabled = true; 
            GetComponent<CapsuleCollider>().enabled = true;
            CambiarDestino();
            estadoActual = Estados.Caminando;
        }
    }
    public void Detectado(GameObject enemigo)
    {
        
        enemigos = enemigo;
        if (Vector3.Angle(gameObject.transform.position - agent.path.corners[getActualCorner(agent.path.corners)], gameObject.transform.position - enemigo.transform.position) <= anguloCamino)
        {
            NavMeshPath destino = CaminoHuida(enemigo, puntosRecorrido.ToList());
            agent.path = destino;
        }
        estadoActual = Estados.Huyendo;
        
        agent.speed = velocidadHuida;
        agent.stoppingDistance = distanciaParadaHuida;
    }
    void Huyendo()
    {
        if(agent.remainingDistance <= agent.stoppingDistance)
        {
            
            agent.path = CaminoHuida(enemigos, puntosRecorrido.ToList());
        }
        if(enemigos.GetComponent<Agente>().getEstadoActual() != "Persiguiendo" && enemigos.GetComponent<Agente>().getEstadoActual() != "Rastreando")
        {
            estadoActual = Estados.Caminando;
            
            CambiarDestino();
            enemigos = null;
            agent.speed = velocidadBase;
            agent.stoppingDistance = distanciaParadaBase;
        }
       
        foreach (Transform t in rayCasters)
        {
            if (Physics.Raycast(gameObject.transform.position, t.transform.forward, out hit) && hit.transform.CompareTag("Caja") && estadoActual == Estados.Huyendo && Vector3.Angle(gameObject.transform.position - enemigos.transform.position, gameObject.transform.position - hit.point) >= 45)
            {
                
                estadoActual = Estados.Escondido;
                agent.SetDestination(hit.transform.position);
               
            }

            //AÑADIR PARA DETECTAR MULTILES ENEMIGOS
        }
    }

    void Escondiendo() 
    {
        if(enemigos != null && enemigos.GetComponent<Agente>().getEstadoActual() != "Persiguiendo" && enemigos.GetComponent<Agente>().getEstadoActual() != "Rastreando")
        {
            estadoActual = Estados.Caminando;
            
            agent.speed = velocidadBase;
            agent.stoppingDistance = distanciaParadaBase;
            CambiarDestino();
            enemigos = null;
        }
    }

    void Aturdido()
    {
        if(Time.time - tiempoAturdido >= duracionAturdido)
        {
            agent.isStopped = false;
            estadoActual = estadoPrevio;
        }
    }

    NavMeshPath CaminoHuida(GameObject enemigo,List<GameObject> recorrido) // CAMBIAR PARA TENER EN CUENTA MULTIPLES ENEMIGOS
    {
        
        if (recorrido.Count == 0)
        {
            Debug.LogError("RECORRIDO DE CAMINOHUIDA VACIO");
            return null;
        }
        NavMeshPath path = new NavMeshPath();
        GameObject puntoElegido = puntosRecorrido[Random.Range(0, recorrido.Count - 1)];
        NavMesh.CalculatePath(gameObject.transform.position,puntoElegido.transform.position,NavMesh.AllAreas, path);
        if(Vector3.Angle(gameObject.transform.position - path.corners[0],gameObject.transform.position - enemigo.transform.position) <= anguloCamino)
        {
            recorrido.Remove(puntoElegido);
            return CaminoHuida(enemigo, recorrido);
        }
        else
        {
            return path;
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
            if(other.gameObject.GetComponent<Agente>().getEstadoActual() != "Encerrando") { 
           
            agent.enabled = true;
            agent.isStopped = false;
            agent.speed = velocidadBase;
            agent.SetDestination(jaula.transform.position);
            GetComponent<SphereCollider>().enabled = false;
            GetComponent<CapsuleCollider>().enabled = false;
            enemigos = null;
            agent.stoppingDistance = distanciaParadaBase;
            estadoActual = Estados.Atrapado;
            }
        }
        if (other.gameObject.CompareTag("Caja") && (estadoActual == Estados.Huyendo || estadoActual == Estados.Escondido))
        {
            GetComponent<SphereCollider>().enabled = false;
            GetComponent<CapsuleCollider>().enabled = false;
            
            agent.speed = velocidadBase;
            agent.stoppingDistance = distanciaParadaBase;
            StartCoroutine(Esconderse());
            enemigos = null;
        }
        if (other.gameObject.CompareTag("Bala") && (estadoActual == Estados.Huyendo || estadoActual == Estados.Caminando || estadoActual == Estados.Escondido))
        {
            tiempoAturdido = Time.time;
            estadoPrevio = estadoActual;
            estadoActual = Estados.Aturdido;
            agent.isStopped = true;
        }
    }

    IEnumerator Esconderse()
    {
        yield return new WaitForSeconds(tiempoEscondido);

        CambiarDestino();
        estadoActual = Estados.Caminando;
        agent.speed = velocidadBase;
        agent.stoppingDistance = distanciaParadaBase;
        GetComponent<SphereCollider>().enabled = true;

        GetComponent<CapsuleCollider>().enabled = true;


    }

    
}
