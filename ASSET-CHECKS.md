# Local asset runtime checks

Run `dotnet run` from FrameworkTest. These interactive checks have not been performed by the implementation agent.

The three catalogue keys create separate native textures from the same small test.png.
Core is drawn white; menu/game copies are tinted and labelled.

1. Startup: a loading screen precedes the menu. Expect:
   `[Assets] LOAD CoreTexture`, then `[Assets] LOAD MenuTexture`.
2. Select StartGame. Expect KEEP CoreTexture, UNLOAD MenuTexture, LOAD GameTexture.
3. Press the configured MenuBack input (default Escape/controller B). Expect KEEP CoreTexture,
   UNLOAD GameTexture, LOAD MenuTexture.
4. Repeat transitions. CoreTexture must have exactly one LOAD for the application lifetime,
   and no UNLOAD until shutdown. Loading counts should show one unload and one load on transitions.
5. Exercise menus, rebinding and resolution changes, then ExitGame and separately the window close
   button. Remaining assets must log UNLOAD before Raylib closes the window.

API checks with a live window:
- A fresh AssetManager validates every catalogue file but has no loaded assets or pending work.
- Require then release a key before ProcessNext: no native work.
- Release a loaded key then require it before ProcessNext: same borrowed handle remains.
- Set the same desired set twice: no work after completion.
- Set A B C D to B C D E F: one unload and two loads; shared handles remain unchanged.
- Invalid keys in a batch must throw without partially changing requirements.
- ClearRequiredAssets cancels loads and schedules loaded assets for unloading.
- ProcessNext returns true when complete; each call processes at most one asset, unloads first.
- UnloadAll immediately releases everything and clears requirements, including after a load failure.

Catalogue validation rejects blank/duplicate/padded keys, unsupported types, blank/invalid/missing
file paths, nonpositive/missing font sizes, and font sizes attached to textures.
Relative paths retain the existing AppContext.BaseDirectory convention.
Requirements use case-sensitive logical keys and set semantics, not reference counting.
Borrowed handles must not be used after their asset is processed for unloading.
