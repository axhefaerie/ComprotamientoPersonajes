using UnityEngine;

public class Regadera : MonoBehaviour
{
    [SerializeField] private int capacidadMaxima = 5;       // Capacidad máxima de la regadera
    [SerializeField] private int usosIniciales = 5;          // Capacidad inicial de la regadera 

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite spriteVacia;
    [SerializeField] private Sprite spriteMedioVacia;
    [SerializeField] private Sprite spriteMedioLlena;
    [SerializeField] private Sprite spriteLlena;

    private int usosRestantes;  // Contador de usos restantes (hacia atras)
    public bool TieneAgua => usosRestantes > 0;     // Indicador de si queda agua en la regadera
    public int UsosRestantes => usosRestantes;      // Usos de la regadera

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        usosRestantes = Mathf.Clamp(usosIniciales, 0, capacidadMaxima);     // Caalcula los usos restantes
        ActualizarVisual();
    }

    // Método que gasta un uso de la regadera (riega)
    public bool GastarUna()
    {
        if (usosRestantes <= 0) return false;
        usosRestantes--;
        ActualizarVisual();
        return true;
    }

    // Método que rellena la regadera a su capacidad inicial
    public void Recargar()
    {
        usosRestantes = capacidadMaxima;
        ActualizarVisual();
    }

    public void EstablecerVisible(bool visible)
    {
        if (spriteRenderer != null)
            spriteRenderer.enabled = visible;
    }

    public void EstablecerFlipX(bool flipX)
    {
        if (spriteRenderer != null)
            spriteRenderer.flipX = flipX;
    }

    // Método que actualiza visualmente la regadera para mostrar su capacidad de agua
    private void ActualizarVisual()
    {
        if (spriteRenderer == null) return;

        // Si no queda agua en la regadera, se pone el sprite vacío
        if (usosRestantes <= 0)
        {
            if (spriteVacia != null) spriteRenderer.sprite = spriteVacia;
            return;
        }

        // Si está llena, se actualiza el sprite
        if (usosRestantes >= capacidadMaxima)
        {
            if (spriteLlena != null) spriteRenderer.sprite = spriteLlena;
            return;
        }

        // En base a lo que quede en la regadera, se va poniendo el sprite acorde
        float proporcion = (float)usosRestantes / Mathf.Max(1, capacidadMaxima);
        if (proporcion <= 0.4f)
        {
            if (spriteMedioVacia != null) spriteRenderer.sprite = spriteMedioVacia;
        }
        else
        {
            if (spriteMedioLlena != null) spriteRenderer.sprite = spriteMedioLlena;
        }
    }
}
