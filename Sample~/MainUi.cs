using System;
using System.IO;
using AStar.Recorder;
using UnityEngine;
using UnityEngine.UI;

namespace AStar.Sample.Recorder
{
    public class MainUi : MonoBehaviour
    {
        private IRecorder m_Recorder;
        [SerializeField] private Button m_StartButton;
        [SerializeField] private Button m_StopButton;
        [SerializeField] private InputField m_InputField;
        [SerializeField] private RenderTexture m_RenderTexture;

        private void Awake()
        {
            string root = Path.Combine(Application.persistentDataPath, "AStar.Recorder", DateTime.Now.GetFileString());
            if (!Directory.Exists(root))
                Directory.CreateDirectory(root);
            
            OfflineRecorder.Instance.Config = new OfflineRecorder.RecorderConfig
            {
                Fps = uint.Parse(m_InputField.text),
                RenderTexture = m_RenderTexture,
                CacheDirectory = root
            };
            m_Recorder = OfflineRecorder.Instance;
            m_Recorder.OutputPath = Path.Combine(root, "output.mp4");
            m_Recorder.OnEncoded += Debug.Log;

            m_StartButton.onClick.AddListener(StartCapture);
            m_StopButton.onClick.AddListener(StopCapture);
        }

        public void StartCapture() => m_Recorder.Begin();
        public void StopCapture() => m_Recorder.Stop();
    }
}