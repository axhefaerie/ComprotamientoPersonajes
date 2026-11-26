using UnityEngine;

public class SliderEstadoMaceta : MonoBehaviour
{
    [SerializeField] private Maceta maceta;

    [SerializeField] private SpriteRenderer fondo;      // el del fondo para que se vea el slider
    [SerializeField] private SpriteRenderer relleno;    // el que se va borrando
    [SerializeField] private bool mostrarSoloCuandoImporta = true;

    [SerializeField] private Color colorRiegos = new Color(0.43f, 0.102f, 0.180f);   // azul para mostrar el tiempo hasta proximo riego
    [SerializeField] private Color colorCrecimiento = new Color(0.208f, 0.102f, 0.180f); // rosa para mostrar tiuempo hasta el crecimiento

    [SerializeField] private float anchoMaximo = 1f;    // ancho del relleno a progreso = 1
    [SerializeField] private bool creceHaciaDerecha = true;

    private Vector3 escalaOriginalRelleno;
    private Vector3 posicionOriginalRelleno;

    private void Awake()
    {
        if (maceta == null) maceta = GetComponentInParent<Maceta>();

        if (relleno != null)
        {
            escalaOriginalRelleno = relleno.transform.localScale;
            posicionOriginalRelleno = relleno.transform.localPosition;
        }
    }

    private void Update()
    {
        if (maceta == null || relleno == null) return;

        bool visible = mostrarSoloCuandoImporta ? maceta.MostrarBarra : true;

        if (fondo != null) fondo.enabled = visible;
        relleno.enabled = visible;

        if (!visible) return;
        relleno.color = maceta.EnFaseDeRiegos ? colorRiegos : colorCrecimiento; // Se cambia el color del slider
        ActualizarRelleno(Mathf.Clamp01(maceta.ProgresoActual));
    }

    private void ActualizarRelleno(float progreso)
    {
        // Escala en X: 0 = nada, 1 = ancho completo
        Vector3 escala = escalaOriginalRelleno;
        escala.x = escalaOriginalRelleno.x * progreso;
        relleno.transform.localScale = escala;

        // Desplazamos para que se encoja desde el lado correcto
        // (asumiendo que el pivote del sprite está en el centro)
        float desplazamiento = (escalaOriginalRelleno.x - escala.x) * 0.5f * anchoMaximo;
        Vector3 pos = posicionOriginalRelleno;
        pos.x += creceHaciaDerecha ? -desplazamiento : desplazamiento;
        relleno.transform.localPosition = pos;
    }
}
