using UnityEngine;

// Tipos de zonas que puede interactuar el jardinero
public enum TipoZona { Maceta, Grifo, Encimera }

public class ZonaInteraccion : MonoBehaviour
{
    [SerializeField] private TipoZona tipo = TipoZona.Maceta;
    [SerializeField] private Transform puntoUso;        

    public TipoZona Tipo => tipo;
    public Transform PuntoUso => puntoUso != null ? puntoUso : transform;
}
