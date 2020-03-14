See also https://github.com/firefoxpdm/Rainbeau-s-mods for an overview of the update of Rainbeau's mods.

**Known issues**
* WorldEdit throws an error at startup time when loaded together with this mod. Beginning of the stack trace:
```
Error while loading worldedit - System.ArgumentNullException: Value cannot be null.
Parameter name: method
  at HarmonyLib.HarmonyMethod..ctor (System.Reflection.MethodInfo method) [0x00016] in <2161c8330234450b8141397c32c11571>:0 
  at WorldEdit.WorldEdit.RealisticPlanetsPatch (HarmonyLib.Harmony harmonyInstance) [0x00020] in <47ab241e702d4bd488115db0d791dad2>:0 
  at WorldEdit.WorldEdit..ctor (Verse.ModContentPack content) [0x0003a] in <47ab241e702d4bd488115db0d791dad2>:0 
```
This suggests the error is with the WorldEdit mod, more specifically it is using an outdated Harmony, which makes its use problematic with everything else. There is nothing this mod can do to fix that problem.
