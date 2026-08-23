using UnityEngine;

namespace InkRidge.Environment
{
    public class PavilionSceneBuilder : SceneBuilder
    {
        [Header("Pavilion Config")]
        [SerializeField] private int _treeCount = 15;

        protected override void Build()
        {
            base.Build();
            SetupSkybox(new Color(0.80f, 0.84f, 0.88f), new Color(0.94f, 0.92f, 0.90f));
            BuildGround();
            BuildPavilion();
            BuildSurroundingTrees();
            BuildStonePath();
            BuildMeditationPoint();
            BuildBoundaryWalls();
            SetupLighting(GongbiColors.WarmLight, new Vector3(0.4f, -0.5f, 0.5f), 1.0f);
            SetupFog(new Color(0.93f, 0.91f, 0.89f), 0.04f);
        }

        private void BuildGround()
        {
            // Stone ground with moss patches
            var groundMat = MakeMat(new Color(0.42f, 0.44f, 0.40f), 0.006f);
            Plane("Ground", Vector3.zero, new Vector3(12, 1, 12), groundMat);

            var mossMat = MakeMat(new Color(0.20f, 0.32f, 0.16f), 0.004f);
            var rng = new System.Random(11);
            for (int i = 0; i < 12; i++)
            {
                float x = (float)(rng.NextDouble() * 2 - 1) * 8f;
                float z = (float)(rng.NextDouble() * 2 - 1) * 8f;
                if (Mathf.Abs(x) < 4f && Mathf.Abs(z) < 4f) continue;
                Plane($"Moss_{i}", new Vector3(x, 0.01f, z),
                    new Vector3(0.8f + (float)rng.NextDouble() * 0.6f, 1,
                        0.8f + (float)rng.NextDouble() * 0.6f), mossMat);
            }
        }

        private void BuildPavilion()
        {
            var woodMat = MakeMat(GongbiColors.DarkWood, 0.01f);
            var roofMat = MakeMat(GongbiColors.CinnabarRoof, 0.012f);
            var tileMat = MakeMat(GongbiColors.GrayTileRoof, 0.008f);
            var goldMat = MakeMat(GongbiColors.Gold, 0.006f);
            var floorMat = MakeMat(GongbiColors.OchreWall, 0.004f);

            float pillarH = 3.8f;
            float size = 3.5f;

            // Stone base
            var baseMat = MakeMat(GongbiColors.Bluestone, 0.008f);
            Cube("PavilionBase", new Vector3(0, 0.15f, 0),
                new Vector3(size * 2.3f, 0.3f, size * 2.3f), baseMat);

            // 4 pillars
            Vector3[] corners = {
                new Vector3(-size, 0, -size),
                new Vector3(size, 0, -size),
                new Vector3(-size, 0, size),
                new Vector3(size, 0, size)
            };
            for (int i = 0; i < corners.Length; i++)
            {
                Cylinder($"Pillar_{i}", new Vector3(corners[i].x, pillarH / 2f + 0.3f, corners[i].z),
                    new Vector3(0.35f, pillarH, 0.35f), woodMat);
                // Pillar bases
                Cylinder($"PillarBase_{i}", new Vector3(corners[i].x, 0.35f, corners[i].z),
                    new Vector3(0.5f, 0.1f, 0.5f), baseMat);
            }

            // Cross beams
            Cube("BeamN", new Vector3(0, pillarH + 0.3f, -size),
                new Vector3(size * 2f, 0.25f, 0.25f), woodMat);
            Cube("BeamS", new Vector3(0, pillarH + 0.3f, size),
                new Vector3(size * 2f, 0.25f, 0.25f), woodMat);
            Cube("BeamE", new Vector3(size, pillarH + 0.3f, 0),
                new Vector3(0.25f, 0.25f, size * 2f), woodMat);
            Cube("BeamW", new Vector3(-size, pillarH + 0.3f, 0),
                new Vector3(0.25f, 0.25f, size * 2f), woodMat);

            // Multi-layered roof (Chinese style: upturned eaves)
            // Layer 1 — wide eave
            Cube("RoofEave", new Vector3(0, pillarH + 0.5f, 0),
                new Vector3(size * 3.2f, 0.15f, size * 3.2f), tileMat);

            // Layer 2 — main roof body
            Cube("RoofMain", new Vector3(0, pillarH + 1.5f, 0),
                new Vector3(size * 2.4f, 0.2f, size * 2.4f), roofMat);

            // Layer 3 — roof top (smaller)
            Cube("RoofTop", new Vector3(0, pillarH + 2.5f, 0),
                new Vector3(size * 1.5f, 0.15f, size * 1.5f), roofMat);

            // Ridge ornament — gold sphere on top
            Sphere("RoofOrnament", new Vector3(0, pillarH + 3.2f, 0),
                new Vector3(0.4f, 0.4f, 0.4f), goldMat);

            // Upturned eave corners (4 small angled cubes)
            for (int i = 0; i < 4; i++)
            {
                float sx = (i < 2 ? -1 : 1) * size * 1.6f;
                float sz = (i % 2 == 0 ? -1 : 1) * size * 1.6f;
                Cube($"EaveCorner_{i}", new Vector3(sx, pillarH + 0.7f, sz),
                    new Vector3(0.4f, 0.15f, 0.4f), tileMat);
            }

            // Floor
            Cube("PavilionFloor", new Vector3(0, 0.2f, 0),
                new Vector3(size * 1.8f, 0.1f, size * 1.8f), floorMat);

            // Steps leading up
            Cube("Step1", new Vector3(0, 0.15f, size + 1f),
                new Vector3(2.5f, 0.15f, 0.8f), baseMat);
            Cube("Step2", new Vector3(0, 0.3f, size + 0.5f),
                new Vector3(2f, 0.15f, 0.6f), baseMat);
        }

        private void BuildSurroundingTrees()
        {
            var trunkMat = MakeMat(GongbiColors.DarkWood, 0.008f);
            var leafMat = MakeMat(new Color(0.18f, 0.35f, 0.14f), 0.004f);
            var darkLeafMat = MakeMat(new Color(0.10f, 0.28f, 0.10f), 0.004f);
            var rng = new System.Random(22);

            for (int i = 0; i < _treeCount; i++)
            {
                float angle = i / (float)_treeCount * Mathf.PI * 2f;
                float dist = 8f + (float)rng.NextDouble() * 3f;
                float x = Mathf.Sin(angle) * dist;
                float z = Mathf.Cos(angle) * dist;

                float h = 4f + (float)rng.NextDouble() * 2f;
                Cylinder($"TreeTrunk_{i}", new Vector3(x, h / 2f, z),
                    new Vector3(0.3f, h, 0.3f), trunkMat);

                var lm = rng.Next(2) == 0 ? leafMat : darkLeafMat;
                Sphere($"TreeLeaves_{i}", new Vector3(x, h + 0.5f, z),
                    new Vector3(1.8f, 1.2f, 1.8f), lm);
                Sphere($"TreeLeavesB_{i}", new Vector3(x + 0.5f, h + 0.2f, z + 0.3f),
                    new Vector3(1.2f, 0.8f, 1.2f), lm);
            }
        }

        private void BuildStonePath()
        {
            var stoneMat = MakeMat(GongbiColors.Bluestone, 0.005f);
            var rng = new System.Random(44);
            // Path from boundary to pavilion steps
            for (int i = 0; i < 8; i++)
            {
                float z = 5f + i * 1.2f;
                float x = (float)(rng.NextDouble() * 2 - 1) * 0.4f;
                Cube($"PathStone_{i}", new Vector3(x, 0.06f, z),
                    new Vector3(1.0f, 0.1f, 0.7f), stoneMat);
            }
        }

        private void BuildMeditationPoint()
        {
            var cushionMat = MakeMat(new Color(0.55f, 0.15f, 0.12f), 0.006f);
            // Meditation cushion inside pavilion
            Cylinder("MeditationCushion", new Vector3(0, 0.25f, 0),
                new Vector3(0.7f, 0.15f, 0.7f), cushionMat);

            // Small incense burner
            var burnerMat = MakeMat(GongbiColors.Gold, 0.008f);
            Cylinder("IncenseBurner", new Vector3(0, 0.3f, 1f),
                new Vector3(0.15f, 0.2f, 0.15f), burnerMat);
        }

        private void BuildBoundaryWalls()
        {
            var wallMat = MakeMat(new Color(0.2f, 0.22f, 0.2f), 0.003f);
            for (int side = -1; side <= 1; side += 2)
            {
                for (int dir = 0; dir < 2; dir++)
                {
                    float x = side * 10f;
                    float z = dir == 0 ? 0 : 10f;
                    float w = dir == 0 ? 0.5f : 20f;
                    float d = dir == 0 ? 20f : 0.5f;
                    var wall = Cube($"BW_{side}_{dir}", new Vector3(x, 1f, z),
                        new Vector3(w, 2f, d), wallMat);
                    wall.GetComponent<Renderer>().enabled = false;
                }
            }
        }
    }
}
