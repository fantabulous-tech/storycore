using System;

namespace StoryCore.Utils {
    public class DelayPromiseException : Exception {
        public DelayPromiseException(string message) : base(message) { }
    }
}