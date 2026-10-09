# EVE-O Preview 6.0.0.3 (modified)

A modified build of EVE-O Preview 6.0.0.3, decompiled and rebuilt from the original fork.

## Download

- **[EVE-O-Preview-share.zip](build/EVE-O-Preview-share.zip)**: the exe, a template config with placeholder character names, and a setup guide.
- **[EVE-O Preview.exe](build/EVE-O%20Preview.exe)**: the exe on its own.

Setup instructions: [template/README.md](template/README.md)

## Changes from the original

- **Hotkeys only work while an EVE client is the active window.** Keys like Tab behave normally in other apps. Set `"HotkeysOnlyWhenClientActive": false` to restore the old behaviour.
- **Character-select cycling.** `CharSelectCycleForwardHotkeys` / `CharSelectCycleBackwardHotkeys` cycle through clients at character select (window title `EVE`), in launch order. Off by default.
- **Default config written on first launch** if no `EVE-O Preview.json` exists.
- **Fixed default hotkey:** group 1 backward was Ctrl+Enter, now Ctrl+F13.

## Repository layout

| Folder | Contents |
|---|---|
| `src/` | Source code (C#, .NET Framework 4.8) |
| `lib/` | Libraries extracted from the original exe |
| `template/` | Shareable config template and setup guide |
| `build/` | Latest build output (updated automatically) |
| `original/` | The original 6.0.0.3 exe |
| `ci/` | Build logs |

Every push to `src/`, `lib/` or `template/` rebuilds on GitHub Actions (Windows) and commits the new exe to `build/`.
