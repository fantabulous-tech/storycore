using System;
using UnityEngine.Events;

namespace StoryCore {
    [Serializable]
    public class UnityEventBool : UnityEvent<bool> { }
    
    [Serializable]
    public class UnityEventString : UnityEvent<string> { }

}