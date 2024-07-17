#if LOADER_BUILTIN

using Boardgame.Modding;
using JetBrains.Annotations;

namespace Py.LibNetwork.Internal.Loader
{
    [UsedImplicitly]
    public class LibNetworkBuiltIn : DemeoMod
    {
        public override void Load()
        {
            new LibNetwork(gameContext).Load();
        }

        public override ModdingAPI.ModInformation ModInformation { get; } = new()
        {
            name = LibNetworkMeta.Name,
            version = LibNetworkMeta.Version,
            author = LibNetworkMeta.Author,
            description = LibNetworkMeta.Description,
            isNetworkCompatible = LibNetworkMeta.NetworkCompatible
        };
    }
}

#endif
