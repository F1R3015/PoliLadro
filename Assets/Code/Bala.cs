using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bala : MonoBehaviour
{

    [SerializeField]
    float tiempoDesaparecer;
    [SerializeField]  float velocidad;
    [SerializeField] Material material;
    LineRenderer lineRenderer;
    // Start is called before the first frame update
    void Start() 
    {
        Destroy(gameObject, tiempoDesaparecer);
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startColor = Color.black;
        lineRenderer.endColor = Color.black;
        lineRenderer.material = material;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * velocidad * Time.deltaTime);
        List<Vector3> pos = new List<Vector3>
        {
            transform.position,
            transform.position + transform.TransformDirection(Vector3.back) * 1.5f
        };
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.SetPositions(pos.ToArray());
    }



    

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }
}
