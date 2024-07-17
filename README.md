![Demeo Version](https://img.shields.io/badge/Demeo-v1.36_hotfix-blue?style=flat-square)
![Discord](https://img.shields.io/discord/841011788195823626?style=flat-square&logo=discord&logoColor=white&link=https%3A%2F%2Fdiscord.gg%2FZDjjbRwzs4)
![GitHub release (with filter)](https://img.shields.io/github/v/release/JustPyrrha/Py.LibNetwork?style=flat-square&link=https%3A%2F%2Fgithub.com%2Forendain%2FDemeoMods%2Freleases%2Flatest)
![GitHub all releases](https://img.shields.io/github/downloads/JustPyrrha/Py.LibNetwork/total?style=flat-square)
# Py.LibNetwork

Channel based networking for [Demeo](https://www.resolutiongames.com/demeo) mods.

## Installation
### Built-in Loader
Download the [latest stable release](https://github.com/JustPyrrha/Py.LibNetwork/releases/latest)'s `Py.LibNetwork-version+BuiltIn.zip` file
and extract it into your Demeo game folder.

You should have a folder structure that looks like this:
```
Demeo/
├─ DemeoMods/
│  ├─ Py.LibNetwork/
│  │  ├─ Py.LibNetwork.BuiltIn.dll
```

<details>
<summary>Other Loaders</summary>

### BepInEx
Download the [latest stable release](https://github.com/JustPyrrha/Py.LibNetwork/releases/latest)'s `Py.LibNetwork-version+BepInEx.zip` file
and extract it into your Demeo game folder.

You should have a folder structure that looks like this:
```
Demeo/
├─ BepInEx/
│  ├─ plugins/
│  │  ├─ Py.LibNetwork.BepInEx.dll
```

### MelonLoader
Download the [latest stable release](https://github.com/JustPyrrha/Py.LibNetwork/releases/latest)'s `Py.LibNetwork-version+MelonLoader.zip` file
and extract it into your Demeo game folder.

You should have a folder structure that looks like this:
```
Demeo/
├─ Mods/
│  ├─ Py.LibNetwork.MelonLoader.dll 
```

</details>

## Developer Guide
### Setup
Add dependencies for `ResolutionGames.Singleton.dll` (which can be found in Demeo's `demeo_Data/Managed` folder)
and `Py.LibNetwork.<loader>.dll`

### Usage
1. Add an event handler.
   > [!INFORMATION]
   > Channel names can be anything, but it's recommended to include basic information such as mod name and a data version.\
   > Since there's no mod version enforcement, you may need to apply data migrations.\
   > See `Py.LibNetwork/RemoteModListHandler.cs` for a basic example.

    ```csharp
    using Py.LibNetwork;
    const string ChannelName = "MyMod/MyChannel";

    ModNetwork.Instance.OnMessage += (channel, data, sender) =>
    {
        if(channel != ChannelName) return;
        using var dataStream = new MemoryStream(data);
        using (var reader = new BinaryReader(dataStream))
        {
            var str = reader.ReadString();
            DemeoLog.Log("MyMod", $"Player {sender.Value} says {str}!");
        }
    };
    ```

2. Send a message to a specific player.
   ```csharp
   using var output = new MemoryStream();
   using (var writer = new BinaryWriter(output))
   {
       writer.Write("Hello World");
   }
   // You can get playerData from vairous PlayerHub methods.
   ModNetwork.Instance.SendMessage(ChannelName, output.ToArray(), playerData.PlayerId);
   ```
   
3. Broadcast a message.
   ```csharp
   using var output = new MemoryStream();
   using (var writer = new BinaryWriter(output))
   {
       writer.Write("Hello World");
   }
   ModNetwork.Instance.BroadcastMessage(ChannelName, output.ToArray());
   ```
   
## Remote Mod Lists
As a basic example we provide [RemoteModListHandler.cs](Py.LibNetwork/RemoteModListHandler.cs),
but it can also be used to check which mods a remote player has installed.\
`RemoteModListHandler.RemoteModLists` is a static `Dictionary` keyed by the player's `PlayerId`, it'll only contain a key for a player
if we receive a mod list from them. It can contain an empty list if we receive a mod list with a newer protocol version than ours,
and we'll apply data migrations if it's an older known version.
