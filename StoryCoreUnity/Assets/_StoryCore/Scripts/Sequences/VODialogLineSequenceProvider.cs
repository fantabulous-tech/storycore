using UnityEngine;

namespace StoryCore {
    [CreateAssetMenu]
    public class VODialogLineSequenceProvider : AbstractLineSequenceProvider {
        public override DialogLineSequence CreateDialogLine(StoryTeller storyTeller, string text, string section) {
            return new VODialogLineSequence(storyTeller, text, section);
        }
    }
}