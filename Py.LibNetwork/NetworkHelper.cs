using System.Linq;
using Boardgame;
using Boardgame.Networking;

namespace Py.LibNetwork
{
    public static class NetworkHelper
    {
        public static string TryResolvePlayerName(GameContext context, PlayerId id) =>
            TryResolvePlayerName(context.networkController, id);
        public static string TryResolvePlayerName(INetworkController networkController, PlayerId id) =>
            networkController.PlayerList.First(it => it.ID == id).nickname ?? id.Value.ToString();
    }
}
