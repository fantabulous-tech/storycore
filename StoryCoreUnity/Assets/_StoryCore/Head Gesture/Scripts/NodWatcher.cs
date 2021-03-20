using System;
using CoreUtils;
using UnityEngine;

namespace StoryCore.HeadGesture {
    public class NodWatcher {
        private readonly HeadGestureTracker m_Tracker;
        private readonly DirectionEdge[] m_Edges;
        private readonly string m_Name;

        private DirectionEdge m_LastEdgePulled;
        private float m_LastNodTime;
        private int m_NodCount;

        public event Action<DirectionEdge> Nodded;

        public NodWatcher(string name, DirectionEdge[] edges, HeadGestureTracker tracker) {
            m_Name = name;
            m_Edges = edges;
            m_Tracker = tracker;

            m_Edges.ForEach(e => {
                e.LimitReached += OnLimitReached;
                e.OffCentered += OnOffCentered;
            });
        }

        public void Update() {
            // Check for timeout.
            if (m_LastNodTime + m_Tracker.MaxDuration < Time.unscaledTime) {
                m_NodCount = 0;
                m_LastEdgePulled = null;
            }
        }

        private void OnOffCentered(DirectionEdge edge) {
            ResetNod();
        }

        private void OnLimitReached(DirectionEdge edge) {
            if (m_LastEdgePulled != edge) {
                if (m_LastEdgePulled == null) {
                    // If this is the first time we are hitting an edge, then skip counting.
                    //Debug.Log($"{m_Name} nod started.");
                } else {
                    m_NodCount++;
                    //Debug.Log($"{m_Name} Nods = {m_NodCount}");
                }

                DirectionEdge displayedEdge = m_LastEdgePulled;
                m_LastEdgePulled = edge;
                UpdateSprites();
                ResetNodTimer(edge);

                if (m_NodCount > m_Tracker.RequiredNodCount) {
                    Nodded?.Invoke(displayedEdge);
                    ResetNod();
                }
            } else if (m_LastEdgePulled && m_LastEdgePulled.AtLimit) {
                ResetNodTimer(edge);
            }
        }

        private void ResetNodTimer(DirectionEdge edge) {
            m_LastNodTime = Time.unscaledTime;

            // Only show the edge on the last nod.
            if (m_NodCount >= m_Tracker.RequiredNodCount - 1 && m_Tracker.ShowFeedback) {
                edge.ResetFade();
            }
        }

        public void ResetNod() {
            //Debug.Log($"Resetting {m_Name}");
            m_NodCount = 0;
            m_Edges.ForEach(e => e.Recenter());
            m_LastEdgePulled = null;
            UpdateSprites();
        }

        private void UpdateSprites() {
            m_Edges.ForEach(e => e.SetSprite(m_LastEdgePulled == e));
        }
    }
}