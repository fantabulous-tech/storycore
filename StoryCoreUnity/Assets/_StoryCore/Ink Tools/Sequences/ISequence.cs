namespace StoryCore {
    internal interface ISequence {
        bool IsComplete { get; }
        bool AllowsChoices { get; }
        void Interrupt();
        void Start();
        void Cancel();
    }
}