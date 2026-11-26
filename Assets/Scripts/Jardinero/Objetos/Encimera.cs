using System.Collections.Generic;
using UnityEngine;

public class Encimera : MonoBehaviour
{
    [SerializeField] private Transform[] huecos;        // Huecos para dejar las flores
    [SerializeField] private Vector3 escalaAlDepositar = Vector3.one;       // Escala del sprite de las flores al dejarlas en la encimera

    // Se ha añadido un tiempo de desaparicion de las flores de la encimera, simulando que otro personaje las piulla
    // ASí el jardinero trabaja eternamente esclavizado
    [SerializeField] private float tiempoAntesDeRetirar = 20f;

    private readonly List<GameObject> flores = new List<GameObject>();      // Guarda las flores de la enciemra
    private readonly List<float> tiempos = new List<float>();               // Guarda el tiempo restante de cada flor
    public bool HayHueco => flores.Count < huecos.Length;       // Devuelve true si noestá lleno huecos
    public int FloresEnEncimera => flores.Count;            // Número de flores en la encimera

    private void Update()
    {
        // Recorremos hacia atrás para eliminar las primeras flores
        for (int i = flores.Count - 1; i >= 0; i--)
        {
            tiempos[i] -= Time.deltaTime;
            if (tiempos[i] > 0f) continue;

            if (flores[i] != null) Destroy(flores[i]);
            flores.RemoveAt(i);
            tiempos.RemoveAt(i);
        }
    }

    // Método que deja la flor en la encimera
    public void Depositar(GameObject flor)
    {
        if (flor == null || !HayHueco) return;

        Transform hueco = huecos[flores.Count];
        flor.transform.SetParent(hueco, false);
        flor.transform.localPosition = Vector3.zero;
        flor.transform.localRotation = Quaternion.identity;
        flor.transform.localScale = escalaAlDepositar;

        // flor por delante de la encimera
        SpriteRenderer spriteFlor = flor.GetComponentInChildren<SpriteRenderer>();
        if (spriteFlor != null)
            spriteFlor.sortingOrder = 1;

        flores.Add(flor);
        tiempos.Add(tiempoAntesDeRetirar);
    }
}
