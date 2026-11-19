using UnityEngine;

public class Regadera : MonoBehaviour
{
    [SerializeField] private int capacidadMaxima = 3;       // Capacidad máxima de la regadera
    [SerializeField] private int usosIniciales = 3;          // Capacidad inicial de la regadera 

    private int usosRestantes;  // Contador de usos restantes (hacia atras)
    public bool TieneAgua => usosRestantes > 0;
    public int UsosRestantes => usosRestantes;

    private void Awake()
    {
        usosRestantes = Mathf.Clamp(usosIniciales, 0, capacidadMaxima);     // Ca´lcula los usos restantes
    }

    // Método que gasta un uso de la regadera (riega)
    public bool GastarUna()
    {
        if (usosRestantes <= 0) return false;
        usosRestantes--;
        return true;
    }

    // Método que rellena la regadera a su capacidad inicial
    public void Recargar()
    {
        usosRestantes = capacidadMaxima;
    }
}
