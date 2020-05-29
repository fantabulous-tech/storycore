﻿using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace StoryCore {
    [CreateAssetMenu]
    public class TextReplacementConfig : ScriptableObject {
        [SerializeField] private List<Replacement> m_Replacements;

        [NonSerialized] private List<Replacement> m_ConvertedReplacements;

        [Serializable]
        public class Replacement {
            [UsedImplicitly] public string Search;
            [UsedImplicitly] public string Replace;
        }

        private void OnEnable() {
            m_ConvertedReplacements = m_Replacements == null
                                          ? new List<Replacement>()
                                          : m_Replacements.Select(r => {
                                              if (r == null) {
                                                  return null;
                                              }

                                              r.Replace = r.Replace.Replace("\\n", "\n");
                                              return r;
                                          }).ToList();
        }

        public string Convert(string text) {
            foreach (Replacement r in m_ConvertedReplacements) {
                text = text.Replace(r.Search, r.Replace);
            }
            return text;
        }
    }
}