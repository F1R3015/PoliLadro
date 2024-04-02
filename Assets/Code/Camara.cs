using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camara : MonoBehaviour
{
    Vector3 movimiento;
    float rotacionX;
    float rotacionY;
    [SerializeField] float velocidadMovimiento;
    [SerializeField] float sensibilidad;
    // Start is called before the first frame update
    void Start()
    {
        movimiento = Vector3.zero;
    } 

    // Update is called once per frame
    void Update()
    {
        
        movimiento = Vector3.Normalize(transform.right * Input.GetAxis("Horizontal") + transform.forward * Input.GetAxis("Vertical"));

        rotacionY += Input.GetAxis("Mouse X") * sensibilidad;
        rotacionX += Input.GetAxis("Mouse Y") * -1 * sensibilidad;
        transform.localEulerAngles = new Vector3(rotacionX, rotacionY, 0);
        transform.position += movimiento * velocidadMovimiento * Time.deltaTime;
    }
}
