using UnityEngine;

namespace InkRidge.Environment
{
    public class SummitSceneBuilder : SceneBuilder
    {
        protected override void Build()
        {
            base.Build();
            SetupSkybox(new Color(0.02f, 0.02f, 0.06f), new Color(0.06f, 0.08f, 0.15f));
            BuildSummit();
            BuildStarfield();
            BuildDistantMountains();
            BuildMeditationPoint();
            BuildBoundaryWalls();
            SetupLighting(new Color(0.55f, 0.62f, 0.85f), new Vector3(0.2f, -0.4f, 0.8f), 0.6f);
            SetupFog(new Color(0.04f, 0.05f, 0.10f), 0.008f);
        }

        private void BuildSummit()
        {
            var rockMat = MakeMat(GongbiColors.GrayStone, 0.01f);
            var darkRockMat = MakeMat(new Color(0.35f, 0.35f, 0.38f), 0.01f);
            var snowMat = MakeMat(new Color(0.88f, 0.90f, 0.92f), 0.006f);
            var rng = new System.Random(66);

            // Summit platform — circular with snow cap
            Cylinder("SummitPlatform", new Vector3(0, 0.1f, 0),
                new Vector3(5.5f, 0.25f, 5.5f), rockMat);

            // Snow layer on top
            Cylinder("SnowCap", new Vector3(0, 0.28f, 0),
                new Vector3(5.2f, 0.06f, 5.2f), snowMat);

            // Surrounding rocks
            for (int i = 0; i < 15; i++)
            {
                float angle = i / 15f * Mathf.PI * 2f;
                float dist = 3.5f + (float)rng.NextDouble() * 2f;
                float x = Mathf.Sin(angle) * dist;
                float z = Mathf.Cos(angle) * dist;
                float s = 0.4f + (float)rng.NextDouble() * 0.6f;
                var mat = rng.Next(2) == 0 ? rockMat : darkRockMat;
                var rock = Sphere($"Rock_{i}", new Vector3(x, s * 0.3f, z),
                    new Vector3(s, s * 0.7f, s), mat);
                rock.transform.rotation = Random.rotation;
            }

            // A few snow-dusted stones
            for (int i = 0; i < 5; i++)
            {
                float angle = i / 5f * Mathf.PI * 2f;
                float x = Mathf.Sin(angle) * 4.5f;
                float z = Mathf.Cos(angle) * 4.5f;
                Sphere($"SnowRock_{i}", new Vector3(x, 0.3f, z),
                    new Vector3(0.8f, 0.5f, 0.8f), snowMat);
            }
        }

        private void BuildStarfield()
        {
            // Procedural starfield using StarfieldRenderer (adapted from Daydream Elements)
            var starObj = new GameObject("Starfield");
            starObj.transform.SetParent(_root.transform);
            starObj.AddComponent<MeshFilter>();
            var starMR = starObj.AddComponent<MeshRenderer>();
            var starMat = new Material(Shader.Find("Unlit/Color"));
            starMat.color = new Color(0.95f, 0.95f, 1f);
            starMR.material = starMat;
            var starRenderer = starObj.AddComponent<StarfieldRenderer>();

            // Moon
            var moonMat = MakeMat(new Color(0.95f, 0.93f, 0.85f), 0.003f);
            var moon = Sphere("Moon", new Vector3(-15f, 20f, -25f),
                new Vector3(3f, 3f, 3f), moonMat);
            // Moon glow
            var glowMat = MakeMat(new Color(0.80f, 0.80f, 0.70f, 0.3f), 0.001f);
            Sphere("MoonGlow", new Vector3(-15f, 20f, -25f),
                new Vector3(5f, 5f, 5f), glowMat);
        }

        private void BuildDistantMountains()
        {
            var farMat = MakeMat(new Color(0.08f, 0.10f, 0.15f), 0.004f);
            var midMat = MakeMat(new Color(0.12f, 0.14f, 0.20f), 0.005f);

            // Distant mountain silhouettes
            for (int i = 0; i < 6; i++)
            {
                float angle = i / 6f * Mathf.PI * 2f;
                float dist = 40f + i * 5f;
                float x = Mathf.Sin(angle) * dist;
                float z = Mathf.Cos(angle) * dist;
                float h = 8f + i * 2f;
                float w = 12f + i * 2f;
                var mat = i % 2 == 0 ? farMat : midMat;
                Cube($"FarMountain_{i}", new Vector3(x, h / 2f, z),
                    new Vector3(w, h, 3f), mat);
            }
        }

        private void BuildMeditationPoint()
        {
            var mat = MakeMat(GongbiColors.Bluestone, 0.008f);
            var goldMat = MakeMat(GongbiColors.Gold, 0.006f);

            // Meditation platform with gold trim
            Cube("MeditationPlatform", new Vector3(0, 0.35f, 0),
                new Vector3(2.5f, 0.15f, 2.5f), mat);

            // Gold trim around platform
            Cube("GoldTrimN", new Vector3(0, 0.43f, 1.3f),
                new Vector3(2.6f, 0.05f, 0.05f), goldMat);
            Cube("GoldTrimS", new Vector3(0, 0.43f, -1.3f),
                new Vector3(2.6f, 0.05f, 0.05f), goldMat);
            Cube("GoldTrimE", new Vector3(1.3f, 0.43f, 0),
                new Vector3(0.05f, 0.05f, 2.6f), goldMat);
            Cube("GoldTrimW", new Vector3(-1.3f, 0.43f, 0),
                new Vector3(0.05f, 0.05f, 2.6f), goldMat);
        }

        private void BuildBoundaryWalls()
        {
            var wallMat = MakeMat(new Color(0.05f, 0.05f, 0.08f), 0.003f);
            for (int side = -1; side <= 1; side += 2)
            {
                var wall = Cube($"Boundary_{side}", new Vector3(side * 7f, 1f, 0),
                    new Vector3(0.5f, 2f, 20f), wallMat);
                wall.GetComponent<Renderer>().enabled = false;
            }
            for (int dir = -1; dir <= 1; dir += 2)
            {
                var wall = Cube($"Boundary_{dir}", new Vector3(0, 1f, dir * 7f),
                    new Vector3(20f, 2f, 0.5f), wallMat);
                wall.GetComponent<Renderer>().enabled = false;
            }
        }
    }
}
