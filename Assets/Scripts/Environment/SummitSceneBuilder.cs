using UnityEngine;

namespace InkRidge.Environment
{
    public class SummitSceneBuilder : SceneBuilder
    {
        protected override void Build()
        {
            base.Build();
            BuildSummit();
            BuildStarfield();
            BuildMeditationPoint();
            SetupLighting(new Color(0.6f, 0.65f, 0.8f), new Vector3(0.2f, -0.5f, 0.8f), 0.5f);
            SetupFog(new Color(0.05f, 0.05f, 0.12f), 0.01f);
        }

        private void BuildSummit()
        {
            var rockMat = MakeMat(GongbiColors.GrayStone, 0.012f);
            Cylinder("SummitPlatform", new Vector3(0, 0.1f, 0),
                new Vector3(5f, 0.2f, 5f), rockMat);
        }

        private void BuildStarfield()
        {
            var starObj = new GameObject("Starfield");
            starObj.transform.SetParent(_root.transform);
            var ps = starObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = new Color(0.9f, 0.9f, 1f);
            main.startSize = 0.08f;
            main.startLifetime = 999f;
            main.loop = false;
            main.maxParticles = 500;

            var emission = ps.emission;
            emission.rateOverTime = 500;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 50f;

            ps.Play();
        }

        private void BuildMeditationPoint()
        {
            var mat = MakeMat(GongbiColors.Bluestone, 0.01f);
            Cube("MeditationPlatform", new Vector3(0, 0.25f, 0),
                new Vector3(2f, 0.2f, 2f), mat);
        }
    }
}
