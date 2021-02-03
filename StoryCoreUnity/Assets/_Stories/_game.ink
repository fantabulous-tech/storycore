# title: StoryCore Demo Scripts

INCLUDE debug
INCLUDE scenario/test

VAR debug = true
VAR hasRestarted = false
VAR test_counter = 0
VAR inVR = false

-> game_start

=== game_start ===

// Debug Jump
// (must be in-editor and have 'Debug' enabled in StoryTeller)
{ debug: -> Debug -> }

-> scenario_test
