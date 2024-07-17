#if LOADER_BEPINEX || LOADER_MELON

using Boardgame;
using Boardgame.Modding;
using HarmonyLib;

namespace Py.LibNetwork.Internal.Loader
{
    // If we're not running on the built-in loader we need to patch some things to get consistent loading with the built-in loader.
    internal static class LibNetworkPatches
    {
        internal static void Patch(HarmonyLib.Harmony harmony)
        {
            harmony.Patch(
                original: typeof(ModdingAPI).GetMethod(nameof(ModdingAPI.RunModLoad)),
                prefix: new HarmonyMethod(typeof(LibNetworkPatches), nameof(ModdingAPI_RunModLoad_Prefix))
            );
        }

        private static void ModdingAPI_RunModLoad_Prefix(GameContext gameContext)
        {
            new LibNetwork(gameContext).Load();
        }
    }
}

#endif
