using UnityEngine;
using System;

// Para saber que tiene q hacer

public class PercepcionJardinero : MonoBehaviour
{
    [SerializeField] private float radio = 10f;     // Radio de alcance

    private readonly Collider2D[] buffer = new Collider2D[32];

    public Maceta BuscarMaceta(Func<Maceta, bool> filtro)
    {
        Maceta mejor = null;
        float mejorDist = float.MaxValue;
        Vector2 origen = transform.position;

        int n = Physics2D.OverlapCircleNonAlloc(origen, radio, buffer);
        for (int i = 0; i < n; i++)
        {
            var maceta = buffer[i].GetComponentInParent<Maceta>();
            if (maceta == null || !filtro(maceta)) continue;

            float d = (maceta.transform.position - transform.position).sqrMagnitude;
            if (d < mejorDist) { mejorDist = d; mejor = maceta; }
        }
        return mejor;
    }

    public ZonaInteraccion BuscarZona(TipoZona tipo)
    {
        ZonaInteraccion mejor = null;
        float mejorDist = float.MaxValue;
        Vector2 origen = transform.position;

        int n = Physics2D.OverlapCircleNonAlloc(origen, radio, buffer);
        for (int i = 0; i < n; i++)
        {
            var zona = buffer[i].GetComponentInParent<ZonaInteraccion>();
            if (zona == null || zona.Tipo != tipo) continue;

            float d = (zona.transform.position - transform.position).sqrMagnitude;
            if (d < mejorDist) { mejorDist = d; mejor = zona; }
        }
        return mejor;
    }
}
