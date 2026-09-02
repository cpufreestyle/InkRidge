using UnityEngine;

namespace InkRidge.Environment
{
    /// <summary>
    /// Forces Unity to include the custom shaders in the build. They are only
    /// referenced via Shader.Find() at runtime, so the build pipeline strips
    /// them unless something in a scene holds a material reference.
    /// </summary>
    public class ShaderKeepAlive : MonoBehaviour
    {
        private static Shader _toon;
        private static Shader _skybox;

        void Awake()
        {
            // Touch the shaders so the linker keeps them. Also primes
            // SceneBuilder's cached lookup.
            _toon = Shader.Find("Gongbi/Toon");
            _skybox = Shader.Find("Gongbi/InkSkybox");
        }
    }
}
