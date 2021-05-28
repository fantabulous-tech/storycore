# StoryCore
A system of tools for Unity to support quickly assembling interactive scenes based on Ink scripts that support scene transitions and lip synched VO.

<details>
  <summary>Table of Contents</summary>

  * [Ink Script Commands](#Ink-Script-Commands)
    * [/action &lt;action_name>](#action-action_name)
    * [/ambient &lt;track>](#ambient-track)
    * [/character &lt;name>](#character-name-perform-emotion-intensity)
    * [/emotion &lt;emotion>](#emotion-emotion-intensity)
    * [/list &lt;bucket>](#list-bucket)
    * [/log &lt;message>](#log-message)
    * [/logWarning &lt;message>](#logWarning-message)
    * [/logError &lt;message>](#logError-message)
    * [/lookat &lt;target>](#lookat-target)
    * [/music &lt;track>](#music-track)
    * [/notify title="&lt;title>" text="&lt;text>"](#notify-titletitle-texttext)
    * [/open_link &lt;url>](#open_link-url)
    * [/perform &lt;performance>](#perform-performance-emotion-intensity)
    * [/quit](#quit)
    * [/recenter](#recenter)
    * [/scene &lt;room>.&lt;staging>](#scene-roomstaging)
    * [/sound &lt;sound_name>](#sound-sound_name)
    * [/wait [&lt;seconds>]](#wait-seconds)
  * [Ink Script Choices](#Ink-Script-Choices)
    * [[yes] / [no]](#yes--no)
    * [[timeout] / [timeout:&lt;seconds>]](#timeout--timeoutseconds)
    * [[wait] / [wait:&lt;seconds>]](#wait--waitseconds)
    * [[continue] / [continue:&lt;seconds>]](#continue--continueseconds)
    * [[distracted] / [pay-attention]](#distracted--pay-attention)
    * [[move] / [move:&lt;location>]](#move--movelocation)
  * [Other Ink Script Info](#Other-Ink-Script-Info)
    * [VO Delays](#VO-Delays)
</details>


## Ink Script Commands

These are the custom commands added to Ink Script. For more information on Ink itself, see the [Inkle’s how-to guide](https://github.com/inkle/ink/blob/master/Documentation/WritingWithInk.md).

Note: You can use all of these in the in-game debug console. While playing, press Tab to bring up the console, then type the command. You can also use **help** in the console to show all the available commands.


### /action &lt;action_name>

Triggers custom GameEvents that specific scenes or systems know about. Currently supported actions include:


<table>
  <tr>
   <td><strong>&lt;action_name></strong>
   </td>
   <td><strong>Description</strong>
   </td>
  </tr>
  <tr>
   <td>open_journal
   </td>
   <td>Opens the player’s journal.
   </td>
  </tr>
  <tr>
   <td>close_journal
   </td>
   <td>Closes the player’s journal.
   </td>
  </tr>
  <tr>
   <td>restart
   </td>
   <td>Restart the demo.
   </td>
  </tr>
</table>



### /ambient &lt;track>

Sets the ambient audio loop. Options are found in the Audio Ambient Bucket. For a complete list: **/list ambient**


<table>
  <tr>
   <td><strong>&lt;track></strong>
   </td>
   <td><strong>Description</strong>
   </td>
  </tr>
  <tr>
   <td>crowd
   </td>
   <td>Murmuring of a crowd
   </td>
  </tr>
  <tr>
   <td>rain
   </td>
   <td>Rain and thunder sounds
   </td>
  </tr>
  <tr>
   <td>none or off
   </td>
   <td>Turns off the ambient sound.
   </td>
  </tr>
</table>



### /character &lt;name> [&lt;perform> [&lt;emotion> [&lt;intensity>]]]

Selects a character to focus on and optionally set their current performance and emotion. For a complete list: **/list characters**. For performance options, see [/perform](#perform-performance-emotion-intensity) and for emotion options, see [/emotion](#emotion-emotion-intensity) below.


### /emotion &lt;emotion> [&lt;intensity>]

Sets the focused character's current facial expression.

If set, **&lt;intensity>** will determine the amount of the expression to apply over neutral. (Range from 1 to 100. Default is 100)


### /list &lt;bucket>

Lists out the items in a specific bucket of assets.


<table>
  <tr>
   <td><strong>&lt;bucket></strong>
   </td>
   <td><strong>Description</strong>
   </td>
  </tr>
  <tr>
   <td>actions
   </td>
   <td>List of actions available for the <strong>/action</strong> command.
   </td>
  </tr>
  <tr>
   <td>ambient
   </td>
   <td>List of ambient audio tracks available for the <strong>/ambient &lt;track></strong> command.
   </td>
  </tr>
  <tr>
   <td>music
   </td>
   <td>List of music audio tracks for the <strong>/music &lt;track></strong> command.
   </td>
  </tr>
  <tr>
   <td>characters
   </td>
   <td>List of available characters available for the <strong>/character &lt;character_name></strong> command.
   </td>
  </tr>
  <tr>
   <td>choices
   </td>
   <td>List of all <strong>[choice]</strong> options available for scripting.
   </td>
  </tr>
  <tr>
   <td>commands
   </td>
   <td>List of all <strong>/command</strong> options available for scripting.
   </td>
  </tr>
  <tr>
   <td>performances
   </td>
   <td>List of all available performances for the <strong>/perform &lt;performance></strong> command.
   </td>
  </tr>
  <tr>
   <td>scenes
   </td>
   <td>List of all available scenes for the <strong>/scene &lt;room>.&lt;staging></strong> command.
   </td>
  </tr>
  <tr>
   <td>vo
   </td>
   <td>List of all available VO lines available. (Warning: Very long).
   </td>
  </tr>
</table>


### /log &lt;message>

Displays &lt;message> in the console.


### /logWarning &lt;message>

Displays &lt;message> in the console as a warning.


### /logError &lt;message>

Displays &lt;message> in the console as an error.


### /lookat &lt;target>

Tells the focused character to look at the specified target. Current targets include ‘player’ or a character name.


### /music &lt;track>

Sets the music audio loop. Options are found in the Audio Music Bucket. For a complete list: **/list music**


<table>
  <tr>
   <td><strong>&lt;track></strong>
   </td>
   <td><strong>Description</strong>
   </td>
  </tr>
  <tr>
   <td>entry
   </td>
   <td>Music used in the entryway.
   </td>
  </tr>
  <tr>
   <td>none or off
   </td>
   <td>Turns off the music.
   </td>
  </tr>
</table>


### /notify title="&lt;title>" [text="&lt;text>"]

Puts up a notification box with a title and optional text. Best to have it include a question like “Understood?” and have a **[yes]** choice immediately after so the player can read it at their own speed.


### /open_link &lt;url>

This opens a web page to the specified **&lt;url>**. This is currently used to open the page with the access code when trying to enter an early access area.


### /perform &lt;performance> [&lt;emotion> [&lt;intensity>]]

This tells the focused character to perform an animation. This can also sometimes include sounds, such as performances of a ‘crowd’ character. See **/list performances** for a complete list.

Additionally, you can add an optional emotion and emotion intensity. See [/emotion](#emotion-emotion-intensity) above.


### /quit

Immediately quits the game.


### /recenter

Recenters the player or the last used **[move]** or starting spawn point.


### /scene &lt;room>.&lt;staging>

Loads the two Unity scenes that make up a fully assembled scene in the game. The **&lt;room>** part of the name is the room that the scene takes place in. The **&lt;staging>** part of the name is the name of the staged scene that contains the layout of elements in the room like the player’s spawn location, move location, and starting scene characters.

Note: You can use **/scene none** to load an empty scene. This is sometimes necessary if you want to transition back to the spawn point of the existing scene (such as for replaying a scene for testing purposes).


### /sound &lt;sound_name>

Plays a one-off audio file. Options are found in the Sound Bucket. For a complete list: **/list sounds**


<table>
  <tr>
   <td><strong>&lt;sound_name></strong>
   </td>
   <td><strong>Description</strong>
   </td>
  </tr>
  <tr>
   <td>tada
   </td>
   <td>Triggers the ‘tada’ sound of great accomplishment.
   </td>
  </tr>
</table>


### /wait [&lt;seconds>]

Waits for a set amount of time before continuing. This is the equivalent of using the **[continue]** choice. **&lt;seconds>** determines how long to wait. If not included, **/wait** will pause for 1 second.


## Ink Script Choices


### [yes] / [no]

These two choices are the default yes/no options for the player.



*   **[yes]** responds to the player’s head nod (up and down)
*   **[no]** responds to the player’s head shake (left and right)


### [timeout] / [timeout:&lt;seconds>]

A ‘wait’ choice that is used as a way to tell when the player is indecisive. Waits for **&lt;seconds>** then selects the **[timeout]** choice. Default is 7 seconds.


### [wait] / [wait:&lt;seconds>]

A ‘wait’ choice that usually represents a short pause or used for explicit timing. Waits for **&lt;seconds>** then selects the **[wait]** choice. Default is 1 second. If used as the only choice, this acts exactly the same as using the **/wait** command.


### [continue] / [continue:&lt;seconds>]

A ‘wait’ choice that is usually used represents no pause. Waits for **&lt;seconds>** then selects the **[continue]** choice. Default is 0 seconds.

This is sometimes used in the middle of a long section of dialog to break up the text when testing in the editor. It is also used to ‘scope’ choices.

For example, the **[yes]** and **[no]** choices will be tracked and detected before the character has finished speaking. If there is a lot of dialog, then the player might accidentally trigger **[yes]** or **[no]** earlier than desired. By using a **[continue]** choice, the **[yes]** and **[no]** are scoped down to just the lines after the **[continue]** choice.


### [distracted] / [pay-attention]

These two choices are driven by the Focused Character’s head and whether or not it is inside an area around the center of the player’s field of view. A 1-second timer determines if they are ‘distracted’ (looking away from the Focused Character for more than 1 second) or ‘paying attention’ looking at the Focused Character for more than 1 second).

*   **[distracted]** - If this choice is available, it is triggered by the player looking away from the Focused Character for more than 1 second.
*   **[pay-attention]** - If this choice is available, it is triggered by the player looking at the Focused Character for more than 1 second. This is usually used after the player has triggered the **[distracted]** choice to see when they look back.


### [move] / [move:&lt;location>]

Triggered when the player moves to a location. The choice itself also determines if the move location is visible. All move locations are hidden until a **[move]** is available. If **&lt;location>** is specified, then only those locations matching the **&lt;location>** name are visible. When a player moves to those locations, it selects the associated **[move]** choice.

The **&lt;status>** portion allows the move location to show it’s current status. If the **[move]** choice isn’t there at all, then it is invisible and disabled.


<table>
  <tr>
   <td><strong>&lt;status></strong>
   </td>
   <td><strong>Description</strong>
   </td>
  </tr>
  <tr>
   <td>unlocked
   </td>
   <td>Available for the player to use it. (default)
   </td>
  </tr>
  <tr>
   <td>under_construction
   </td>
   <td>Currently unavailable with a lock on it and an ‘under construction’ tag.
   </td>
  </tr>
  <tr>
   <td>early_access
   </td>
   <td>Shows with the ‘early access’ tag. Available if the early access code has been entered or will ask for the code if it hasn’t been entered yet.
   </td>
  </tr>
</table>


## Other Ink Script Info


### VO Delays

Delays are automatically added between voice-over dialog based on the punctuation at the end of a line.


<table>
  <tr>
   <td><strong>Last Character</strong>
   </td>
   <td><strong>Delay</strong>
   </td>
  </tr>
  <tr>
   <td><pre>.
?
"
'
\
~
;
:
)
]</pre>
   </td>
   <td>0.8 seconds
<p>
Note: ‘\’ is special. It is removed from the subtitle text, but tells the dialog system to add in the 0.8 second delay.
   </td>
  </tr>
  <tr>
   <td><pre>a-z
-
/</pre>
   </td>
   <td>0 seconds
<p>
Note: ‘/’ is special. It is removed from the subtitle text, but tells the dialog system to remove the delay.
   </td>
  </tr>
  <tr>
   <td>other
   </td>
   <td>0.4 seconds
   </td>
  </tr>
</table>

