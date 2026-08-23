using UnityEngine;
using UnityEditor;
using InkRidge.Environment;

public class WindDebug
{
    [MenuItem("Debug/Check Wind Status")]
    static void Check()
    {
        float speed = Shader.GetGlobalFloat(Shader.PropertyToID("_WindSpeed"));
        float mag = Shader.GetGlobalFloat(Shader.PropertyToID("_WindMagnitude"));
        float intensity = Shader.GetGlobalFloat(Shader.PropertyToID("_WindIntensity"));
        float dirX = Shader.GetGlobalFloat(Shader.PropertyToID("_WindDirectionX"));
        float dirZ = Shader.GetGlobalFloat(Shader.PropertyToID("_WindDirectionZ"));

        Debug.Log($"[WindDebug] _WindSpeed={speed} _WindMagnitude={mag} _WindIntensity={intensity} dir=({dirX},{dirZ})");

        // Check if WindSystem exists
        var ws = Object.FindObjectOfType<WindSystem>();
        Debug.Log($"[WindDebug] WindSystem found: {ws != null}");

        // Check a bamboo material's _WindSway
        var renderers = Object.FindObjectsOfType<MeshRenderer>();
        int checkedCount = 0;
        foreach (var r in renderers)
        {
            if (r.sharedMaterial != null && r.sharedMaterial.HasProperty("_WindSway"))
            {
                float sway = r.sharedMaterial.GetFloat("_WindSway");
                if (sway > 0 && checkedCount < 3)
                {
                    Debug.Log($"[WindDebug] {r.gameObject.name} mat={r.sharedMaterial.name} _WindSway={sway}");
                    checkedCount++;
                }
            }
        }
        if (checkedCount == 0) Debug.Log("[WindDebug] NO materials with _WindSway > 0 found!");
    }
}
