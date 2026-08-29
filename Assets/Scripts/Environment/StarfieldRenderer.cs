using UnityEngine;

namespace InkRidge.Environment
{
    /// <summary>
    /// Procedural starfield renderer using a custom mesh.
    /// Adapted from Google Daydream Elements StarRendering (Apache 2.0).
    /// Generates stars on a sphere with optional constellations, twinkling, and color variation.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class StarfieldRenderer : MonoBehaviour, IDynamicMeshRenderer
    {
        [Header("Star Layout")]
        [SerializeField] private int _starCount = 600;
        [SerializeField] private float _starDistance = 50f;
        [SerializeField] private float _minBrightness = 0.3f;
        [SerializeField] private float _maxBrightness = 1.0f;

        [Header("Twinkling")]
        [SerializeField] private bool _twinkle = true;
        [SerializeField] private float _twinkleSpeed = 2f;
        [SerializeField] private float _twinkleAmount = 0.3f;

        [Header("Colors")]
        [SerializeField] private Color _starColor = new Color(0.95f, 0.95f, 1f);
        [SerializeField] private Color _warmStarColor = new Color(1f, 0.92f, 0.8f);
        [SerializeField] private float _warmStarRatio = 0.2f;

        private Mesh _mesh;
        private Vector3[] _vertices;
        private Color[] _colors;
        private float[] _phase;
        private float[] _baseAlpha;

        void Start()
        {
            GenerateStars();
        }

        // Twinkle speed is ~2 rad/s; updating the color buffer at display rate
        // (72 Hz in VR) is wasted CPU/VBO bandwidth. 12-15 Hz is visually identical.
        private const int TwinkleUpdateInterval = 5;
        private int _twinkleFrame;

        void Update()
        {
            if (_twinkle && _mesh != null)
            {
                _twinkleFrame++;
                if (_twinkleFrame % TwinkleUpdateInterval != 0) return;

                float time = Time.time;
                for (int i = 0; i < _colors.Length; i++)
                {
                    // Compute alpha from the stored base brightness instead of
                    // accumulating on the previous value (the old running sum
                    // drifted into the Clamp01 bounds and erased star variation).
                    float twinkle = Mathf.Sin(time * _twinkleSpeed + _phase[i]) * _twinkleAmount;
                    _colors[i].a = Mathf.Clamp01(_baseAlpha[i] + twinkle);
                }
                _mesh.colors = _colors;
            }
        }

        void GenerateStars()
        {
            _mesh = new Mesh();
            _vertices = new Vector3[_starCount];
            _colors = new Color[_starCount];
            _phase = new float[_starCount];
            _baseAlpha = new float[_starCount];

            for (int i = 0; i < _starCount; i++)
            {
                // Random point on sphere using Fibonacci distribution for even spread
                float t = (float)i / _starCount;
                float inclination = Mathf.Acos(1 - 2 * t);
                float azimuth = 2 * Mathf.PI * i * 0.618033988749895f; // golden ratio

                float sinInc = Mathf.Sin(inclination);
                _vertices[i] = new Vector3(
                    sinInc * Mathf.Cos(azimuth),
                    Mathf.Cos(inclination),
                    sinInc * Mathf.Sin(azimuth)
                ) * _starDistance;

                // Brightness variation
                float brightness = Random.Range(_minBrightness, _maxBrightness);
                _baseAlpha[i] = brightness;

                // Color: mostly white-blue, some warm
                Color baseCol = Random.value < _warmStarRatio ? _warmStarColor : _starColor;
                _colors[i] = new Color(baseCol.r, baseCol.g, baseCol.b, brightness);

                // Random phase for twinkling
                _phase[i] = Random.Range(0f, Mathf.PI * 2f);
            }

            // Stars as points
            int[] indices = new int[_starCount];
            for (int i = 0; i < _starCount; i++)
                indices[i] = i;

            _mesh.vertices = _vertices;
            _mesh.colors = _colors;
            _mesh.SetIndices(indices, MeshTopology.Points, 0);

            GetComponent<MeshFilter>().mesh = _mesh;
        }
    }
}
