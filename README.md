
# TLD Radio Sync Fix

Fixes the wobbly/warbly sound on The Long Drive's custom radio (the one where it sounds like the music is constantly speeding up and slowing down a tiny bit  "wow/flutter").

## What's actually going on

I dug through `Assembly-CSharp.dll` to find where the custom radio handles playback, and the culprit is in `radioscript.SetRadio()`. It runs every frame and keeps a separate manual clock (`cTime`) to track where the song "should" be. Whenever that clock drifts more than 0.5 seconds from what the AudioSource is actually playing, the game does a hard `AudioSource.time = cTime`  basically a forced seek.

Seeks aren't seamless. Do that often enough and it sounds exactly like what people describe as wobble/flutter. It happens with every audio format (WAV, OGG, MP3) because the resync logic doesn't care what format the clip is  it's not a decoding issue, it's a sync issue.

## What this mod does

Small Harmony transpiler patch that finds the `0.5f` threshold inside `SetRadio` and bumps it up (default `2.5f`, configurable). That's it  no rewritten logic, no touching the original DLL. Just relaxing the trigger so the game doesn't seek constantly.

## Install

1. You need [MelonLoader](https://melonwiki.xyz/) already installed and working (you should see a console window pop up alongside the game).
2. Drop `RadioSyncFixMod.dll` into your game's `Mods/` folder.
3. Launch the game. Check the MelonLoader console for:
   ```
   [RadioSyncFix] Loaded. Current threshold: 2.5s (game default: 0.5s).
   [RadioSyncFix] Transpiler applied to SetRadio, 2 instance(s) of 0.5f replaced.
   ```
4. Tune in the custom radio and listen.

If it says "2 instance(s)", the patch landed in both spots it needs to (main channel + secondary channel). If it says 0, the game version probably changed and this needs updating  open an issue.

## Tuning

Don't want to recompile to try a different value? Edit `UserData/MelonPreferences.cfg`, look for `[TLDRadioSyncFix]`, and change `ResyncThreshold`.

## Build it yourself

Needs .NET SDK 6.0. Grab `Assembly-CSharp.dll`, `UnityEngine.dll`, `UnityEngine.CoreModule.dll` from the game's `Managed/` folder, and `MelonLoader.dll` + `0Harmony.dll` from `MelonLoader/net6/`, drop them in `game_refs/`, then:

```bash
dotnet build -c Release
```

## Notes

- This only touches the resync threshold, nothing else about how the radio works.
- Reversible  just delete the dll from `Mods/` if you don't want it anymore.
- Tested on V2023.05.02d. If a future update changes `SetRadio`, the patch might silently do nothing (check the console log above) or, worse, patch the wrong thing  always check the log line.
- Not affiliated with the game devs, just a fan fix.


Made by [Dranolkint](https://steamcommunity.com/id/Coollinkname) :3
