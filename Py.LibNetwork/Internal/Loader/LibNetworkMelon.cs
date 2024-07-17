#if LOADER_MELON

using MelonLoader;
using Py.LibNetwork.Internal;
using Py.LibNetwork.Internal.Loader;

[assembly: MelonInfo(
    typeof(LibNetworkMelon),
    LibNetworkMeta.Name,
    LibNetworkMeta.Version,
    LibNetworkMeta.Author,
    LibNetworkMeta.Repo
)]
[assembly: MelonGame("Resolution Games", "Demeo")]
[assembly: MelonGame("Resolution Games", "Demeo PC Edition")]

namespace Py.LibNetwork.Internal.Loader
{
    public class LibNetworkMelon : MelonMod
    {
        public override void OnInitializeMelon()
        {
            LibNetworkPatches.Patch(HarmonyInstance);
        }
    }
}

#endif
