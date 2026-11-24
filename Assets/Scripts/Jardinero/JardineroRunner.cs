using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.UnityToolkit;
using BehaviourAPI.UnityToolkit.GUIDesigner.Runtime;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.UnityToolkit.Demos
{
    public class JardineroRunner : EditorBehaviourRunner
    {
        private Jardinero jardinero;
        private PushPerception _reiniciar;

        private PushPerception _atencion;

        private bool _arrancado;

        protected override void Init()
        {
            Debug.Log("Init del runner");
            jardinero = GetComponent<Jardinero>();
            Debug.Log($"jardinero = {(jardinero != null ? "OK" : "NULL")}");
            Maceta.OnNecesitaAtencion += AlNecesitarAtencion;
            base.Init();
        }

        protected override void OnUpdated()
        {
            if (!_arrancado && _atencion != null)
            {
                _arrancado = true;
                Debug.Log("Disparo atencion");
                _atencion.Fire();
            }

            if (_reiniciar != null && Input.GetKeyDown(KeyCode.R))
                _reiniciar.Fire();

            base.OnUpdated();
        }

        private void OnDestroy()
        {
            Maceta.OnNecesitaAtencion -= AlNecesitarAtencion;
        }

        private void AlNecesitarAtencion(Maceta maceta)
        {
            Debug.Log($"Aviso de {maceta.name} · push = {(_atencion != null ? "OK" : "NULL")}");
            jardinero.RegistrarMacetaPendiente(maceta);
            _atencion?.Fire();
        }

        protected override void ModifyGraphs( Dictionary<string, BehaviourGraph> graphMap, Dictionary<string, PushPerception> pushPerceptionMap)
        {
            if (pushPerceptionMap.TryGetValue("reiniciar", out var p)) _reiniciar = p;
            if (pushPerceptionMap.TryGetValue("atencion", out var a)) _atencion = a;

            Debug.Log($"Pushes asignadas · atencion = {(_atencion != null ? "OK" : "NULL")}");
        }
    }
}
