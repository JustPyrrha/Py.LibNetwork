using JetBrains.Annotations;
using UnityEngine;

namespace Py.LibNetwork.Internal
{
    public static class ModLog
    {
        [StringFormatMethod("message")]
        internal static void Log(string message, params object[] args) => Debug.LogFormat($"[{LibNetworkMeta.Name}] ${message}", args);
        [StringFormatMethod("message")]
        internal static void LogWarn(string message, params object[] args) => Debug.LogWarningFormat($"[{LibNetworkMeta.Name}] ${message}", args);
        [StringFormatMethod("message")]
        internal static void LogError(string message, params object[] args) => Debug.LogErrorFormat($"[{LibNetworkMeta.Name}] ${message}", args);
    }
}
