using Boardgame;
using Photon.Pun;
using UnityEngine;

namespace Py.LibNetwork.Internal
{
    public static class LibNetworkMeta
    {
        public const string Name = "Py.LibNetwork";
        public const string Version = "1.1.0";
        public const string Author = "JustPyrrha";
        public const string Description = "Networking utilities for mod developers.";
        public const string Repo = "https://github.com/JustPyrrha/Py.LibNetwork";
        public const bool NetworkCompatible = true;
    }

    public class LibNetwork(GameContext gameContext)
    {
        public void Load()
        {
#if LOADER_BEPINEX || LOADER_MELON
            // There currently exists a bug that will prevent the game from loading
            // with mods added to the modding api because the modding menu prefab isn't set.
            if (gameContext.commonRegistry.moddingMenuPrefab != null)
            {
                if (ModdingAPI.GetInstalledMods().All(it => it.name != LibNetworkMeta.Name))
                {
                    ModdingAPI.ExternallyInstalledMods.Add(new ModdingAPI.ModInformation
                    {
                        name = LibNetworkMeta.Name,
                        version = LibNetworkMeta.Version,
                        author = LibNetworkMeta.Author,
                        description = LibNetworkMeta.Description,
                        isNetworkCompatible = LibNetworkMeta.NetworkCompatible
                    });
                }
            }
#endif

            SetupModPhotonView();
            SetupModListHandler();
        }

        private void SetupModPhotonView()
        {
            var gameObject = new GameObject(LibNetworkMeta.Name);
            gameObject.AddComponent<PhotonView>();
            gameObject.AddComponent<ModNetworkComponent>();
        }

        private void SetupModListHandler()
        {
            var modListHandler = new RemoteModListHandler(gameContext);
            var networkController = gameContext.networkController;

            networkController.JoinedRoom += modListHandler.JoinedRoomHandle;
            networkController.LocalPlayerLeftRoom += modListHandler.LeftRoomHandle;
            networkController.OtherPlayerJoined += modListHandler.OtherPlayerJoinedHandle;
            networkController.OtherPlayerDisconnected += modListHandler.OtherPlayerLeftHandle;
            ModNetwork.Instance.OnMessage += modListHandler.OnMessageHandle;

            modListHandler.ModListReceived += (playerId, mods) =>
            {
                if (mods.Count > 0)
                {
                    ModLog.Log(
                        "Received a ModList with {0} mods from player {1}.",
                        mods.Count,
                        NetworkHelper.TryResolvePlayerName(gameContext, playerId)
                    );
                    foreach (var mod in mods)
                    {
                        ModLog.Log(
                            "    {0} v{1} by {2}. (Network Compatible? {3})",
                            mod.name,
                            mod.version,
                            mod.author,
                            mod.isNetworkCompatible ? "Y" : "N"
                        );
                    }
                }
                else
                {
                    ModLog.Log(
                        "Received an empty ModList from {0}.",
                        NetworkHelper.TryResolvePlayerName(gameContext, playerId)
                    );
                }
            };
        }
    }
}