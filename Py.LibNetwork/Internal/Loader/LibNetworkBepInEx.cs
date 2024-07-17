#if LOADER_BEPINEX

using BepInEx;
using HarmonyLib;

namespace Py.LibNetwork.Internal.Loader
{
    [BepInPlugin(LibNetworkMeta.Name, LibNetworkMeta.Name, LibNetworkMeta.Version)]
    public class LibNetworkBepInEx : BaseUnityPlugin
    {
        private void Awake()
        {
            var harmony = new Harmony(LibNetworkMeta.Name);
            LibNetworkPatches.Patch(harmony);
        }
    }
}

#endif
