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
            jardinero = GetComponent<Jardinero>();
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

        // Si una maceta necesita atenbción, se guarda la maceta en la liskta de pendientes y se llama a atención pusch
        private void AlNecesitarAtencion(Maceta maceta)
        {
            jardinero.RegistrarMacetaPendiente(maceta);
            _atencion?.Fire();
        }

        // Añade las percepciones push
        protected override void ModifyGraphs( Dictionary<string, BehaviourGraph> graphMap, Dictionary<string, PushPerception> pushPerceptionMap)
        {
            if (pushPerceptionMap.TryGetValue("reiniciar", out var p)) _reiniciar = p;
            if (pushPerceptionMap.TryGetValue("atencion", out var a)) _atencion = a;
        }
    }
}
