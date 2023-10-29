using System;
using System.Collections.Generic;
using System.IO;
using Boardgame.Modding;
using Boardgame.Networking;

namespace Py.LibNetwork
{
    /// <summary>
    /// Example <see cref="ModNetwork"/> usage.
    /// You can also use <see cref="RemoteModLists"/> in your mods to determine if a remote client is modded.
    /// </summary>
    public class RemoteModListHandler
    {
        private const string ModListChannelPrefix = "Py.LibNetwork/ModList/";
        private const int ModListChannelVersion = 0;

        public static Dictionary<PlayerId, List<ModdingAPI.ModInformation>> RemoteModLists = new();
        public event Action<PlayerId, List<ModdingAPI.ModInformation>> ModListReceived;

        #region (De)Serialization

        private static byte[] SerializeModList(List<ModdingAPI.ModInformation> modList)
        {
            using var output = new MemoryStream();
            using (var writer = new BinaryWriter(output))
            {
                writer.Write(modList.Count);
                foreach (var mod in modList)
                {
                    writer.Write(mod.name);
                    writer.Write(mod.version);
                    writer.Write(mod.author);
                    writer.Write(mod.description);
                    writer.Write(mod.isNetworkCompatible);
                }
            }

            return output.ToArray();
        }

        private static List<ModdingAPI.ModInformation> DeserializeModList(byte[] data)
        {
            using var input = new MemoryStream(data);
            using var reader = new BinaryReader(input);

            var modList = new List<ModdingAPI.ModInformation>();
            var modCount = reader.ReadInt32();
            for (var i = 0; i < modCount; i++)
            {
                modList.Add(new ModdingAPI.ModInformation
                {
                    name = reader.ReadString(),
                    version = reader.ReadString(),
                    author = reader.ReadString(),
                    description = reader.ReadString(),
                    isNetworkCompatible = reader.ReadBoolean()
                });
            }

            return modList;
        }

        #endregion

        private void SendModList(PlayerId target, List<ModdingAPI.ModInformation> modList)
        {
            var data = SerializeModList(modList);
            ModNetwork.Instance.SendChannelMessage(ModListChannelPrefix + ModListChannelVersion, data, target);
        }

        private void BroadcastModList(List<ModdingAPI.ModInformation> modList)
        {
            var data = SerializeModList(modList);
            ModNetwork.Instance.BroadcastChannelMessage(ModListChannelPrefix + ModListChannelVersion, data);
        }

        private void OnModListReceived(int protocolVersion, byte[] data, PlayerId playerId)
        {
            switch (protocolVersion)
            {
                case > ModListChannelVersion:
                {
                    Internal.LibNetwork.LogWarn(
                        $"Received a ModList with protocol version {protocolVersion} which is higher than ours ({ModListChannelVersion}). Is there an update? Marking player {playerId} as modded and discarding ModList.");
                    RemoteModLists[playerId] = new List<ModdingAPI.ModInformation>();
                    ModListReceived?.Invoke(playerId, new List<ModdingAPI.ModInformation>());
                    break;
                }
                case < ModListChannelVersion:
                {
                    Internal.LibNetwork.LogWarn($"Received a ModList with outdated protocol version {protocolVersion} from player {playerId}, applying data migration.");
                    break;
                }
                default:
                {
                    var modList = DeserializeModList(data);
                    RemoteModLists[playerId] = modList;
                    ModListReceived?.Invoke(playerId, modList);
                    break;
                }
            }
        }

        #region Handlers

        public void JoinedRoomHandle()
        {
            BroadcastModList(ModdingAPI.GetInstalledMods());
        }

        public void LeftRoomHandle()
        {
            RemoteModLists.Clear();
        }

        public void OtherPlayerJoinedHandle(NetworkPlayer player)
        {
            SendModList(player.ID, ModdingAPI.GetInstalledMods());
        }

        public void OtherPlayerLeftHandle(NetworkPlayer player)
        {
            if (RemoteModLists.ContainsKey(player.ID))
                RemoteModLists.Remove(player.ID);
        }

        public void OnChannelMessageHandle(string channel, byte[] data, PlayerId sender)
        {
            if (!channel.StartsWith(ModListChannelPrefix)) return;
            var protocolVersion = Convert.ToInt32(channel[ModListChannelPrefix.Length..]);
            OnModListReceived(protocolVersion, data, sender);
        }

        #endregion
    }
}