using UnityEngine;

namespace InkRidge.Environment
{
    public class PavilionSceneBuilder : SceneBuilder
    {
        protected override void Build()
        {
            base.Build();
            BuildGround();
            BuildPavilion();
            BuildMeditationPoint();
            SetupLighting(GongbiColors.WarmLight, new Vector3(0.4f, -0.6f, 0.5f));
            SetupFog(new Color(0.92f, 0.90f, 0.88f), 0.035f);
        }

        private void BuildGround()
        {
            var groundMat = MakeMat(GongbiColors.GrayStone, 0.008f);
            Plane("Ground", Vector3.zero, new Vector3(10, 1, 10), groundMat);
        }

        private void BuildPavilion()
        {
            var woodMat = MakeMat(GongbiColors.DarkWood, 0.012f);
            var roofMat = MakeMat(GongbiColors.CinnabarRoof, 0.015f);
            var tileMat = MakeMat(GongbiColors.GrayTileRoof, 0.01f);

            float pillarHeight = 3.5f;
            float size = 3f;
            Vector3[] corners = {
                new Vector3(-size, 0, -size),
                new Vector3(size, 0, -size),
                new Vector3(-size, 0, size),
                new Vector3(size, 0, size)
            };
            foreach (var corner in corners)
            {
                Cylinder("Pillar", new Vector3(corner.x, pillarHeight / 2f, corner.z),
                    new Vector3(0.3f, pillarHeight, 0.3f), woodMat);
            }

            Cube("RoofBase", new Vector3(0, pillarHeight, 0),
                new Vector3(size * 2.5f, 0.2f, size * 2.5f), woodMat);

            Cube("RoofTop", new Vector3(0, pillarHeight + 1.5f, 0),
                new Vector3(size * 2f, 0.15f, size * 2f), roofMat);

            Cube("RoofEave", new Vector3(0, pillarHeight + 0.2f, 0),
                new Vector3(size * 3f, 0.1f, size * 3f), tileMat);

            var floorMat = MakeMat(GongbiColors.OchreWall, 0.005f);
            Cube("PavilionFloor", new Vector3(0, 0.05f, 0),
                new Vector3(size * 1.8f, 0.1f, size * 1.8f), floorMat);
        }

        private void BuildMeditationPoint()
        {
            var mat = MakeMat(GongbiColors.OchreWall, 0.01f);
            Cube("MeditationCushion", new Vector3(0, 0.15f, 0),
                new Vector3(0.8f, 0.2f, 0.8f), mat);
        }
    }
}
