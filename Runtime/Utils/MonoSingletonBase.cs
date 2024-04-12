using System;
using UnityEngine;

namespace AStar.Recorder
{
    public abstract class MonoSingletonBase<T> : MonoBehaviour where T : MonoSingletonBase<T>
    {
        protected static T m_Instance;

        public static T Instance
        {
            get
            {
                if (m_Instance != null) return m_Instance;
                m_Instance = MonoBehaviourExtensions.Generate<T>();
                return m_Instance;
            }
        }
    }
}