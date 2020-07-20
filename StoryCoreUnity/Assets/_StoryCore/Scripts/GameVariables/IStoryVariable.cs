namespace StoryCore.GameVariables {
    public interface IStoryVariable {
        void SetInStory();
        void Subscribe();
        void Unsubscribe();
    }
}