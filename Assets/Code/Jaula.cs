using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jaula : MonoBehaviour
{


    bool jaulaAbierta = false;
    bool corrutinaEnCurso = false;

    private void OnTriggerEnter(Collider other)
    {
        
        if (other.gameObject.CompareTag("Ladron"))
        {
            
            if(other.gameObject.GetComponent<Ladron>().getEstadoActual() != "Atrapado" && other.gameObject.GetComponent<Ladron>().getEstadoActual() != "Encerrado") 
            {
                
                jaulaAbierta = true;
                if (corrutinaEnCurso)
                {
                    StopCoroutine(CerrarJaulaConDelay());
                }
                StartCoroutine(CerrarJaulaConDelay());
            }
        }

        if (other.gameObject.CompareTag("Agente"))
        {
            CerrarJaula();
        }
    }   
    // CUIDAO SI ENTRA EL POLICIA CON EL LADRON TIENE QUE CERRAR LA JAULA

    public bool getJaulaAbierta()
    {
        return jaulaAbierta;
    }

    IEnumerator CerrarJaulaConDelay() 
    {
        corrutinaEnCurso = true;
        yield return new WaitForSeconds(3);

        jaulaAbierta = false;
        corrutinaEnCurso = false;
    }

    public void CerrarJaula()
    {
        jaulaAbierta = false;
        corrutinaEnCurso =false; 
    }
    
}
