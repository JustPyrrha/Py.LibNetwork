using System.Collections.Generic;
using System.Linq;
using Boardgame;
using Boardgame.Modding;
using Boardgame.Networking;
using Photon.Pun;
using UnityEngine;

namespace Py.LibNetwork
{
    public class ModNetwork : MonoBehaviour
    {
        public static ModNetwork Instance;
        private PhotonView _photonView;
        private GameContext _gameContext;
        private readonly Dictionary<int, Dictionary<string, string>> _networkModLists = new();

        #region RPC Methods
        [PunRPC]
        public void OnModListRPC(Dictionary<string, string> modList, PhotonMessageInfo info)
        {
            LibNetwork.Log($"Received ModList from {info.Sender.UserId}");
            foreach (var (modName, version) in modList)
            {
                LibNetwork.Log($"{modName} ({version})");
            }

            _networkModLists[info.Sender.ActorNumber] = modList;
        }

        [PunRPC]
        public void OnModEventIdSync(PhotonMessageInfo info)
        {
            // byte[]
        }
        #endregion

        #region Room Events
        public void OnSelfJoinedRoom()
        {
            BroadcastModList();
        }

        public void OnSelfLeftRoom()
        {
            _networkModLists.Clear();
        }

        public void OnPlayerJoinedRoom(NetworkPlayer player)
        {
            SendModList(player);
            if(_photonView.AmOwner)
                SyncModEventIds();
        }

        public void OnPlayerLeftRoom(NetworkPlayer player)
        {
            _networkModLists[player.ID.Value] = null;
        }

        #endregion

        #region Network Calls
        private void BroadcastModList()
        {
            if (!PhotonNetwork.IsConnected) return; // safety measure
            _photonView.RPC(
                nameof(OnModListRPC),
                RpcTarget.Others,
                ModdingAPI.GetInstalledMods().ToDictionary(mod => mod.name, mod => mod.version)
            );
        }
        
        private void SendModList(NetworkPlayer player)
        {
            if (!PhotonNetwork.IsConnected) return; // safety measure
            _photonView.RPC(
                nameof(OnModListRPC), 
                PhotonNetwork.CurrentRoom.GetPlayer(player.ID.Value),
                ModdingAPI.GetInstalledMods().ToDictionary(mod => mod.name, mod => mod.version)
            );
        }

        private void SyncModEventIds()
        {
            _photonView.RPC(
                nameof(OnModEventIdSync),
                RpcTarget.All,
                "test"
            );
        }
        #endregion

        public void SetGameContext(GameContext context)
        {
            _gameContext = context;
        }
        
        private void Awake()
        {
            DontDestroyOnLoad(this);
            
            // Setup Photon View
            _photonView = PhotonView.Get(this);
            _photonView.ViewID = 900; // 0-999, we'll use a high view id to avoid conflicts with vanilla.
            _photonView.OwnershipTransfer = OwnershipOption.Request;
            _photonView.Synchronization = ViewSynchronization.UnreliableOnChange;
            _photonView.observableSearch = PhotonView.ObservableSearch.AutoFindAll;

            Instance = this;
        }
    }
}