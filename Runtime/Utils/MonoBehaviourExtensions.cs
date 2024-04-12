using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AStar.Recorder
{
    public static class MonoBehaviourExtensions
    {
        public static T Generate<T>() where T : MonoBehaviour
        {
            GameObject go = new GameObject(typeof(T).Name);
            T component = go.AddComponent<T>();
            if (component == null)
            {
                Object.Destroy(go);
                throw new Exception($"Failed to create instance of {typeof(T).Name}");
            }
            Object.DontDestroyOnLoad(go);
            return component;
        }
    }
}