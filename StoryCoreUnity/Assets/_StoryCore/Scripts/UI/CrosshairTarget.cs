using System.Collections.Generic;
using StoryCore.Utils;
using UnityEngine;

namespace StoryCore.UI {
    public class CrosshairTarget : MonoBehaviour {
        [SerializeField] private List<Collider> m_IgnoredColliders;
        [SerializeField] private LayerMask m_LayerMask;

        public GameObject Target { get; private set; }
        public float Distance { get; private set; }
        public Vector3 Point { get; private set; }

        private void Update() {
            RaycastHit[] hits = new RaycastHit[10];
            Physics.RaycastNonAlloc(transform.ForwardRay(), hits, Mathf.Infinity, m_LayerMask);
            Target = null;
            Distance = -1;

            foreach (RaycastHit hit in hits) {
                if (!hit.collider) {
                    continue;
                }

                if (m_IgnoredColliders.Contains(hit.collider)) {
                    continue;
                }

                if (Distance < 0 || hit.distance < Distance) {
                    Target = hit.transform.gameObject;
                    Distance = hit.distance;
                    Point = hit.point;
                }
            }
        }
    }
}