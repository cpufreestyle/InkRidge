using UnityEditor;
using UnityEngine;
using UnityEditor.PackageManager;

public class InstallCodelyBridge
{
    public static void Install()
    {
        Debug.Log("[InstallCodelyBridge] Installing cn.tuanjie.codely.bridge...");
        var request = Client.Add("cn.tuanjie.codely.bridge");
        while (!request.IsCompleted) { }
        
        if (request.Status == StatusCode.Success)
        {
            Debug.Log("[InstallCodelyBridge] Successfully installed: " + request.Result.packageId);
        }
        else if (request.Status == StatusCode.Failure)
        {
            Debug.LogError("[InstallCodelyBridge] Failed: " + request.Error.message);
        }
    }
}
