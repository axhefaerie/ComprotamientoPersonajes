using System.Collections.Generic;
using UnityEngine;

public class Encimera : MonoBehaviour
{
    [SerializeField] private Transform[] huecos;        // Huecos para dejar las flores
    [SerializeField] private Vector3 escalaAlDepositar = Vector3.one;       // Escala del sprite de las flores al dejarlas en la encimera

    private readonly List<GameObject> flores = new List<GameObject>();

    public bool HayHueco => flores.Count < huecos.Length;       // Devuelve true si noestá lleno huecos

    public void Depositar(GameObject flor)
    {
        if (flor == null || !HayHueco) return;

        Transform hueco = huecos[flores.Count];
        flor.transform.SetParent(hueco, false);
        flor.transform.localPosition = Vector3.zero;
        flor.transform.localRotation = Quaternion.identity;
        flor.transform.localScale = escalaAlDepositar;

        flores.Add(flor);
    }
}
