using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SpeechRecognitionAPI.Util
{
    internal class UnityMainThreadDispatcher : MonoBehaviour
    {
        private static readonly Queue<Action> _executionQueue = new Queue<Action>();

        void Start()
        {
            Plugin.mls.LogInfo("SpeechRecognitionAPI Dispatcher created");
        }

        public void Update()
        {
            lock (_executionQueue)
            {
                while (_executionQueue.Count > 0)
                {
                    var action = _executionQueue.Dequeue();
                    if (Plugin.logging.Value) Plugin.mls.LogDebug("[Dispatcher] Invoking action.");
                    action?.Invoke();
                }
            }
        }

        internal static void Enqueue(Action action)
        {
            if (action == null)
                return;

            lock (_executionQueue)
            {
                _executionQueue.Enqueue(action);
            }
        }
    }
}
