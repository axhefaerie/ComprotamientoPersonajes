using System.Collections.Generic;
using UnityEngine;

public class Jardinero : MonoBehaviour
{
    // Referencias a objetos de la escena
    [SerializeField] private Regadera regadera;
    [SerializeField] private Encimera encimera;
    [SerializeField] private Transform puntoMano;

    // Movimiento del Jardinero
    [SerializeField] private float velocidad = 2f;
    [SerializeField] private float velocidadGiro = 180f;
    [SerializeField] private float distanciaLlegada = 0.15f;

    // Duración que le lleva hacer las distintas acciones (por simplificar se va a usar solo 1 tiempo común aunruqe podrían usarse varios distintos)
    [SerializeField] private float duracionAccion = 1f;

    // Estado del ciclo actual (se escriben y leen dentro del script)
    private PercepcionJardinero percepcion;
    private Maceta macetaObjetivo;

    private Maceta macetaParaRegar;
    private Maceta macetaParaReplantar;
    private Maceta macetaParaRecoger;

    private Transform puntoObjetivo;
    private GameObject florEnMano;
    private readonly List<Maceta> macetasPendientes = new List<Maceta>();

    // Temporizador para acciones que duran un rato (para que no se solapen o no entre en bucles raros)
    private float temporizador;
    private bool ocupado;

    public bool LlevaFlor => florEnMano != null;

    private void Awake()
    {
        percepcion = GetComponent<PercepcionJardinero>();
        Debug.Log($"Jardinero Awake · percepcion = {(percepcion != null ? "OK" : "NULL")}");
    }

    private void Start()
    {
        var m = percepcion.BuscarMaceta(x => true);
        Debug.Log($"Maceta más cercana: {(m != null ? m.name : "NINGUNA")}");
        var g = percepcion.BuscarZona(TipoZona.Grifo);
        Debug.Log($"Grifo: {(g != null ? g.name : "NINGUNO")}");
    }

    private void Update()
    {
        if (puntoObjetivo != null && !ocupado)
            Avanzar();
    }

    // PERCEPCIONES
    // Busca la maceta más cercana que tenga semilla (esté creciendo algo) y le falte riego
    public bool HayMacetaSeca()
    {
        if (ocupado) return false;      // Si tiene una flor en la mano, no puede hacer otra acción
        macetaParaRegar = percepcion.BuscarMaceta(m => m.NecesitaAgua);
        Debug.Log($"HayMacetaVacia → {(macetaObjetivo != null ? macetaObjetivo.name : "ninguna")}");
        // Devuelve true para disparar el estado de ir a regar (solo si también hay agua en la regadera)
        return macetaParaRegar != null && regadera.TieneAgua;       
    }

    // Busca la maceta más cercana vacía para replantar
    public bool HayMacetaVacia()
    {
        if (ocupado) return false;
        macetaParaReplantar = percepcion.BuscarMaceta(m => m.EstaVacia);     // La maceta tiene que estar vacía
        Debug.Log($"HayMacetaVacia → {(macetaObjetivo != null ? macetaObjetivo.name : "ninguna")}");
        // Devuelve true para ir a plantar
        return macetaParaReplantar != null && !LlevaFlor;        
    }

    // Busca la maceta más cercana con la flor lista (para recoger)
    public bool HayFlorLista()
    {
        if (ocupado) return false;
        macetaParaRecoger = percepcion.BuscarMaceta(m => m.TieneFlorLista);
        Debug.Log($"HayMacetaVacia → {(macetaObjetivo != null ? macetaObjetivo.name : "ninguna")}");
        // Devuelve true para ir a recoger la flor (comprueba que no lleve flor para evitar que se recojan 2 veces o pase algo extraño)
        return macetaParaRecoger != null && !LlevaFlor;
    }

    // Comprueba que la regadera está vacía para ir a recargar
    public bool SinAgua()
    {
        if (ocupado) return false;
        // Si la regadera no tiene agua y hay zona de recarga, se va a rellenar
        bool resultado = !regadera.TieneAgua && percepcion.BuscarZona(TipoZona.Grifo) != null;
        Debug.Log($"SinAgua → {resultado}");
        return resultado;
    }

    // Comprueba si se puede depositar la flor
    public bool LlevaFlorYHayHueco()
    {
        if (ocupado) return false;
        return LlevaFlor && encimera.HayHueco;
    }

    // Comprueba que se ha llegado al destino
    public bool EnDestino()
    {
        if (puntoObjetivo == null) return true;     // Si no hay punto objetivo, ddevuelve true
        Vector2 posActual = transform.position;
        Vector2 posDestino = puntoObjetivo.position;
        return Vector2.Distance(posActual, posDestino) < distanciaLlegada;
    }

    // Devuelve true cuando se ha terminado una acción
    public bool AccionTerminada()
    {
        if (!ocupado) return true;
        temporizador -= Time.deltaTime;
        if (temporizador <= 0f)
        {
            ocupado = false;
            return true;
        }
        return false;
    }

    // ACCIONES    
    public void IrARegar()
    {
        if (macetaParaRegar == null) return;
        puntoObjetivo = macetaParaRegar.GetComponent<ZonaInteraccion>().PuntoUso;
        Avanzar();
    }

    public void Regar()
    {
        if (ocupado) return;

        if (macetaParaRegar != null && regadera.GastarUna())
            macetaParaRegar.Regar();

        IniciarAccion(duracionAccion);
    }

    public void IrARecargar()
    {
        var grifo = percepcion.BuscarZona(TipoZona.Grifo);
        if (grifo != null) puntoObjetivo = grifo.PuntoUso;
        Avanzar();
    }

    public void Recargar()
    {
        if (ocupado) return;

        regadera.Recargar();

        IniciarAccion(duracionAccion);
    }

    public void IrARecoger()
    {
        if (macetaParaRecoger == null) return;
        puntoObjetivo = macetaParaRecoger.GetComponent<ZonaInteraccion>().PuntoUso;
        Avanzar();
    }

    public void Recoger()
    {
        if (ocupado) return;

        if (macetaParaRecoger == null) return;
        macetaParaRecoger.RecogerFlor();
        florEnMano = macetaParaRecoger.SacarFlorVisual(puntoMano);
        IniciarAccion(duracionAccion);
    }

    public void IrAEncimera()
    {
        puntoObjetivo = encimera.transform;
        Avanzar();
    }

    public void Depositar()
    {
        if (ocupado) return;

        if (florEnMano == null) return;
        encimera.Depositar(florEnMano);
        florEnMano = null;
        IniciarAccion(duracionAccion);

        puntoObjetivo = null;
        macetaObjetivo = null;
    }

    public void IrAReplantar()
    {
        if (macetaParaReplantar == null) return;
        puntoObjetivo = macetaParaReplantar.GetComponent<ZonaInteraccion>().PuntoUso;
        Avanzar();
    }

    public void Replantar()
    {
        if (ocupado) return;

        macetaParaReplantar?.PlantarSemilla();
        IniciarAccion(duracionAccion);
    }

    public void LimpiarObjetivo()
    {
        Debug.Log("¡El grafo está en Prueba!");
        macetaObjetivo = null;
        puntoObjetivo = null;
    }

    public void Nada() { Debug.Log("estoy en Buscar"); }

    // INTERNO
    private void IniciarAccion(float duracion)
    {
        ocupado = true;
        temporizador = duracion;
    }

    private void Avanzar()
    {
        if (puntoObjetivo == null) return;

        Vector3 destino = new Vector3(puntoObjetivo.position.x, puntoObjetivo.position.y, transform.position.z);
        Vector3 dir = destino - transform.position;

        if (dir.sqrMagnitude > 0.0001f)
        {
            transform.position = Vector3.MoveTowards(transform.position, destino, velocidad * Time.deltaTime);

            // Si giráis el transform en Z (asumiendo que "arriba" / transform.up es el frente del sprite):
            float angulo = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            Quaternion rotacionDeseada = Quaternion.Euler(0f, 0f, angulo);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotacionDeseada, velocidadGiro * Time.deltaTime);
        }

        /*
        Vector3 destino = puntoObjetivo.position;
        destino.y = transform.position.y;

        transform.position = Vector3.MoveTowards(
            transform.position, destino, velocidad * Time.deltaTime);

        Vector3 dir = destino - transform.position;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, Quaternion.LookRotation(dir),
                velocidadGiro * Time.deltaTime);
        */
    }

    public void RegistrarMacetaPendiente(Maceta maceta)
    {
        if (maceta != null && !macetasPendientes.Contains(maceta))
            macetasPendientes.Add(maceta);
    }

    public void QuitarMacetaPendiente(Maceta maceta)
    {
        macetasPendientes.Remove(maceta);
    }

}
