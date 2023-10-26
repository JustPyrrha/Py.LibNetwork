using Boardgame.Modding;
using Boardgame.Networking;
using Photon.Pun;
using UnityEngine;
using Utils;

namespace Py.LibNetwork
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class LibNetwork : DemeoMod
    {
        public override void Load()
        {
            var gameObject = new GameObject("Py.LibNetwork");
            gameObject.AddComponent<PhotonView>();
            gameObject.AddComponent<ModNetwork>();

            gameContext.networkController.JoinedRoom += ModNetwork.Instance.OnSelfJoinedRoom;
            gameContext.networkController.LocalPlayerLeftRoom += ModNetwork.Instance.OnSelfLeftRoom;
            gameContext.networkController.OtherPlayerJoined += ModNetwork.Instance.OnPlayerJoinedRoom;
            gameContext.networkController.OtherPlayerDisconnected += ModNetwork.Instance.OnPlayerLeftRoom;
            
        }

        public override ModdingAPI.ModInformation ModInformation { get; } = new()
        {
            name = "Py.LibNetwork",
            version = "1.0.0",
            author = "JustPyrrha",
            description = "Networking utilities for mod developers.",
            isNetworkCompatible = true
        };

        private const string LogPrefix = "Py.LibNetwork";

        internal static void Log(string message) => DemeoLog.Log(LogPrefix, message);
        internal static void LogWarn(string message) => DemeoLog.LogWarn(LogPrefix, message);
        internal static void LogError(string message) => DemeoLog.LogError(LogPrefix, message);
    }
}
