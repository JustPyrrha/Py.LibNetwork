using System;
using Fidelity.Singleton;
using Photon.Pun;

namespace Py.LibNetwork
{
    public class ModNetwork : Singleton<ModNetwork>
    {
        /// <summary>
        /// Triggered when a channel message is received from the network.
        /// This could either be from a broadcast (<see cref="BroadcastChannelMessage"/>) or an targeted send (<see cref="SendChannelMessage"/>).
        /// </summary>
        public event Action<string /* channel */, byte[] /* data */, PlayerId /* sender */> OnChannelMessage;

        /// <summary>
        /// Send a channel message to a specific player.
        /// </summary>
        /// <example>
        /// using var output = new MemoryStream();
        /// using (var writer = new BinaryWriter(output))
        /// {
        ///     writer.Write(someData);
        ///     writer.Write(moreData);
        /// }
        /// ModNetwork.Instance.SendChannelMessage("MyMod/MyChannel", output.ToArray(), playerData.PlayerId);
        /// </example>
        /// <param name="channel">The channel to send on.</param>
        /// <param name="data">The data to send.</param>
        /// <param name="target">The player to send the message to.</param>
        public void SendChannelMessage(string channel, byte[] data, PlayerId target)
        {
            OnSendChannelMessage?.Invoke(
                channel,
                data,
                target
            );
        }
        
        /// <summary>
        /// Broadcast a channel message to all players.
        /// Keep in mind the message is sent using <see cref="RpcTarget.Others"/> so the local client won't receive it.
        /// </summary>
        /// <example>
        /// using var output = new MemoryStream();
        /// using (var writer = new BinaryWriter(output))
        /// {
        ///     writer.Write(someData);
        ///     writer.Write(moreData);
        /// }
        /// ModNetwork.Instance.BroadcastChannelMessage("MyMod/MyChannel", output.ToArray());
        /// </example>
        /// <param name="channel">The channel to send on.</param>
        /// <param name="data">The data to send.</param>
        public void BroadcastChannelMessage(string channel, byte[] data)
        {
            OnBroadcastChannelMessage?.Invoke(
                channel,
                data
            );
        }

        #region Internals (Used by ModNetworkBehaviour)
        // this whole internal setup is a bit shit, I'll probably change it in the future.
        // when I do I'll ensure that the public facing API stays the same.
        
        internal event Action<string, byte[], PlayerId> OnSendChannelMessage;
        internal event Action<string, byte[]> OnBroadcastChannelMessage;
        
        internal void InvokeOnChannelMessage(string channel, byte[] data, PhotonMessageInfo info)
        {
            OnChannelMessage?.Invoke(
                channel,
                data,
                new PlayerId(info.Sender.ActorNumber)
            );
        }
        #endregion
    }
}