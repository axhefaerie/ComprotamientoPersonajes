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
    private bool ocultarRegadera;

    public bool LlevaFlor => florEnMano != null;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        percepcion = GetComponent<PercepcionJardinero>();

        if (spriteRenderer == null)
            Debug.LogError("Falta SpriteRenderer en Jardinero o sus hijos", this);

        if (percepcion == null)
            Debug.LogError("Falta PercepcionJardinero", this);

        if (regadera == null)
            Debug.LogError("Falta asignar Regadera", this);

        if (encimera == null)
            Debug.LogError("Falta asignar Encimera", this);

        if (puntoMano == null)
            Debug.LogError("Falta asignar PuntoMano", this);

        ActualizarVisibilidadRegadera();
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
                Debug.Log("Acción terminada");
            }

            return;
        }

        // Si no está ocupado, puede desplazarse
        if (puntoObjetivo != null)
        {
            Avanzar();
        }
    }

    // ================== PERCEPCIONES ==================
    // Busca la maceta más cercana que tenga semilla (esté creciendo algo) y le falte riego
    public bool HayMacetaSeca()
    {
        if (ocupado) return false;

        // Buscar la maceta más cercana que necesite agua
        macetaParaRegar = percepcion.BuscarMaceta(m => m.NecesitaAgua);

        // Comprobar que existe una maceta y que hay agua
        bool resultado = macetaParaRegar != null && regadera.TieneAgua;

        Debug.Log($"HayMacetaSeca → {(macetaParaRegar != null ? macetaParaRegar.name : "ninguna")} | Resultado: {resultado}");

        return resultado;
    }

    // Busca la maceta más cercana vacía para replantar
    public bool HayMacetaVacia()
    {
        if (ocupado) return false;

        macetaParaReplantar = percepcion.BuscarMaceta(m => m.EstaVacia);     // La maceta tiene que estar vacía
        bool resultado = macetaParaReplantar != null && !LlevaFlor;

        Debug.Log($"HayMacetaVacia → {(macetaParaRegar != null ? macetaParaRegar.name : "ninguna")}");
        // Devuelve true para ir a plantar
        return resultado;        
    }

    // Busca la maceta más cercana con la flor lista (para recoger)
    public bool HayFlorLista()
    {
        if (ocupado) return false;

        macetaParaRecoger = percepcion.BuscarMaceta(m => m.TieneFlorLista);
        bool resultado = macetaParaRecoger != null && !LlevaFlor;

        Debug.Log($"HayMacetaVacia → {(macetaParaRegar != null ? macetaParaRegar.name : "ninguna")}");
        // Devuelve true para ir a recoger la flor (comprueba que no lleve flor para evitar que se recojan 2 veces o pase algo extraño)
        return resultado;
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
        // Si no hay destino, no podemos considerar que haya llegado
        if (puntoObjetivo == null)
            return false;

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
    public void IrARegar()
    {
        if (ocupado) return;

        ocultarRegadera = false;
        ActualizarVisibilidadRegadera();

        // Comprobar que existe una maceta objetivo
        if (macetaParaRegar == null) return;

        // Comprobar que sigue necesitando agua
        if (!macetaParaRegar.NecesitaAgua)
        {
            macetaParaRegar = null;
            puntoObjetivo = null;
            return;
        }

        // Comprobar que la regadera tiene agua
        if (!regadera.TieneAgua)
        {
            macetaParaRegar = null;
            puntoObjetivo = null;
            return;
        }

        puntoObjetivo = macetaParaRegar.GetComponent<ZonaInteraccion>().PuntoUso;

        Avanzar();
    }

    public void Regar()
    {
        if (ocupado) return;

        // Comprobar que existe una maceta
        if (macetaParaRegar == null) return;

        // Comprobar que el jardinero ha llegado
        if (!EnDestino()) return;

        // Comprobar que la maceta sigue necesitando agua
        if (!macetaParaRegar.NecesitaAgua)
        {
            macetaParaRegar = null;
            puntoObjetivo = null;
            Debug.Log("La maceta ya no necesita agua");
            return;
        }

        // Comprobar que queda agua
        if (!regadera.TieneAgua)
        {
            Debug.Log("La regadera está vacía");
            return;
        }

        // Gastar agua y regar
        if (regadera.GastarUna())
        {         
            macetaParaRegar.Regar();
            ocultarRegadera = false;
            ActualizarVisibilidadRegadera();
            IniciarAccion(duracionAccion);
            Debug.Log("Maceta regada correctamente");
        }
    }

    public void IrARecargar()
    {
        if (ocupado) return;

        ocultarRegadera = false;
        ActualizarVisibilidadRegadera();

        var grifo = percepcion.BuscarZona(TipoZona.Grifo);
        if (grifo != null)
            puntoObjetivo = grifo.PuntoUso != null ? grifo.PuntoUso : grifo.transform;
        //puntoObjetivo = grifo.PuntoUso;

        Avanzar();
    }

    public void Recargar()
    {
        if (ocupado) return;
        if (regadera.TieneAgua) return;

        ocultarRegadera = false;
        ActualizarVisibilidadRegadera();

        regadera.Recargar();
        IniciarAccion(duracionAccion);
    }

    public void IrARecoger()
    {
        if (ocupado) return;

        ocultarRegadera = true;
        ActualizarVisibilidadRegadera();

        if (macetaParaRecoger == null) return;
        puntoObjetivo = macetaParaRecoger.GetComponent<ZonaInteraccion>().PuntoUso;
        
        Avanzar();
    }

    public void Recoger()
    {
        if (ocupado) return;
        if (macetaParaRecoger == null) return;
        if (!macetaParaRecoger.TieneFlorLista) return;

        ocultarRegadera = true;
        ActualizarVisibilidadRegadera();

        macetaParaRecoger.RecogerFlor();
        florEnMano = macetaParaRecoger.SacarFlorVisual(puntoMano);

        if (florEnMano != null) // Ajustar escala
            florEnMano.transform.localScale = escalaFlorEnMano;

        IniciarAccion(duracionAccion);
    }

    public void IrAEncimera()
    {
        if (ocupado) return;

        ocultarRegadera = true;
        ActualizarVisibilidadRegadera();

        if (encimera != null)
            puntoObjetivo = encimera.transform;

        Avanzar();
    }

    public void Depositar()
    {
        if (ocupado) return;
        if (florEnMano == null) return;
        if (!encimera.HayHueco) return;

        ocultarRegadera = true;
        ActualizarVisibilidadRegadera();

        encimera.Depositar(florEnMano);
        florEnMano = null;

        IniciarAccion(duracionAccion);
        puntoObjetivo = null;
        macetaObjetivo = null;
    }

    public void IrAReplantar()
    {
        if (ocupado) return;

        ocultarRegadera = true;
        ActualizarVisibilidadRegadera();

        if (macetaParaReplantar == null) return;
        puntoObjetivo = macetaParaReplantar.GetComponent<ZonaInteraccion>().PuntoUso;
        Avanzar();
    }

    public void Replantar()
    {
        if (ocupado) return;
        if (macetaParaReplantar == null) return;
        if (!macetaParaReplantar.EstaVacia) return;
        if (LlevaFlor) return;

        ocultarRegadera = true;
        ActualizarVisibilidadRegadera();

        macetaParaReplantar.PlantarSemilla();
        IniciarAccion(duracionAccion);
    }

    public void LimpiarObjetivo()
    {
        Debug.Log("¡El grafo está en Prueba!");
        macetaObjetivo = null;
        puntoObjetivo = null;
    }

    public void Nada() { Debug.Log("estoy en Buscar"); }

    // ================== INTERNO ==================
    private void IniciarAccion(float duracion)
    {
        if (ocupado) return;

        ocupado = true;
        temporizador = Mathf.Max(0f, duracion);

        Debug.Log($"Acción de {temporizador} segundos");
    }

    private void Avanzar()
    {
        // Si no hay destino, no se mueve
        if (puntoObjetivo == null)
            return;

        Vector3 destino = new Vector3(
            puntoObjetivo.position.x,
            puntoObjetivo.position.y,
            transform.position.z
        );

        // Comprobar si ya ha llegado
        if (EnDestino())
        {
            // Ajustar la posición para que coincida exactamente
            transform.position = destino;
            return;
        }

        Vector3 dir = destino - transform.position;

        if (dir.sqrMagnitude > 0.0001f)
        {
            ActualizarFlipX(dir.x);

            // Movimiento hacia el destino
            transform.position = Vector3.MoveTowards(
                transform.position,
                destino,
                velocidad * Time.deltaTime
            );
        }

        // Comprobar nuevamente si ha llegado después de moverse
        if (EnDestino())
        {
            transform.position = destino;
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

    public void RegistrarMacetaPendiente(Maceta maceta)
    {
        if (maceta != null && !macetasPendientes.Contains(maceta))
            macetasPendientes.Add(maceta);
    }

    public void QuitarMacetaPendiente(Maceta maceta)
    {
        macetasPendientes.Remove(maceta);
    }

    private void ActualizarVisibilidadRegadera()
    {
        if (regadera == null) return;
        bool visible = !ocultarRegadera && !LlevaFlor;
        regadera.EstablecerVisible(visible);
    }
}
