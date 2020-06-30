namespace StoryCore.GameEvents {
    public enum MenuOrder {
        EventGeneric = -1000,
        EventBool = -950,
        EventGameObject,
        EventString,
        EventTransform,
        EventPosAndRot,
        VariableBool,
        VariableFloat,
        VariableFloatRange,
        VariableInt,
        VariableString,
        VariableVector3
    }
}