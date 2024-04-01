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
            else if(jaulaAbierta)
            {
                CerrarJaula();
            }
        }

        if (other.gameObject.CompareTag("Agente") && jaulaAbierta)
        {
            CerrarJaula();
        }
    }   
    

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
