using UnityEngine;

namespace InkRidge.Environment
{
    public class PavilionSceneBuilder : SceneBuilder
    {
        [Header("Pavilion Config")]
        [SerializeField] private int _treeCount = 20;
        [SerializeField] private int _bambooCount = 25;

        protected override void Build()
        {
            base.Build();
            SetupSkybox(new Color(0.80f, 0.84f, 0.88f), new Color(0.94f, 0.92f, 0.90f));
            BuildGround();
            BuildPavilion();
            BuildSurroundingTrees();
            BuildBambooBackground();
            BuildStonePath();
            BuildLanterns();
            BuildMeditationPoint();
            BuildBoundaryWalls();
            var fogObj = new GameObject("GradientFog");
            fogObj.transform.SetParent(_root.transform);
            fogObj.AddComponent<GradientFog>();
            var windObj = new GameObject("WindSystem");
            windObj.transform.SetParent(_root.transform);
            var wind = windObj.AddComponent<WindSystem>();
            SetupLighting(GongbiColors.WarmLight, new Vector3(0.4f, -0.5f, 0.5f), 1.0f);
            SetupFog(new Color(0.93f, 0.91f, 0.89f), 0.025f); // reduced fog density
        }

        private void BuildGround()
        {
            var groundMat = MakeMat(new Color(0.42f, 0.44f, 0.40f), 0.006f);
            Plane("Ground", Vector3.zero, new Vector3(14, 1, 14), groundMat);

            var mossMat = MakeMat(new Color(0.20f, 0.32f, 0.16f), 0.004f);
            var rng = new System.Random(11);
            for (int i = 0; i < 15; i++)
            {
                float x = (float)(rng.NextDouble() * 2 - 1) * 10f;
                float z = (float)(rng.NextDouble() * 2 - 1) * 10f;
                if (Mathf.Abs(x) < 4f && Mathf.Abs(z) < 4f) continue;
                Plane($"Moss_{i}", new Vector3(x, 0.01f, z),
                    new Vector3(0.8f + (float)rng.NextDouble() * 0.6f, 1,
                        0.8f + (float)rng.NextDouble() * 0.6f), mossMat);
            }

            // Fallen leaves scatter
            var leafMat = MakeMat(new Color(0.65f, 0.45f, 0.20f), 0.002f);
            for (int i = 0; i < 20; i++)
            {
                float x = (float)(rng.NextDouble() * 2 - 1) * 8f;
                float z = (float)(rng.NextDouble() * 2 - 1) * 8f;
                Sphere($"Leaf_{i}", new Vector3(x, 0.02f, z),
                    new Vector3(0.15f, 0.02f, 0.15f), leafMat);
            }
        }

        private void BuildPavilion()
        {
            var woodMat = MakeMat(GongbiColors.DarkWood, 0.01f);
            var roofMat = MakeMat(GongbiColors.CinnabarRoof, 0.012f);
            var tileMat = MakeMat(GongbiColors.GrayTileRoof, 0.008f);
            var goldMat = MakeMat(GongbiColors.Gold, 0.006f);
            var floorMat = MakeMat(GongbiColors.OchreWall, 0.004f);
            var redMat = MakeMat(GongbiColors.Vermillion, 0.008f);

            float pillarH = 4.0f;
            float size = 3.5f;

            // Stone base — two tiers
            var baseMat = MakeMat(GongbiColors.Bluestone, 0.008f);
            Cube("PavilionBase", new Vector3(0, 0.15f, 0),
                new Vector3(size * 2.3f, 0.3f, size * 2.3f), baseMat);
            Cube("PavilionBase2", new Vector3(0, 0.35f, 0),
                new Vector3(size * 2.0f, 0.1f, size * 2.0f), baseMat);

            // 4 pillars with decorative bases
            Vector3[] corners = {
                new Vector3(-size, 0, -size),
                new Vector3(size, 0, -size),
                new Vector3(-size, 0, size),
                new Vector3(size, 0, size)
            };
            for (int i = 0; i < corners.Length; i++)
            {
                Cylinder($"Pillar_{i}", new Vector3(corners[i].x, pillarH / 2f + 0.4f, corners[i].z),
                    new Vector3(0.35f, pillarH, 0.35f), woodMat);
                // Stone pillar base
                Cylinder($"PillarBase_{i}", new Vector3(corners[i].x, 0.42f, corners[i].z),
                    new Vector3(0.5f, 0.1f, 0.5f), baseMat);
                // Red pillar top cap
                Cylinder($"PillarCap_{i}", new Vector3(corners[i].x, pillarH + 0.35f, corners[i].z),
                    new Vector3(0.45f, 0.08f, 0.45f), redMat);
            }

            // Cross beams (dougong style)
            for (int i = 0; i < 4; i++)
            {
                bool horizontal = i < 2;
                float z = i == 0 ? -size : (i == 1 ? size : 0);
                float x = i == 2 ? -size : (i == 3 ? size : 0);
                Cube($"Beam_{i}", new Vector3(x, pillarH + 0.4f, z),
                    horizontal ? new Vector3(size * 2f, 0.25f, 0.25f) : new Vector3(0.25f, 0.25f, size * 2f), woodMat);
            }

            // Bracket sets (small cubes at corners between beams and roof)
            for (int i = 0; i < 4; i++)
            {
                float sx = (i < 2 ? -1 : 1) * size;
                float sz = (i % 2 == 0 ? -1 : 1) * size;
                Cube($"Bracket_{i}", new Vector3(sx, pillarH + 0.5f, sz),
                    new Vector3(0.4f, 0.2f, 0.4f), woodMat);
            }

            // Multi-layered roof (Chinese style)
            // Layer 1 — wide eave (gray tile)
            Cube("RoofEave", new Vector3(0, pillarH + 0.6f, 0),
                new Vector3(size * 3.4f, 0.15f, size * 3.4f), tileMat);
            // Layer 2 — main roof body (cinnabar red)
            Cube("RoofMain", new Vector3(0, pillarH + 1.6f, 0),
                new Vector3(size * 2.6f, 0.2f, size * 2.6f), roofMat);
            // Layer 3 — roof top (smaller)
            Cube("RoofTop", new Vector3(0, pillarH + 2.6f, 0),
                new Vector3(size * 1.6f, 0.15f, size * 1.6f), roofMat);
            // Ridge ornament — gold sphere + spire
            Sphere("RoofOrnament", new Vector3(0, pillarH + 3.3f, 0),
                new Vector3(0.4f, 0.4f, 0.4f), goldMat);
            Cylinder("RoofSpire", new Vector3(0, pillarH + 3.8f, 0),
                new Vector3(0.08f, 0.5f, 0.08f), goldMat);

            // Upturned eave corners (4 pieces with slight upward angle)
            for (int i = 0; i < 4; i++)
            {
                float sx = (i < 2 ? -1 : 1) * size * 1.7f;
                float sz = (i % 2 == 0 ? -1 : 1) * size * 1.7f;
                Cube($"EaveCorner_{i}", new Vector3(sx, pillarH + 0.8f, sz),
                    new Vector3(0.5f, 0.15f, 0.5f), tileMat);
            }

            // Floor
            Cube("PavilionFloor", new Vector3(0, 0.25f, 0),
                new Vector3(size * 1.8f, 0.1f, size * 1.8f), floorMat);

            // Steps — 3 tiers
            Cube("Step1", new Vector3(0, 0.15f, size + 1.2f),
                new Vector3(2.8f, 0.15f, 0.8f), baseMat);
            Cube("Step2", new Vector3(0, 0.3f, size + 0.7f),
                new Vector3(2.2f, 0.15f, 0.6f), baseMat);
            Cube("Step3", new Vector3(0, 0.45f, size + 0.2f),
                new Vector3(1.8f, 0.15f, 0.5f), baseMat);
        }

        private void BuildSurroundingTrees()
        {
            var trunkMat = MakeWindMat(GongbiColors.DarkWood, 0.3f, 0.008f);
            var leafMat = MakeWindMat(new Color(0.18f, 0.35f, 0.14f), 0.8f, 0.004f);
            var darkLeafMat = MakeWindMat(new Color(0.10f, 0.28f, 0.10f), 0.7f, 0.004f);
            var autumnMat = MakeWindMat(new Color(0.75f, 0.45f, 0.15f), 0.8f, 0.004f); // autumn color
            var rng = new System.Random(22);

            for (int i = 0; i < _treeCount; i++)
            {
                float angle = i / (float)_treeCount * Mathf.PI * 2f;
                float dist = 8f + (float)rng.NextDouble() * 4f;
                float x = Mathf.Sin(angle) * dist;
                float z = Mathf.Cos(angle) * dist;

                float h = 4f + (float)rng.NextDouble() * 3f;
                Cylinder($"TreeTrunk_{i}", new Vector3(x, h / 2f, z),
                    new Vector3(0.3f, h, 0.3f), trunkMat);

                // Randomly use autumn color for variety
                var lm = rng.Next(4) == 0 ? autumnMat : (rng.Next(2) == 0 ? leafMat : darkLeafMat);
                Sphere($"TreeLeaves_{i}", new Vector3(x, h + 0.5f, z),
                    new Vector3(1.8f, 1.2f, 1.8f), lm);
                Sphere($"TreeLeavesB_{i}", new Vector3(x + 0.5f, h + 0.2f, z + 0.3f),
                    new Vector3(1.2f, 0.8f, 1.2f), lm);
            }
        }

        private void BuildBambooBackground()
        {
            var bambooMat = MakeWindMat(GongbiColors.BambooGreen, 0.5f, 0.008f);
            var leafMat = MakeWindMat(GongbiColors.EmeraldGreen, 0.7f, 0.004f);
            var rng = new System.Random(77);

            for (int i = 0; i < _bambooCount; i++)
            {
                float angle = i / (float)_bambooCount * Mathf.PI * 2f;
                float dist = 11f + (float)rng.NextDouble() * 3f;
                float x = Mathf.Sin(angle) * dist;
                float z = Mathf.Cos(angle) * dist;

                float h = 5f + (float)rng.NextDouble() * 3f;
                Cylinder($"Bamboo_{i}", new Vector3(x, h / 2f, z),
                    new Vector3(0.2f, h, 0.2f), bambooMat);
                Sphere($"BambooLeaf_{i}", new Vector3(x, h + 0.3f, z),
                    new Vector3(1.0f, 0.7f, 1.0f), leafMat);
            }
        }

        private void BuildStonePath()
        {
            var stoneMat = MakeMat(GongbiColors.Bluestone, 0.005f);
            var rng = new System.Random(44);
            for (int i = 0; i < 10; i++)
            {
                float z = 5f + i * 1.0f;
                float x = (float)(rng.NextDouble() * 2 - 1) * 0.4f;
                Cube($"PathStone_{i}", new Vector3(x, 0.06f, z),
                    new Vector3(1.0f + (float)rng.NextDouble() * 0.3f, 0.1f, 0.7f), stoneMat);
            }
        }

        private void BuildLanterns()
        {
            var lanternMat = MakeMat(GongbiColors.CinnabarRoof, 0.008f);
            var glowMat = MakeMat(new Color(1.0f, 0.85f, 0.4f, 0.6f), 0.002f);

            // Stone lanterns flanking the path
            for (int side = -1; side <= 1; side += 2)
            {
                for (int i = 0; i < 2; i++)
                {
                    float z = 5f + i * 3f;
                    var base_obj = Cube($"LanternBase_{side}_{i}", new Vector3(side * 1.5f, 0.3f, z),
                        new Vector3(0.3f, 0.6f, 0.3f), lanternMat);
                    // Lantern glow
                    Sphere($"LanternGlow_{side}_{i}", new Vector3(side * 1.5f, 0.7f, z),
                        new Vector3(0.3f, 0.3f, 0.3f), glowMat);
                    // Lantern roof
                    Cube($"LanternRoof_{side}_{i}", new Vector3(side * 1.5f, 0.9f, z),
                        new Vector3(0.35f, 0.08f, 0.35f), lanternMat);
                }
            }
        }

        private void BuildMeditationPoint()
        {
            var cushionMat = MakeMat(new Color(0.55f, 0.15f, 0.12f), 0.006f);
            Cylinder("MeditationCushion", new Vector3(0, 0.3f, 0),
                new Vector3(0.7f, 0.15f, 0.7f), cushionMat);

            // Small incense burner
            var burnerMat = MakeMat(GongbiColors.Gold, 0.008f);
            Cylinder("IncenseBase", new Vector3(0, 0.35f, 1f),
                new Vector3(0.2f, 0.1f, 0.2f), burnerMat);
            Cylinder("IncenseBody", new Vector3(0, 0.45f, 1f),
                new Vector3(0.12f, 0.2f, 0.12f), burnerMat);
        }

        private void BuildBoundaryWalls()
        {
            var wallMat = MakeMat(new Color(0.2f, 0.22f, 0.2f), 0.003f);
            for (int side = -1; side <= 1; side += 2)
            {
                for (int dir = 0; dir < 2; dir++)
                {
                    float x = side * 12f;
                    float z = dir == 0 ? 0 : 12f;
                    float w = dir == 0 ? 0.5f : 24f;
                    float d = dir == 0 ? 24f : 0.5f;
                    var wall = Cube($"BW_{side}_{dir}", new Vector3(x, 1f, z),
                        new Vector3(w, 2f, d), wallMat);
                    wall.GetComponent<Renderer>().enabled = false;
                }
            }
        }
    }
}
