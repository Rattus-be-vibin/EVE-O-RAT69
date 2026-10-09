# EVE-O Preview - Rattus Edition v1.1: setup

This is a modified build of EVE-O Preview 6.0.0.3. The client-switching hotkeys
only work while an EVE client is the active window, so keys like Tab behave
normally in every other app.

## Install

1. Put `EVE-O Preview.exe` and `EVE-O Preview.json` in the same folder.
2. Make sure EVE-O Preview is **closed** before you edit the JSON (it overwrites
   the file when it saves).
3. Open `EVE-O Preview.json` in a text editor (Notepad, VS Code) and follow the steps below.
4. Run `EVE-O Preview.exe`. If Windows SmartScreen warns you, choose
   **More info → Run anyway** (the build isn't code-signed).

## Add your characters

Every character is written as `"EVE - <character name>"`, exactly as it
appears in the EVE window title. Example: `"EVE - Jane Doe"`.

Replace the placeholder names (`EVE - Main Character 1`, `EVE - Scout Character 1`, …)
with your own.

### Cycle groups

There are 5 cycle groups. Each has a forward hotkey, a backward hotkey and a list of
characters. The number after each name is its position in the cycle.

```json
"CycleGroup1ForwardHotkeys": [ "Tab" ],
"CycleGroup1BackwardHotkeys": [ "Shift+Tab" ],
"CycleGroup1ClientsOrder": {
  "EVE - Jane Doe": 1,
  "EVE - John Doe": 2
},
```

Default hotkeys in this file:

| Group | Forward | Backward |
|---|---|---|
| 1 Main | `Tab` | `Shift+Tab` |
| 2 Scout | `~` (`Oemtilde`) | `Shift+Oemtilde` |
| 3 Utility | `Z` | `Shift+Z` |
| 4 Extra A | `F16` | `Control+F16` |
| 5 Extra B | `F17` | `Control+F17` |

You can add or remove lines, and a group can be left with placeholder names if you
don't use it. Key names follow Windows Forms naming: `A`–`Z`, `F1`–`F24`,
`D1` (number 1), `Oemtilde` (~), and modifiers `Shift+`, `Control+`, `Alt+`.

### Character-select cycling (optional)

These hotkeys cycle only through clients that are still at the character
selection screen (window title exactly `EVE`), in the order you launched them.
They're empty (off) by default. To bind them:

```json
"CharSelectCycleForwardHotkeys": [ "Control+Tab" ],
"CharSelectCycleBackwardHotkeys": [ "Control+Shift+Tab" ],
```

Pick keys that don't clash with your other hotkeys or with EVE's own shortcuts.

### Other per-character settings (optional)

- `DisableThumbnail`: set a character to `true` to hide its preview thumbnail.
- `ClientHotkey`: a hotkey that jumps straight to one character.
- `PerClientActiveClientHighlightColor`: a highlight border colour for one character.

Thumbnail positions (`FlatLayout`) start empty and fill in automatically as
you drag thumbnails around.

## JSON tips

- Every line in a list needs a comma after it, **except the last one** before `}`.
- Names must match the in-game name exactly, including capitals and spaces.
- If the program won't start after an edit, paste the file into a JSON
  validator (search "JSON validator") to find the typo.

## Turning the hotkey change off

Set `"HotkeysOnlyWhenClientActive": false` to make hotkeys work everywhere
again, as in the original EVE-O Preview.
