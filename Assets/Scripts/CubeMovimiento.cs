using UnityEngine;

public class CuboMovimiento : MonoBehaviour
{
    public enum DireccionMovimiento { Horizontal, Vertical }
    public DireccionMovimiento direccion = DireccionMovimiento.Horizontal;

    [Tooltip("Distancia máxima del recorrido (en unidades del mundo)")]
    public float distancia = 3f;

    [Tooltip("Velocidad del movimiento del cubo")]
    public float velocidad = 2f;

    private Vector3 posicionInicial;

    void Start()
    {
        posicionInicial = transform.position;
    }

    void Update()
    {
        // Calcula un movimiento oscilante usando seno para ida y vuelta suave
        float desplazamiento = Mathf.Sin(Time.time * velocidad) * distancia;

        if (direccion == DireccionMovimiento.Horizontal)
        {
            transform.position = posicionInicial + new Vector3(desplazamiento, 0, 0);
        }
        else if (direccion == DireccionMovimiento.Vertical)
        {
            transform.position = posicionInicial + new Vector3(0, desplazamiento, 0);
        }
    }
}
