// ---------------------------------------------------------------------
// ---------------------------------------------------------------------

using System;
using System.Linq;
using StoryCore.GameEvents;
using StoryCore.Utils;

using UnityEngine;

using Random = UnityEngine.Random;

namespace StoryCore.Commands {

    [CreateAssetMenu(menuName = "Commands/Command")]
    public class CommandHandler : BaseGameEvent<CommandHandler, string> {
        [SerializeField] private bool m_AllowChoices = true;
        [SerializeField, ReadOnly] private int m_ID;

        public bool AllowsChoices => m_AllowChoices;

        #if UNITY_EDITOR
        protected void OnValidate() {
            if(UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(this
                                                                          , out string guid
                                                                          , out long localID)) {
                string fourBytesOfGuid = guid.Substring(0,8);
                long longID = uint.Parse(fourBytesOfGuid, System.Globalization.NumberStyles.AllowHexSpecifier);
                //  --- int.MaxValue is roughly half of uint range, so maps pretty close to int.
                int subID = (int)(longID - int.MaxValue);

                if (m_ID != subID) {
                    m_ID = subID;
                }
            }
        }
        #endif

        public string CommandDescription => m_EventDescription;

        public event EventHandler<ScriptCommandInfo> OnCommand; 

        public int GetID() {
            return m_ID;
        }

        public virtual bool OnQueue(ScriptCommandInfo commandInfo) {
            return true;
        }

        public bool TryRun(ScriptCommandInfo info, Action callback) {
            try {
                Run(info).Then(callback);
                return true;
            }
            catch (Exception e) {
                Debug.LogException(e);
                return false;
            }
        }

        public virtual DelaySequence Run(ScriptCommandInfo info) {
            if (info.Params.Any()) {
                info.Params.ForEach(Raise);
            } else {
                Raise(null);
                // RaiseGeneric();
            }

            RaiseOnCommand(info);

            return DelaySequence.Empty;
        }

        protected virtual void RaiseOnCommand(ScriptCommandInfo info) {
            OnCommand?.Invoke(this, info);
        }

        protected override void RaiseDefault() {
            base.RaiseDefault();
            Run(new ScriptCommandInfo(Name));
        }
    }
}