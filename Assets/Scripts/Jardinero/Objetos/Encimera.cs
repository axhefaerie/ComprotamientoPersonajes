using System.Collections.Generic;
using UnityEngine;

public class Encimera : MonoBehaviour
{
    [SerializeField] private Transform[] huecos;        // Huecos para dejar las flores
    [SerializeField] private Vector3 escalaAlDepositar = Vector3.one;       // Escala del sprite de las flores al dejarlas en la encimera

    // Se ha añadido un tiempo de desaparicion de las flores de la encimera, simulando que otro personaje las piulla
    // ASí el jardinero trabaja eternamente esclavizado
    [SerializeField] private float tiempoAntesDeRetirar = 20f;
    
    // Clase que guarda las varibables de la flor en la enciemra
    private class FlorEnEncimera
    {
        public GameObject objeto;       // El objeto flor
        public int hueco;               // El hueco en el que se encuetnra de la encimera
        public float tiempoRestante;    // El tiempo restante a su desaparición
    }
    private readonly List<FlorEnEncimera> flores = new List<FlorEnEncimera>();  // Guarda las flores de la enciemra
   
    public bool HayHueco => flores.Count < huecos.Length;       // Devuelve true si noestá lleno huecos
    public int FloresEnEncimera => flores.Count;            // Número de flores en la encimera

    // Método qeu busca el primer huecoo libre que haya en la encimera
    private int PrimerHuecoLibre()
    {
        for (int h = 0; h < huecos.Length; h++)
        {
            bool ocupado = false;
            foreach (var f in flores)
            {
                if (f.hueco == h) { ocupado = true; break; }
            }
            if (!ocupado) return h;
        }
        return -1;
    }

    private void Update()
    {
        // Recorremos hacia atrás para eliminar las primeras flores
        for (int i = flores.Count - 1; i >= 0; i--)
        {
            flores[i].tiempoRestante -= Time.deltaTime;
            if (flores[i].tiempoRestante > 0f) continue;

            if (flores[i].objeto != null) Destroy(flores[i].objeto);
            flores.RemoveAt(i);   // el hueco queda libre automáticamente
        }
    }

    // Método que deja la flor en la encimera
    public void Depositar(GameObject flor)
    {
        if (flor == null || !HayHueco) return;

        int hueco = PrimerHuecoLibre();
        if (hueco < 0) return;

        Transform destino = huecos[hueco];
        flor.transform.SetParent(destino, false);
        flor.transform.localPosition = Vector3.zero;
        flor.transform.localRotation = Quaternion.identity;
        flor.transform.localScale = escalaAlDepositar;

        // flor por delante de la encimera
        SpriteRenderer spriteFlor = flor.GetComponentInChildren<SpriteRenderer>();
        if (spriteFlor != null)
            spriteFlor.sortingOrder = 1;

        flores.Add(new FlorEnEncimera
        {
            objeto = flor,
            hueco = hueco,
            tiempoRestante = tiempoAntesDeRetirar
        });
    }
}
