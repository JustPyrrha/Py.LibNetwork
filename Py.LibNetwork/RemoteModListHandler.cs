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
        private const string ChannelPrefix = "Py.LibNetwork/ModList/";
        // bump version here when updating (de)serializers
        private const int ProtocolVersion = 0;

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
            ModNetwork.Instance.SendMessage(ChannelPrefix + ProtocolVersion, data, target);
        }

        private void BroadcastModList(List<ModdingAPI.ModInformation> modList)
        {
            var data = SerializeModList(modList);
            ModNetwork.Instance.BroadcastMessage(ChannelPrefix + ProtocolVersion, data);
        }

        private void OnModListReceived(int protocolVersion, byte[] data, PlayerId playerId)
        {
            switch (protocolVersion)
            {
                case > ProtocolVersion:
                {
                    Internal.LibNetwork.LogWarn(
                        $"Received a ModList with protocol version {protocolVersion} which is higher than ours ({ProtocolVersion}). Is there an update? Marking player {playerId} as modded and discarding ModList.");
                    RemoteModLists[playerId] = new List<ModdingAPI.ModInformation>();
                    ModListReceived?.Invoke(playerId, new List<ModdingAPI.ModInformation>());
                    break;
                }
                case < ProtocolVersion:
                {
                    Internal.LibNetwork.LogWarn($"Received a ModList with outdated protocol version {protocolVersion} from player {playerId}, applying data migration.");
                    // add migrations here when updating (de)serializers
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

        public void OnMessageHandle(string channel, byte[] data, PlayerId sender)
        {
            if (!channel.StartsWith(ChannelPrefix)) return;
            var protocolVersion = Convert.ToInt32(channel[ChannelPrefix.Length..]);
            OnModListReceived(protocolVersion, data, sender);
        }

        #endregion
    }
}