using System.Linq;
using Photon.Pun;
using UnityEngine;

namespace Py.LibNetwork.Internal
{
    [DisallowMultipleComponent]
    internal class ModNetworkComponent : MonoBehaviour
    {
        private PhotonView _photonView;

        #region Network Methods

        [PunRPC]
        private void OnModChannelMessage(string channel, byte[] data, PhotonMessageInfo info)
        {
            ModNetwork.Instance.InvokeOnChannelMessage(channel, data, info);
        }

        private void SendChannelMessage(string channel, byte[] data, PlayerId playerId)
        {
            _photonView.RPC(
                nameof(OnModChannelMessage),
                PhotonNetwork.PlayerList.First(it => it.ActorNumber == playerId.Value),
                channel,
                data
            );
        }

        private void BroadcastChannelMessage(string channel, byte[] data)
        {
            _photonView.RPC(
                nameof(OnModChannelMessage),
                RpcTarget.Others,
                channel,
                data
            );
        }

        #endregion
        
        #region Setup
        
        private void SetupPhoton()
        {
            _photonView = PhotonView.Get(this);
            _photonView.ViewID = 900; // 0-999, we'll use a high view id to avoid conflicts with vanilla.
            _photonView.OwnershipTransfer = OwnershipOption.Request;
            _photonView.Synchronization = ViewSynchronization.UnreliableOnChange;
            _photonView.observableSearch = PhotonView.ObservableSearch.AutoFindAll;
        }

        private void SetupModNetwork()
        {
            var network = ModNetwork.Instance;
            network.OnBroadcastChannelMessage += BroadcastChannelMessage;
            network.OnSendChannelMessage += SendChannelMessage;
        }

        #endregion

        #region Unity Methods

        private void Awake()
        {
            SetupPhoton();
            SetupModNetwork();
            DontDestroyOnLoad(this);
        }

        #endregion
    }
}