using UnityEngine;

namespace InkRidge.Environment
{
    /// <summary>
    /// Marks components whose MeshRenderer must be excluded from static batching
    /// (e.g. meshes rebuilt or recolored every frame). StaticBatchingUtility bakes
    /// the shared mesh into one combined mesh, so any later per-instance mesh
    /// writes would be silently ignored.
    /// </summary>
    public interface IDynamicMeshRenderer { }

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
            mat.SetFloat("_WindSway", 0f); // default: no wind
            return mat;
        }

        /// <summary>Create a material that sways in the wind (for vegetation).</summary>
        protected Material MakeWindMat(Color mainColor, float swayAmount = 0.5f, float outlineWidth = 0.01f)
        {
            var mat = MakeMat(mainColor, outlineWidth);
            mat.SetFloat("_WindSway", swayAmount);
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

        /// <summary>
        /// Ink-painting gradient skybox (Gongbi/InkSkybox) with drifting mist bands.
        /// Also sets Trilight ambient so objects pick up sky/ground bounce light.
        /// </summary>
        protected void SetupSkybox(Color topColor, Color horizonColor)
        {
            var skyMat = new Material(Shader.Find("Gongbi/InkSkybox"));
            if (skyMat != null)
            {
                skyMat.SetColor("_ZenithColor", topColor);
                skyMat.SetColor("_HorizonColor", horizonColor);
                skyMat.SetColor("_BottomColor", Color.Lerp(horizonColor, GongbiColors.InkOutline, 0.45f));
                skyMat.SetColor("_CloudColor", Color.Lerp(horizonColor, GongbiColors.InkOutline, 0.22f));
                skyMat.SetFloat("_CloudCoverage", 0.45f);
                skyMat.SetFloat("_CloudSpeed", 0.012f);
                skyMat.SetFloat("_GrainAmount", 0.02f);
                RenderSettings.skybox = skyMat;
            }

            // Trilight ambient: sky from zenith, ground bounce from ink earth.
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = Color.Lerp(topColor, Color.white, 0.25f);
            RenderSettings.ambientEquatorColor = horizonColor;
            RenderSettings.ambientGroundColor = Color.Lerp(GongbiColors.DarkEarth, horizonColor, 0.3f);
            RenderSettings.ambientIntensity = 1.15f;
        }

        protected void SetupFog(Color fogColor, float density)
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = density;
        }

        protected void SetupLighting(Color lightColor, Vector3 lightDir,
            float intensity = 1.1f)
        {
            var lightObj = new GameObject("DirectionalLight");
            lightObj.transform.SetParent(_root.transform);
            lightObj.transform.rotation = Quaternion.LookRotation(lightDir);
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = lightColor;
            light.intensity = intensity;
            // VR mobile: real-time shadows cost a full extra geometry pass per eye.
            // The toon look does not depend on shadow maps (cel bands + ink outline).
            light.shadows = LightShadows.None;
        }

        protected virtual void Start()
        {
            Build();
            // Runtime-generated primitives cannot be pre-batched in the editor,
            // so merge them once here: one draw call per material (+ outline pass)
            // instead of one per renderer. Renderers flagged IDynamicMeshRenderer
            // are skipped; particle systems are not MeshRenderers and are unaffected.
            if (_root != null)
            {
                var statics = new System.Collections.Generic.List<GameObject>();
                foreach (var r in _root.GetComponentsInChildren<MeshRenderer>())
                {
                    if (r.GetComponentInParent<IDynamicMeshRenderer>() == null)
                        statics.Add(r.gameObject);
                }
                if (statics.Count > 0)
                    StaticBatchingUtility.Combine(statics.ToArray(), _root);
            }
        }
    }
}
