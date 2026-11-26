using System.Collections.Generic;
using UnityEngine;

public class Jardinero : MonoBehaviour
{
    // Referencias a objetos de la escena
    [SerializeField] private Regadera regadera;
    [SerializeField] private Encimera encimera;
    [SerializeField] private Transform puntoMano;
    [SerializeField] private Vector3 escalaFlorEnMano = Vector3.one;

    // Movimiento del Jardinero
    [SerializeField] private float velocidad = 2f;
    [SerializeField] private bool spriteMiraDerechaPorDefecto = true;
    [SerializeField] private float distanciaLlegada = 0.15f;

    // Duración que le lleva hacer las distintas acciones (por simplificar se va a usar solo 1 tiempo común aunruqe podrían usarse varios distintos)
    [SerializeField] private float duracionAccion = 1f;

    // Estado del ciclo actual (se escriben y leen dentro del script)
    private SpriteRenderer spriteRenderer;
    private PercepcionJardinero percepcion;

    // Variables que guardan la maceta más cercana para regar/replantar/recoger
    private Maceta macetaParaRegar;
    private Maceta macetaParaReplantar;
    private Maceta macetaParaRecoger;

    private Transform puntoObjetivo;    // Punto al que debe ir el jardinero
    private GameObject florEnMano;      // Si tiene flor en mano
    private readonly List<Maceta> macetasPendientes = new List<Maceta>();       // Lista de macetas que tienen necesidades

    // Temporizador para acciones que duran un rato (para que no se solapen o no entre en bucles raros)
    private float temporizador;
    private bool ocupado;
    private bool ocultarRegadera;

    public bool LlevaFlor => florEnMano != null;

    private void Awake()  //Debug de los diferentes errores que pueden suceder
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        percepcion = GetComponent<PercepcionJardinero>();
        ActualizarVisibilidadRegadera();
    }

    private void Start()
    {
        var m = percepcion.BuscarMaceta(x => true);
        var g = percepcion.BuscarZona(TipoZona.Grifo);
    }

    private void Update()
    {
        // Si está realizando una acción, actualizar su temporizador
        if (ocupado)
        {
            temporizador -= Time.deltaTime;

            // Comprobar si ha terminado la acción
            if (temporizador <= 0f)
            {
                temporizador = 0f;
                ocupado = false;

                if (!LlevaFlor)
                    ocultarRegadera = false;

                ActualizarVisibilidadRegadera();
            }
            return;
        }

        // Si no está ocupado, puede desplazarse
        if (puntoObjetivo != null)
            Avanzar();  
    }

    // PERCEPCIONES
    // Busca la maceta más cercana que tenga semilla (esté creciendo algo) y le falte riego
    public bool HayMacetaSeca()
    {
        if (ocupado) return false;  // Si está haciendo algo más o tiene flor en mano no puede realizar otra acción
        // Buscar la maceta más cercana que necesite agua
        macetaParaRegar = percepcion.BuscarMaceta(m => m.PuedeRegarse);
        // Comprobar que existe una maceta y que hay agua
        return macetaParaRegar != null && regadera.TieneAgua;
    }

    // Busca la maceta más cercana vacía para replantar
    public bool HayMacetaVacia()
    {
        if (ocupado) return false;
        // Busca la maceta más cercana que esté vacía
        macetaParaReplantar = percepcion.BuscarMaceta(m => m.EstaVacia);     
        // Devuelve true para ir a plantar
        return macetaParaReplantar != null && !LlevaFlor;
    }

    // Busca la maceta más cercana con la flor lista (para recoger)
    public bool HayFlorLista()
    {
        if (ocupado) return false;
        if (LlevaFlor) return false;    // Si ya lleva una
        if (!encimera.HayHueco) return false; // si no hay sitio en la encimera, no pilla la flor para seguir haciendo otras tareas

        // Busca la maceta más cercana que ya tenga la flor lista para ir a recoger
        macetaParaRecoger = percepcion.BuscarMaceta(m => m.TieneFlorLista);
        // Devuelve true para ir a recoger la flor (comprueba que no lleve flor para evitar que se recojan 2 veces o pase algo extraño)
        return macetaParaRecoger != null && !LlevaFlor;
    }

    // Comprueba que la regadera está vacía para ir a recargar
    public bool SinAgua()
    {
        if (ocupado) return false;
        // Si la regadera no tiene agua y hay zona de recarga, se va a rellenar
        return !regadera.TieneAgua && percepcion.BuscarZona(TipoZona.Grifo) != null; 
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
        // Si no hay destino, se puede considerar que está en el objetivo
        if (puntoObjetivo == null)
            return true;

        Vector2 posActual = transform.position;
        Vector2 posDestino = puntoObjetivo.position;

        float distancia = Vector2.Distance(posActual, posDestino);

        return distancia <= distanciaLlegada;
    }

    // Devuelve true cuando se ha terminado una acción
    public bool AccionTerminada()
    {
        return !ocupado;
    }

    // ACCIONES  
    // Va a regar
    public void IrARegar()
    {
        if (ocupado) return;

        ocultarRegadera = false;
        ActualizarVisibilidadRegadera();

        // Comprobar que existe una maceta objetivo
        if (macetaParaRegar == null) return;

        puntoObjetivo = macetaParaRegar.GetComponent<ZonaInteraccion>().PuntoUso;

        Avanzar();
    }

    // Cuando llega al objetivo, riega la maceta (gastando un uso de la regadera)
    public void Regar()
    {
        if (ocupado) return;
        if (macetaParaRegar == null) return;

        // Primero intentamos regar la maceta
        if (macetaParaRegar.Regar())
        {
            // Solo gastamos agua si el riego ha sido aceptado
            regadera.GastarUna();

            ocultarRegadera = false;
            ActualizarVisibilidadRegadera();

            IniciarAccion(duracionAccion);
        }
    }

    // Si la regadera se ha quedado sin usos, el jardinero va a recargarlo al grifo mas cercano
    public void IrARecargar()
    {
        if (ocupado) return;

        ocultarRegadera = false;
        ActualizarVisibilidadRegadera();

        var grifo = percepcion.BuscarZona(TipoZona.Grifo);
        if (grifo != null)
            puntoObjetivo = grifo.PuntoUso != null ? grifo.PuntoUso : grifo.transform;

        Avanzar();
    }

    // Cuando llega a la zona del grifo, recarga la regadera (añadiendo de nuevo todos sus posibles usos y actualizando su apariencia)
    public void Recargar()
    {
        if (ocupado) return;

        ocultarRegadera = false;
        ActualizarVisibilidadRegadera();

        regadera.Recargar();
        IniciarAccion(duracionAccion);
    }

    // Si una flor esta lista, el jardinero va a recogerla
    public void IrARecoger()
    {
        if (ocupado) return;

        ocultarRegadera = true;
        ActualizarVisibilidadRegadera();

        if (macetaParaRecoger == null) return;
        puntoObjetivo = macetaParaRecoger.GetComponent<ZonaInteraccion>().PuntoUso;
        
        Avanzar();
    }

    // Al llegar al punto, recoge la flor (se saca de la maceta y se actualiza su sprite en la mano del jardinero)
    public void Recoger()
    {
        if (ocupado) return;
        if (macetaParaRecoger == null) return;

        ocultarRegadera = true;
        ActualizarVisibilidadRegadera();

        macetaParaRecoger.RecogerFlor();
        florEnMano = macetaParaRecoger.SacarFlorVisual(puntoMano);

        if (florEnMano != null) // Ajustar escala
            florEnMano.transform.localScale = escalaFlorEnMano;

        IniciarAccion(duracionAccion);
    }

    // Cuando se tiene la flor, se va a la encimera
    public void IrAEncimera()
    {
        if (ocupado) return;

        ocultarRegadera = true;
        ActualizarVisibilidadRegadera();

        if (encimera != null)
            puntoObjetivo = encimera.transform;

        Avanzar();
    }

    // Al llegar a la encimera, se trata de depositar la flor en ella si hay hueco
    public void Depositar()
    {
        if (ocupado) return;
        if (florEnMano == null) return;

        ocultarRegadera = true;
        ActualizarVisibilidadRegadera();

        encimera.Depositar(florEnMano);
        florEnMano = null;

        IniciarAccion(duracionAccion);
        puntoObjetivo = null;
    }

    // Si hay una maceta vacía, el jardinero va hacia ella para replantarla
    public void IrAReplantar()
    {
        if (ocupado) return;

        ocultarRegadera = true;
        ActualizarVisibilidadRegadera();

        puntoObjetivo = macetaParaReplantar.GetComponent<ZonaInteraccion>().PuntoUso;
        Avanzar();
    }

    // Al llegar al objetivo, se replanta la maceta
    public void Replantar()
    {
        if (ocupado) return;

        ocultarRegadera = true;
        ActualizarVisibilidadRegadera();

        macetaParaReplantar.PlantarSemilla();
        IniciarAccion(duracionAccion);
    }

    // INTERNO
    // Método que le da duración a las distintasx acciones /(todas las acciones comparten un tiempo de duración común)
    private void IniciarAccion(float duracion)
    {
        ocupado = true;     // Se marca como ocupado al jardinero para que no pueda realizar ninguna otra acción miuentas
        temporizador = Mathf.Max(0f, duracion);
    }

    // Método que hjce que el jardinero avance visualmente hacia el destino
    private void Avanzar()
    {
        // Si no hay destino, no se mueve
        if (puntoObjetivo == null) return;

        Vector3 destino = new Vector3(puntoObjetivo.position.x, puntoObjetivo.position.y, transform.position.z);
        Vector3 dir = destino - transform.position;

        if (dir.sqrMagnitude > 0.0001f)
        {
            ActualizarFlipX(dir.x); // Se gira el sprite del jardinero al caminar hacia la dirección

            // Movimiento hacia el destino
            transform.position = Vector3.MoveTowards(transform.position, destino, velocidad * Time.deltaTime);
        }
    }

    // Para flipear el sprite del jardinero
    private void ActualizarFlipX(float dirX)
    {
        if (spriteRenderer == null || Mathf.Abs(dirX) <= 0.001f)
            return;

        bool vaIzquierda = dirX < 0f;
        bool flip = spriteMiraDerechaPorDefecto ? vaIzquierda : !vaIzquierda;
        spriteRenderer.flipX = flip;

        if (regadera != null)
            regadera.EstablecerFlipX(flip);
    }

    // Se registra a la lista las macetas pendientes que necesitan cosas
    public void RegistrarMacetaPendiente(Maceta maceta)
    {
        if (maceta != null && !macetasPendientes.Contains(maceta))
            macetasPendientes.Add(maceta);
    }

    // Al completar la acci´ñon, se quita la maceta de la lista
    public void QuitarMacetaPendiente(Maceta maceta)
    {
        macetasPendientes.Remove(maceta);
    }

    // Método que permite hacer visible e invisible la regadera en las acciones correspondientes
    private void ActualizarVisibilidadRegadera()
    {
        if (regadera == null) return;
        bool visible = !ocultarRegadera && !LlevaFlor;
        regadera.EstablecerVisible(visible);
    }
}
