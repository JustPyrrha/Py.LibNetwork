using Boardgame.Modding;
using Boardgame.Networking;
using Photon.Pun;
using UnityEngine;
using Utils;

namespace Py.LibNetwork.Internal
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class LibNetwork : DemeoMod
    {
        public override void Load()
        {
            SetupModPhotonView();
            SetupModListHandler(gameContext.networkController);
        }

        public override ModdingAPI.ModInformation ModInformation { get; } = new()
        {
            name = "Py.LibNetwork",
            version = "1.0.0",
            author = "JustPyrrha",
            description = "Networking utilities for mod developers.",
            isNetworkCompatible = true
        };

        private void SetupModPhotonView()
        {
            var gameObject = new GameObject("Py.LibNetwork");
            gameObject.AddComponent<PhotonView>();
            gameObject.AddComponent<ModNetworkComponent>();
        }

        private void SetupModListHandler(INetworkController networkController)
        {
            var modListHandler = new RemoteModListHandler();

            networkController.JoinedRoom += modListHandler.JoinedRoomHandle;
            networkController.LocalPlayerLeftRoom += modListHandler.LeftRoomHandle;
            networkController.OtherPlayerJoined += modListHandler.OtherPlayerJoinedHandle;
            networkController.OtherPlayerDisconnected += modListHandler.OtherPlayerLeftHandle;
            ModNetwork.Instance.OnMessage += modListHandler.OnMessageHandle;

            modListHandler.ModListReceived += (playerId, mods) =>
            {
                if (mods.Count > 0)
                {
                    Log(
                        $"Received a ModList with {mods.Count} mods from player {playerId}");
                    foreach (var mod in mods)
                    {
                        Log(
                            $"    {mod.name} v{mod.version} by {mod.author} (network compatible? {(mod.isNetworkCompatible ? "yes" : "no")})");
                    }
                }
                else
                {
                    Log($"Received an empty ModList from {playerId}.");
                }
            };
        }

        private const string LogPrefix = "Py.LibNetwork";
        internal static void Log(string message) => DemeoLog.Log(LogPrefix, message);
        internal static void LogWarn(string message) => DemeoLog.LogWarn(LogPrefix, message);
        internal static void LogError(string message) => DemeoLog.LogError(LogPrefix, message);
    }
}