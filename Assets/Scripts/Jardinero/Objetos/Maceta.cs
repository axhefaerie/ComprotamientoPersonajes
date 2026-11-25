using UnityEngine;

public class Maceta : MonoBehaviour
{
    public static event System.Action<Maceta> OnNecesitaAtencion;

    [SerializeField] private float tiempoCrecimiento = 40f;            // tiempo qeu tarda en crecer
    [SerializeField] private int riegosNecesarios = 3;         // Veces que necesita ser regada para crecer    
    [SerializeField] private GameObject prefabFlor;            // Prefab de la flor
    [SerializeField] private Transform puntoFlor;              // Punto de aparición de la flor
    [SerializeField] private float tiempoParaSecarse = 15f;          // Tiempo que tarda en volver a pedir agua

    [Header("Bocadillos de estado")]
    [SerializeField] private SpriteRenderer bocadilloRenderer;
    [SerializeField] private Sprite spriteFaltaSemilla;
    [SerializeField] private Sprite spriteFaltaAgua;

    [Header("Destellos al florecer")]
    [SerializeField] private GameObject destellosObjeto;
    [SerializeField] private Animator destellosAnimator;

    private int riegosRecibidos;       // Cuenta los riegos recibidos
     private float sequedad;            // 
     private bool tieneFlor;            // Devuelve si ha crecido la flor entera
     private bool creciendo;            // Devuelve si est  á creciendo
     private float cronometro;          // Cronometro
     private GameObject florInstanciada;        // Flor


    public bool EstaVacia => !tieneFlor && !creciendo;      // Devuelve si la maceta está vacía (si no tiene ninguna flor y si no hay ninguna planta creciendo)
    public bool NecesitaAgua => creciendo && riegosRecibidos < riegosNecesarios;        // Devuelve si necesita agua (si está creciendo y si ha recibido menos riegos de lo nececsario)
    public bool TieneFlorLista => tieneFlor;    // Devuelve si tiene la flor lista para recoger

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
            if (riegosRecibidos > 0) riegosRecibidos--;   // pierde un riego
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

        if (destellosObjeto != null) destellosObjeto.SetActive(false);
        ActualizarBocadillo();
        OnNecesitaAtencion?.Invoke(this);
    }

    // Método que riega la planta 
    public bool Regar()
    {
        if (!NecesitaAgua) return false;
        riegosRecibidos++;
        sequedad = 0f;

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

    private void ActualizarBocadillo()
    {
        if (bocadilloRenderer == null) return;

        if (EstaVacia && spriteFaltaSemilla != null)
        {
            bocadilloRenderer.sprite = spriteFaltaSemilla;
            bocadilloRenderer.enabled = true;
        }
        else if (NecesitaAgua && spriteFaltaAgua != null)
        {
            bocadilloRenderer.sprite = spriteFaltaAgua;
            bocadilloRenderer.enabled = true;
        }
        else
        {
            bocadilloRenderer.enabled = false;
        }
    }
}
