using UnityEngine;

public class Maceta : MonoBehaviour
{
    public static event System.Action<Maceta> OnNecesitaAtencion;

    // Variables de las macetas
    [SerializeField] private float tiempoCrecimiento = 40f;            // tiempo qeu tarda en crecer
    [SerializeField] private int riegosNecesarios = 3;         // Veces que necesita ser regada para crecer    
    [SerializeField] private GameObject prefabFlor;            // Prefab de la flor
    [SerializeField] private Transform puntoFlor;              // Punto de aparición de la flor
    [SerializeField] private float tiempoParaSecarse = 40f;          // Tiempo que tarda en volver a pedir agua
    [SerializeField] private float tiempoEntreRiegos = 30f; // Tiempo de espera entre riegos

    // Variables para el bocadillo que dice al jugador que está pasando con la maceta
    [SerializeField] private SpriteRenderer bocadilloRenderer;
    [SerializeField] private Sprite spriteFaltaSemilla;
    [SerializeField] private Sprite spriteFaltaAgua;
    [SerializeField] private Sprite spriteFaltaEsperar;

    [SerializeField] private GameObject destellosObjeto;
    [SerializeField] private Animator destellosAnimator;

    private int riegosRecibidos;       // Cuenta los riegos recibidos
     private float sequedad;            // 
     private bool tieneFlor;            // Devuelve si ha crecido la flor entera
     private bool creciendo;            // Devuelve si est  á creciendo
     private float cronometro;          // Cronometro
     private GameObject florInstanciada;        // Flor
    private float tiempoUltimoRiego; // Tiempo que ha pasado desde el último riego

    public bool EstaVacia => !tieneFlor && !creciendo;      // Devuelve si la maceta está vacía (si no tiene ninguna flor y si no hay ninguna planta creciendo)
    public bool NecesitaAgua => creciendo && riegosRecibidos < riegosNecesarios;
    public bool PuedeRegarse => NecesitaAgua && Time.time - tiempoUltimoRiego >= tiempoEntreRiegos;// Devuelve si necesita agua (si está creciendo y si ha recibido menos riegos de lo nececsario)
    public bool TieneFlorLista => tieneFlor;    // Devuelve si tiene la flor lista para recoger

    // PAra el slider
    public bool MostrarBarra => creciendo && !PuedeRegarse;
    public bool EnFaseDeRiegos => creciendo && riegosRecibidos < riegosNecesarios;
    private void Start()
    {
        if (destellosObjeto != null) destellosObjeto.SetActive(false);
        ActualizarBocadillo();
    }

    private void Update()
    {
        if (!creciendo)
        {
            ActualizarBocadillo();
            return;
        }

        // Si ya tiene todos los riegos, crece
        if (riegosRecibidos >= riegosNecesarios)
        {
            cronometro += Time.deltaTime;
            if (cronometro >= tiempoCrecimiento) Crecer();
            ActualizarBocadillo();
            return;
        }

        // Si le falta agua, se va secando
        sequedad += Time.deltaTime;
        if (sequedad >= tiempoParaSecarse)
        {
            sequedad = 0f;
            OnNecesitaAtencion?.Invoke(this);
        }

        ActualizarBocadillo();
    }

    // Método que planta la semilla para que empiece a crecer una planta
    public void PlantarSemilla()
    {
        if (!EstaVacia) return;
        creciendo = true;
        riegosRecibidos = 0;
        cronometro = 0f;
        sequedad = 0f;
        tiempoUltimoRiego = -tiempoEntreRiegos;

        if (destellosObjeto != null) destellosObjeto.SetActive(false);
        ActualizarBocadillo();
        OnNecesitaAtencion?.Invoke(this);
    }

    // Método que riega la planta 
    public bool Regar()
    {
        if (!NecesitaAgua) return false;
        if (Time.time - tiempoUltimoRiego < tiempoEntreRiegos) 
            return false;  // Evita regar demasiado rápido
        
        riegosRecibidos++;
        sequedad = 0f;
        tiempoUltimoRiego = Time.time;

        ActualizarBocadillo();
        return true;
    }

    // Método que hace crecer del todo (florecer) la planta
    private void Crecer()
    {
        creciendo = false;
        tieneFlor = true;
        if (prefabFlor != null)
            florInstanciada = Instantiate(prefabFlor, puntoFlor.position, puntoFlor.rotation);

        if (destellosObjeto != null)
        {
            destellosObjeto.SetActive(true);
            if (destellosAnimator != null)
                destellosAnimator.Play(0, -1, 0f); // Reinicia la animacion desde el principio
        }

        ActualizarBocadillo();
    }

    // Método que le quita la flor a la planta
    public void RecogerFlor()
    {
        tieneFlor = false;
        riegosRecibidos = 0;
        cronometro = 0f;

        if (destellosObjeto != null) 
            destellosObjeto.SetActive(false);

        ActualizarBocadillo();
    }

    // Quita el sprite de la flor
    public GameObject SacarFlorVisual(Transform padre)
    {
        if (florInstanciada == null) return null;
        florInstanciada.transform.SetParent(padre, false);
        florInstanciada.transform.localPosition = Vector3.zero;
        var f = florInstanciada;
        florInstanciada = null;
        return f;
    }

    // Actualiza el bocadillo para mostrar el estado de la maceta
    private void ActualizarBocadillo()
    {
        if (bocadilloRenderer == null) return;

        if (EstaVacia && spriteFaltaSemilla != null)
        {
            // No tiene ninguna semilla
            bocadilloRenderer.sprite = spriteFaltaSemilla;
            bocadilloRenderer.enabled = true;
        }
        else if (PuedeRegarse && spriteFaltaAgua != null)
        {
            // Necesita agua y ya puede recibir otro riego
            bocadilloRenderer.sprite = spriteFaltaAgua;
            bocadilloRenderer.enabled = true;
        }
        else
        {
            // No necesita mostrar ningún bocadillo mientras espera entre riegos/crece flor o con flor
            bocadilloRenderer.enabled = false;
        }
    }

    // Método para el slider, para indicar su progreso de secado e indicar cuando hace falta volver a regarlo
    public float ProgresoSecado
    {
        get
        {
            if (!creciendo || riegosRecibidos >= riegosNecesarios) return 0f;
            return Mathf.Clamp01(sequedad / tiempoParaSecarse);
        }
    }

    // Método para el slider pero para indicar cuanto falta para el crecimiento de la flor una vez regada las veces necesarias
    public float ProgresoCrecimiento
    {
        get
        {
            if (!creciendo || riegosRecibidos < riegosNecesarios) return 0f;
            return Mathf.Clamp01(cronometro / tiempoCrecimiento);
        }
    }

    // Método que indica cual es el progreso actual de la maceta (si está creciendo o si necesita regados todavia)
    public float ProgresoActual
    {
        get
        {
            if (EstaVacia || tieneFlor) return 0f;
            return riegosRecibidos >= riegosNecesarios ? ProgresoCrecimiento : ProgresoSecado;
        }
    }
}
