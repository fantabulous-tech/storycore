# title: StoryCore Demo Scripts

INCLUDE debug
INCLUDE scenario/test

EXTERNAL isDebug()


VAR debug = true
VAR hasRestarted = false
VAR test_counter = 0


// Debug Jump
// (must be in-editor and have 'Debug' enabled in StoryTeller)
{ isDebug(): -> Debug -> }


-> scenario_test


// Fallback for isDebug() external function
=== function isDebug() ===
~ return debug