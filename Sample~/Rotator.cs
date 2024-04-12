using UnityEngine;

namespace AStar.Sample.Recorder
{
    public class Rotator : MonoBehaviour
    {
        public Vector3 Axis;
        public float Speed;
        public Space Space;

        private void Update()
        {
            transform.Rotate(Axis, Speed * Time.deltaTime, Space);
        }
    }
}