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
            BuildStoneStele();
            BuildStoneLanterns();
            BuildBoundaryWalls();
            SetupLighting(new Color(0.55f, 0.62f, 0.85f), new Vector3(0.2f, -0.4f, 0.8f), 0.7f);
            SetupFog(new Color(0.04f, 0.05f, 0.10f), 0.006f);
        }

        private void BuildSummit()
        {
            var rockMat = MakeMat(GongbiColors.GrayStone, 0.01f);
            var darkRockMat = MakeMat(new Color(0.35f, 0.35f, 0.38f), 0.01f);
            var snowMat = MakeMat(new Color(0.88f, 0.90f, 0.92f), 0.006f);
            var iceMat = MakeMat(new Color(0.70f, 0.80f, 0.90f), 0.004f);
            var rng = new System.Random(66);

            // Summit platform — two tiers
            Cylinder("SummitBase", new Vector3(0, 0.05f, 0),
                new Vector3(6.5f, 0.1f, 6.5f), darkRockMat);
            Cylinder("SummitPlatform", new Vector3(0, 0.15f, 0),
                new Vector3(5.5f, 0.2f, 5.5f), rockMat);

            // Snow layer on top
            Cylinder("SnowCap", new Vector3(0, 0.28f, 0),
                new Vector3(5.2f, 0.06f, 5.2f), snowMat);

            // Ice ring around base
            Cylinder("IceRing", new Vector3(0, 0.12f, 0),
                new Vector3(6.0f, 0.05f, 6.0f), iceMat);

            // Surrounding rocks — more variety
            for (int i = 0; i < 20; i++)
            {
                float angle = i / 20f * Mathf.PI * 2f;
                float dist = 3.5f + (float)rng.NextDouble() * 2.5f;
                float x = Mathf.Sin(angle) * dist;
                float z = Mathf.Cos(angle) * dist;
                float s = 0.4f + (float)rng.NextDouble() * 0.8f;
                var mat = rng.Next(3) switch { 0 => darkRockMat, 1 => snowMat, _ => rockMat };
                var rock = Sphere($"Rock_{i}", new Vector3(x, s * 0.3f, z),
                    new Vector3(s, s * 0.7f, s), mat);
                rock.transform.rotation = Random.rotation;
            }

            // Snow-dusted stone formations
            for (int i = 0; i < 6; i++)
            {
                float angle = i / 6f * Mathf.PI * 2f;
                float x = Mathf.Sin(angle) * 4.5f;
                float z = Mathf.Cos(angle) * 4.5f;
                var snowRock = Sphere($"SnowRock_{i}", new Vector3(x, 0.3f, z),
                    new Vector3(0.8f, 0.5f, 0.8f), snowMat);
                snowRock.transform.rotation = Random.rotation;
            }

            // Ice crystals (tall thin cylinders)
            var crystalMat = MakeMat(new Color(0.75f, 0.85f, 0.95f), 0.003f);
            for (int i = 0; i < 8; i++)
            {
                float angle = i / 8f * Mathf.PI * 2f + 0.3f;
                float x = Mathf.Sin(angle) * 4f;
                float z = Mathf.Cos(angle) * 4f;
                float h = 0.5f + (float)rng.NextDouble() * 1.0f;
                Cylinder($"Crystal_{i}", new Vector3(x, h / 2f + 0.3f, z),
                    new Vector3(0.08f, h, 0.08f), crystalMat);
            }
        }

        private void BuildStarfield()
        {
            var starObj = new GameObject("Starfield");
            starObj.transform.SetParent(_root.transform);
            starObj.AddComponent<MeshFilter>();
            var starMR = starObj.AddComponent<MeshRenderer>();
            var starMat = new Material(Shader.Find("Unlit/Color"));
            starMat.color = new Color(0.95f, 0.95f, 1f);
            starMR.material = starMat;
            starObj.AddComponent<StarfieldRenderer>();

            // Moon — larger with glow layers
            var moonMat = MakeMat(new Color(0.95f, 0.93f, 0.85f), 0.003f);
            Sphere("Moon", new Vector3(-15f, 20f, -25f),
                new Vector3(3f, 3f, 3f), moonMat);
            var glowMat = MakeMat(new Color(0.80f, 0.80f, 0.70f, 0.3f), 0.001f);
            Sphere("MoonGlow", new Vector3(-15f, 20f, -25f),
                new Vector3(5f, 5f, 5f), glowMat);
            var glow2Mat = MakeMat(new Color(0.70f, 0.70f, 0.60f, 0.15f), 0.001f);
            Sphere("MoonGlow2", new Vector3(-15f, 20f, -25f),
                new Vector3(8f, 8f, 8f), glow2Mat);

            // Shooting star trail (static visual)
            var trailMat = MakeMat(new Color(0.9f, 0.9f, 1f, 0.5f), 0.001f);
            var trail = Cube("ShootingStar", new Vector3(10f, 25f, -15f),
                new Vector3(3f, 0.05f, 0.05f), trailMat);
            trail.transform.rotation = Quaternion.Euler(0, 0, -20f);
        }

        private void BuildDistantMountains()
        {
            var farMat = MakeMat(new Color(0.08f, 0.10f, 0.15f), 0.004f);
            var midMat = MakeMat(new Color(0.12f, 0.14f, 0.20f), 0.005f);
            var snowMountainMat = MakeMat(new Color(0.40f, 0.42f, 0.48f), 0.004f);

            for (int i = 0; i < 8; i++)
            {
                float angle = i / 8f * Mathf.PI * 2f;
                float dist = 35f + i * 4f;
                float x = Mathf.Sin(angle) * dist;
                float z = Mathf.Cos(angle) * dist;
                float h = 8f + i * 2.5f;
                float w = 12f + i * 2f;
                var mat = i % 3 == 0 ? snowMountainMat : (i % 2 == 0 ? farMat : midMat);
                // Mountain shape: wide base, pointed top
                Cube($"FarMountain_{i}", new Vector3(x, h / 2f, z),
                    new Vector3(w, h, 3f), mat);
                // Snow cap on taller mountains
                if (h > 12f)
                {
                    var snowMat = MakeMat(new Color(0.75f, 0.78f, 0.82f), 0.003f);
                    Sphere($"MountainSnow_{i}", new Vector3(x, h - 1f, z),
                        new Vector3(w * 0.3f, h * 0.15f, 1f), snowMat);
                }
            }
        }

        private void BuildStoneStele()
        {
            var steleMat = MakeMat(GongbiColors.Bluestone, 0.008f);
            var goldMat = MakeMat(GongbiColors.Gold, 0.006f);

            // Tall stone stele at summit edge
            Cube("SummitStele", new Vector3(0, 1.5f, -3f),
                new Vector3(0.4f, 2.5f, 0.15f), steleMat);
            // Gold cap on stele
            Cube("SteleCap", new Vector3(0, 2.8f, -3f),
                new Vector3(0.5f, 0.1f, 0.25f), goldMat);
        }

        private void BuildStoneLanterns()
        {
            var lanternMat = MakeMat(GongbiColors.GrayStone, 0.008f);
            var glowMat = MakeMat(new Color(0.9f, 0.85f, 0.5f, 0.5f), 0.002f);

            for (int side = -1; side <= 1; side += 2)
            {
                Cube($"LanternBase_{side}", new Vector3(side * 2f, 0.4f, 2f),
                    new Vector3(0.3f, 0.5f, 0.3f), lanternMat);
                Sphere($"LanternGlow_{side}", new Vector3(side * 2f, 0.7f, 2f),
                    new Vector3(0.25f, 0.25f, 0.25f), glowMat);
                Cube($"LanternRoof_{side}", new Vector3(side * 2f, 0.9f, 2f),
                    new Vector3(0.35f, 0.08f, 0.35f), lanternMat);
            }
        }

        private void BuildMeditationPoint()
        {
            var mat = MakeMat(GongbiColors.Bluestone, 0.008f);
            var goldMat = MakeMat(GongbiColors.Gold, 0.006f);
            var cushionMat = MakeMat(new Color(0.45f, 0.10f, 0.08f), 0.006f);

            Cube("MeditationPlatform", new Vector3(0, 0.35f, 0),
                new Vector3(2.5f, 0.15f, 2.5f), mat);

            // Gold trim
            Cube("GoldTrimN", new Vector3(0, 0.43f, 1.3f),
                new Vector3(2.6f, 0.05f, 0.05f), goldMat);
            Cube("GoldTrimS", new Vector3(0, 0.43f, -1.3f),
                new Vector3(2.6f, 0.05f, 0.05f), goldMat);
            Cube("GoldTrimE", new Vector3(1.3f, 0.43f, 0),
                new Vector3(0.05f, 0.05f, 2.6f), goldMat);
            Cube("GoldTrimW", new Vector3(-1.3f, 0.43f, 0),
                new Vector3(0.05f, 0.05f, 2.6f), goldMat);

            // Meditation cushion
            Cylinder("MeditationCushion", new Vector3(0, 0.45f, 0),
                new Vector3(0.6f, 0.1f, 0.6f), cushionMat);
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
