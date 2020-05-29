namespace StoryCore {
    internal interface ISequence {
        bool IsComplete { get; }
        void OnQueue();
        void Start();
        void Cancel();
    }
}