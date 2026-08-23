using UnityEngine;
using InkRidge.Core;

namespace InkRidge.Environment
{
    /// <summary>
    /// Builds scene geometry from primitives at runtime.
    /// Adapted from gongbi_prototype SceneBuilder.
    /// All objects use Gongbi/Toon material with GongbiColors palette.
    /// </summary>
    public class SceneBuilder : MonoBehaviour
    {
        [Header("Scene Identity")]
        [SerializeField] protected string _sceneName = "Bamboo";

        protected GameObject _root;

        protected virtual void Build()
        {
            _root = new GameObject($"__Scene_{_sceneName}");
        }

        protected Material MakeMat(Color mainColor, float outlineWidth = 0.012f)
        {
            var mat = new Material(Shader.Find("Gongbi/Toon"));
            mat.SetColor("_MainColor", mainColor);
            mat.SetColor("_ShadowColor", GongbiColors.Shadow(mainColor));
            mat.SetColor("_OutlineColor", GongbiColors.InkOutline);
            mat.SetFloat("_OutlineWidth", outlineWidth);
            return mat;
        }

        protected GameObject Cube(string name, Vector3 pos, Vector3 scale,
            Material mat, Transform parent = null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent ? parent : _root.transform);
            go.transform.position = pos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().material = mat;
            return go;
        }

        protected GameObject Cylinder(string name, Vector3 pos, Vector3 scale,
            Material mat, Transform parent = null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent ? parent : _root.transform);
            go.transform.position = pos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().material = mat;
            return go;
        }

        protected GameObject Sphere(string name, Vector3 pos, Vector3 scale,
            Material mat, Transform parent = null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.SetParent(parent ? parent : _root.transform);
            go.transform.position = pos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().material = mat;
            return go;
        }

        protected GameObject Plane(string name, Vector3 pos, Vector3 scale,
            Material mat, Transform parent = null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Plane);
            go.name = name;
            go.transform.SetParent(parent ? parent : _root.transform);
            go.transform.position = pos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().material = mat;
            return go;
        }

        protected void SetupFog(Color fogColor, float density)
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = density;
            RenderSettings.ambientLight = GongbiColors.WarmAmbient;
        }

        protected void SetupLighting(Color lightColor, Vector3 lightDir,
            float intensity = 0.9f)
        {
            var lightObj = new GameObject("DirectionalLight");
            lightObj.transform.SetParent(_root.transform);
            lightObj.transform.rotation = Quaternion.LookRotation(lightDir);
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = lightColor;
            light.intensity = intensity;
            light.shadows = LightShadows.Soft;
        }

        protected virtual void Start()
        {
            Build();
        }
    }
}
