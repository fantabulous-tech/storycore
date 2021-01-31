using CoreUtils;
using StoryCore.Utils;
using UnityEngine;

namespace VRSubtitles {
    public class ShowSubtitle : MonoBehaviour {
        [SerializeField] private string m_Text;
        [SerializeField] private Sprite m_Portrait;
        [SerializeField] private float m_Delay;
        [SerializeField] private float m_Duration = -1;

        private Subtitle m_Subtitle;

        private void Awake() {
            if (string.IsNullOrEmpty(m_Text)) {
                string warning = "WARNING: " + GetScenePath(transform) + " needs it's subtitle text set properly.";
                m_Text = warning;
                Debug.LogWarning(warning, this);
            }
        }

        private void OnEnable() {
            // Delay a frame for each component above us, in case there are other 'ShowSubtitles' above.
            int priority = GetComponents<ShowSubtitle>().IndexOf(this);
            m_Subtitle = SubtitleDirector.Show(m_Text, m_Portrait, m_Delay, m_Duration, priority);
        }

        private void OnDisable() {
            m_Subtitle.Cancel();
        }

        private static string GetScenePath(Transform t) {
            string path = null;
            while (t) {
                path = path == null ? t.name : t.name + "/" + path;
                t = t.parent;
            }

            return path;
        }
    }
}