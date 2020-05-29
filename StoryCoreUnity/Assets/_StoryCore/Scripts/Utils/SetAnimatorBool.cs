using StoryCore.Utils;

namespace StoryCore {
    public class SetAnimatorBool : SetAnimatorBase {
        public void SetTrue(string boolName) {
            SetInternal(() => m_Animator.SetBool(boolName, true));
        }

        public void SetFalse(string boolName) {
            SetInternal(() => m_Animator.SetBool(boolName, false));
        }
    }
}