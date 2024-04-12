using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

namespace AStar.Recorder
{
    public class CoroutineManager : MonoBehaviour
    {
        private static CoroutineManager m_Instance;

        public static CoroutineManager Instance
        {
            get
            {
                if (m_Instance != null) return m_Instance;
                GameObject go = new GameObject("[VideoEncoder CoroutineManager]");
                CoroutineManager manager = go.AddComponent<CoroutineManager>();
                m_Instance = manager;
                return m_Instance;
            }
        }

        public static void WaitUntil(Func<bool> isDone, Action onExecute)
        {
            Instance.StartCoroutine(WaitUntilInternal(isDone, onExecute));
        }

        private static IEnumerator WaitUntilInternal(Func<bool> isDone, Action onExecute)
        {
            yield return new WaitUntil(isDone);
            onExecute?.Invoke();
        }
    }
}