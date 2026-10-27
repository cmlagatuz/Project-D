// Project D | Codename: Zantetsuken — DuelSummonFx.cs
// Particle effects for the summons, built from code so no art assets are needed (original placeholder effects):
//   Shiva   - frost motes and icy wind streaks blowing back, orbiting ice shards, twinkling sparkles, a frost ring on the ground
//   Garuda  - a ground cyclone, wind spiralling up around her, gusts streaming off the wings, drifting feathers
//   Bahamut - a rune ring on the ground, rising embers, sparkles
// Every effect is soft shapes and tints (never colour alone: the shapes differ per summon). Textures are generated at runtime.
using System.Collections.Generic;
using UnityEngine;

namespace ProjectD.DuelDemo
{
    public static class DuelSummonFx
    {
        private static readonly Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();
        private static Mesh _crystal;
        private static Material _fallback;

        /// <summary>Adds the effects for a summon under its view root. fxMat is optional (additive particle material from the scene builder).</summary>
        public static void Attach(Transform root, string id, float height, Material fxMat)
        {
            float s = Mathf.Max(0.5f, height / 4.8f);
            float R = 1.7f * s;
            var baseMat = BaseMaterial(fxMat);
            if (baseMat == null) return;

            var holder = new GameObject("Fx_" + id).transform;
            holder.SetParent(root, false);

            switch (id)
            {
                case "shiva": BuildShiva(holder, baseMat, s, R, height); break;
                case "garuda": BuildGaruda(holder, baseMat, s, R, height); break;
                case "bahamut": BuildBahamut(holder, baseMat, s, R, height); break;
            }
        }

        // ------------------------------------------------------------------ the three summons

        private static void BuildShiva(Transform p, Material mat, float s, float R, float h)
        {
            var ice = new Color(0.78f, 0.94f, 1f);
            var white = new Color(0.95f, 0.99f, 1f);

            // frost ring on the ground, slowly breathing
            var ring = NewSystem(p, "FrostRing", Mat(mat, "ring"), 4f, 4);
            Place(ring, 0.07f);
            Burst(ring, 1);
            SetMain(ring, 4f, 4f, 0f, 0f, R * 2.8f, R * 2.8f, new Color(0.7f, 0.92f, 1f, 0.55f));
            Colors(ring, Fade(new Color(0.7f, 0.92f, 1f), 0.6f));
            Sizes(ring, 0.92f, 1.08f);
            Render(ring, ParticleSystemRenderMode.HorizontalBillboard);

            // frost motes circling up from the ground
            var motes = NewSystem(p, "FrostMotes", Mat(mat, "soft"), 5f, 120);
            Disc(motes, R * 1.1f, 0.25f, 0.1f);
            Rate(motes, 30f);
            SetMain(motes, 3f, 5f, 0.1f, 0.4f, 0.05f * s, 0.12f * s, new Color(ice.r, ice.g, ice.b, 0.9f));
            Gravity(motes, -0.04f);
            Velocity(motes, 0f, 0.15f, 0f, 0.5f);
            Colors(motes, Fade(ice, 0.9f));
            Render(motes, ParticleSystemRenderMode.Billboard);

            // icy wind streaks blowing back and across her
            var wind = NewSystem(p, "IceWind", Mat(mat, "soft"), 2f, 160);
            BoxShape(wind, new Vector3(R * 2.6f, h * 0.8f, R * 1.4f), new Vector3(0f, h * 0.45f, R * 0.4f));
            Rate(wind, 36f);
            SetMain(wind, 0.8f, 1.4f, 0f, 0f, 0.04f * s, 0.075f * s, new Color(white.r, white.g, white.b, 0.6f));
            Velocity(wind, -3.4f * s, 0.2f, -2.2f * s, 0f);
            Colors(wind, Fade(white, 0.65f));
            Stretch(wind, 0.14f, 3f);

            // ice shards orbiting her
            var shards = NewSystem(p, "IceShards", ShardMat(mat), 7f, 30);
            Disc(shards, R * 1.05f, 0.4f, h * 0.2f);
            Rate(shards, 4f);
            SetMain(shards, 5f, 7f, 0f, 0f, 0.2f * s, 0.38f * s, new Color(0.7f, 0.93f, 1f, 0.9f));
            Velocity(shards, 0f, 0.18f, 0f, 0.7f);
            Colors(shards, Fade(new Color(0.75f, 0.95f, 1f), 0.9f));
            RandomRotation3D(shards);
            var shardR = shards.GetComponent<ParticleSystemRenderer>();
            shardR.renderMode = ParticleSystemRenderMode.Mesh;
            shardR.mesh = Crystal();

            // twinkling sparkles around the body
            Sparkles(p, mat, h * 0.42f, h * 0.5f, 22f, 0.1f * s, 0.2f * s, new Color(0.9f, 0.98f, 1f));
        }

        private static void BuildGaruda(Transform p, Material mat, float s, float R, float h)
        {
            var wind = new Color(0.75f, 1f, 0.88f);

            // ground cyclone
            var cyc = NewSystem(p, "Cyclone", Mat(mat, "swirl"), 3f, 4);
            Place(cyc, 0.08f);
            Burst(cyc, 1);
            SetMain(cyc, 3f, 3f, 0f, 0f, R * 3.2f, R * 3.2f, new Color(0.7f, 1f, 0.85f, 0.5f));
            Colors(cyc, Fade(new Color(0.7f, 1f, 0.85f), 0.55f));
            var rot = cyc.rotationOverLifetime;
            rot.enabled = true;
            rot.z = new ParticleSystem.MinMaxCurve(Mathf.Deg2Rad * 150f);
            Render(cyc, ParticleSystemRenderMode.HorizontalBillboard);

            // wind spiralling up around her
            var spiral = NewSystem(p, "WindSpiral", Mat(mat, "soft"), 2.2f, 220);
            Disc(spiral, R * 1.25f, 0.5f, 0.1f);
            Rate(spiral, 46f);
            SetMain(spiral, 1.5f, 2.2f, 0f, 0f, 0.05f * s, 0.09f * s, new Color(wind.r, wind.g, wind.b, 0.65f));
            Velocity(spiral, 0f, 2.2f * s, 0f, 2.8f);
            Colors(spiral, Fade(wind, 0.7f));
            Stretch(spiral, 0.1f, 2.6f);

            // gusts streaming back off the wings
            var gust = NewSystem(p, "Gusts", Mat(mat, "soft"), 1.4f, 160);
            BoxShape(gust, new Vector3(h * 1.1f, h * 0.35f, 0.6f), new Vector3(0f, h * 0.55f, 0f));
            Rate(gust, 28f);
            SetMain(gust, 0.7f, 1.1f, 0f, 0f, 0.04f * s, 0.08f * s, new Color(1f, 1f, 1f, 0.6f));
            Velocity(gust, 0f, 0.4f, -5.2f * s, 0f);
            Colors(gust, Fade(new Color(0.9f, 1f, 0.95f), 0.6f));
            Stretch(gust, 0.16f, 3.4f);

            // drifting feathers and leaves
            var leaf = NewSystem(p, "Feathers", Mat(mat, "leaf"), 3f, 40);
            Disc(leaf, R * 1.3f, 0.9f, h * 0.3f);
            Rate(leaf, 6f);
            SetMain(leaf, 2f, 3.2f, 0f, 0f, 0.12f * s, 0.2f * s, new Color(0.6f, 1f, 0.72f, 0.9f));
            Gravity(leaf, 0.05f);
            Velocity(leaf, 0.4f, 0.2f, -0.6f, 0.9f);
            Colors(leaf, Fade(new Color(0.6f, 1f, 0.72f), 0.9f));
            var lr = leaf.rotationOverLifetime;
            lr.enabled = true;
            lr.z = new ParticleSystem.MinMaxCurve(Mathf.Deg2Rad * 60f, Mathf.Deg2Rad * 220f);
            Render(leaf, ParticleSystemRenderMode.Billboard);

            Sparkles(p, mat, h * 0.45f, h * 0.55f, 10f, 0.08f * s, 0.16f * s, new Color(0.85f, 1f, 0.92f));
        }

        private static void BuildBahamut(Transform p, Material mat, float s, float R, float h)
        {
            var violet = new Color(0.65f, 0.5f, 1f);
            var gold = new Color(1f, 0.78f, 0.4f);

            var ring = NewSystem(p, "RuneRing", Mat(mat, "ring"), 5f, 4);
            Place(ring, 0.07f);
            Burst(ring, 1);
            SetMain(ring, 5f, 5f, 0f, 0f, R * 3.3f, R * 3.3f, new Color(violet.r, violet.g, violet.b, 0.5f));
            Colors(ring, Fade(violet, 0.55f));
            Sizes(ring, 0.94f, 1.06f);
            var rr = ring.rotationOverLifetime;
            rr.enabled = true;
            rr.z = new ParticleSystem.MinMaxCurve(Mathf.Deg2Rad * 20f);
            Render(ring, ParticleSystemRenderMode.HorizontalBillboard);

            var embers = NewSystem(p, "Embers", Mat(mat, "soft"), 4f, 120);
            Disc(embers, R * 1.2f, 0.6f, 0.1f);
            Rate(embers, 22f);
            SetMain(embers, 3f, 4.5f, 0f, 0f, 0.06f * s, 0.13f * s, new Color(gold.r, gold.g, gold.b, 0.9f));
            Velocity(embers, 0f, 0.9f * s, 0f, 0.3f);
            Colors(embers, Fade(gold, 0.9f));
            Render(embers, ParticleSystemRenderMode.Billboard);

            Sparkles(p, mat, h * 0.4f, h * 0.5f, 12f, 0.1f * s, 0.2f * s, new Color(0.85f, 0.8f, 1f));
        }

        private static void Sparkles(Transform p, Material mat, float radius, float centerY, float rate, float min, float max, Color c)
        {
            var sp = NewSystem(p, "Sparkles", Mat(mat, "star"), 1f, 80);
            var sh = sp.shape;
            sh.enabled = true;
            sh.shapeType = ParticleSystemShapeType.Sphere;
            sh.radius = radius;
            sh.radiusThickness = 1f;
            sh.position = new Vector3(0f, centerY, 0f);
            Rate(sp, rate);
            SetMain(sp, 0.5f, 0.9f, 0f, 0f, min, max, new Color(c.r, c.g, c.b, 1f));
            Colors(sp, Fade(c, 1f));
            Sizes(sp, 0f, 1f, 0f);   // grows and shrinks: a twinkle
            Render(sp, ParticleSystemRenderMode.Billboard);
        }

        // ------------------------------------------------------------------ particle helpers

        private static ParticleSystem NewSystem(Transform parent, string name, Material mat, float duration, int max)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);   // modules like duration can only change while stopped
            var main = ps.main;
            main.duration = duration;
            main.loop = true;
            main.playOnAwake = true;
            main.maxParticles = max;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;   // the effect follows (and turns with) the summon
            var shape = ps.shape;
            shape.enabled = false;
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            var r = go.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            ps.Play();
            return ps;
        }

        private static void SetMain(ParticleSystem ps, float lifeMin, float lifeMax, float speedMin, float speedMax, float sizeMin, float sizeMax, Color start)
        {
            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(lifeMin, lifeMax);
            main.startSpeed = new ParticleSystem.MinMaxCurve(speedMin, speedMax);
            main.startSize = new ParticleSystem.MinMaxCurve(sizeMin, sizeMax);
            main.startColor = new ParticleSystem.MinMaxGradient(start);
        }

        private static void Place(ParticleSystem ps, float y) { ps.transform.localPosition = new Vector3(0f, y, 0f); }

        private static void Rate(ParticleSystem ps, float rate)
        {
            var e = ps.emission;
            e.rateOverTime = rate;
        }

        private static void Burst(ParticleSystem ps, int count)
        {
            var e = ps.emission;
            e.rateOverTime = 0f;
            e.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });
        }

        private static void Gravity(ParticleSystem ps, float g)
        {
            var main = ps.main;
            main.gravityModifier = g;
        }

        /// <summary>A flat disc on the ground (the Circle shape lies in XY, so it is turned to face up).</summary>
        private static void Disc(ParticleSystem ps, float radius, float thickness, float y)
        {
            var sh = ps.shape;
            sh.enabled = true;
            sh.shapeType = ParticleSystemShapeType.Circle;
            sh.radius = radius;
            sh.radiusThickness = thickness;
            sh.rotation = new Vector3(-90f, 0f, 0f);
            sh.position = new Vector3(0f, y, 0f);
        }

        private static void BoxShape(ParticleSystem ps, Vector3 size, Vector3 center)
        {
            var sh = ps.shape;
            sh.enabled = true;
            sh.shapeType = ParticleSystemShapeType.Box;
            sh.scale = size;
            sh.position = center;
        }

        /// <summary>Linear velocity in the root's space plus an optional spin around the vertical axis (orbit).</summary>
        private static void Velocity(ParticleSystem ps, float x, float y, float z, float orbitY)
        {
            var v = ps.velocityOverLifetime;
            v.enabled = true;
            v.space = ParticleSystemSimulationSpace.Local;
            v.x = new ParticleSystem.MinMaxCurve(x);
            v.y = new ParticleSystem.MinMaxCurve(y);
            v.z = new ParticleSystem.MinMaxCurve(z);
            v.orbitalY = new ParticleSystem.MinMaxCurve(orbitY);
        }

        private static void Colors(ParticleSystem ps, Gradient g)
        {
            var c = ps.colorOverLifetime;
            c.enabled = true;
            c.color = new ParticleSystem.MinMaxGradient(g);
        }

        private static Gradient Fade(Color c, float peak)
        {
            var g = new Gradient();
            g.SetKeys(
                new[] { new GradientColorKey(c, 0f), new GradientColorKey(c, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(peak, 0.2f), new GradientAlphaKey(peak, 0.7f), new GradientAlphaKey(0f, 1f) });
            return g;
        }

        private static void Sizes(ParticleSystem ps, float from, float to)
        {
            var sz = ps.sizeOverLifetime;
            sz.enabled = true;
            sz.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, from, 1f, to));
        }

        private static void Sizes(ParticleSystem ps, float from, float mid, float to)
        {
            var sz = ps.sizeOverLifetime;
            sz.enabled = true;
            var curve = new AnimationCurve(new Keyframe(0f, from), new Keyframe(0.5f, mid), new Keyframe(1f, to));
            sz.size = new ParticleSystem.MinMaxCurve(1f, curve);
        }

        private static void Render(ParticleSystem ps, ParticleSystemRenderMode mode)
        {
            ps.GetComponent<ParticleSystemRenderer>().renderMode = mode;
        }

        private static void Stretch(ParticleSystem ps, float velocityScale, float lengthScale)
        {
            var r = ps.GetComponent<ParticleSystemRenderer>();
            r.renderMode = ParticleSystemRenderMode.Stretch;
            r.velocityScale = velocityScale;
            r.lengthScale = lengthScale;
        }

        private static void RandomRotation3D(ParticleSystem ps)
        {
            var main = ps.main;
            main.startRotation3D = true;
            main.startRotationX = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startRotationY = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startRotationZ = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        }

        // ------------------------------------------------------------------ materials and textures

        private static readonly Dictionary<Material, Dictionary<string, Material>> Mats = new Dictionary<Material, Dictionary<string, Material>>();

        private static Material BaseMaterial(Material assigned)
        {
            if (assigned != null) return assigned;
            if (_fallback != null) return _fallback;
            var shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) return null;
            _fallback = new Material(shader);
            return _fallback;
        }

        private static Material Mat(Material baseMat, string textureName)
        {
            if (!Mats.TryGetValue(baseMat, out var perTexture))
            {
                perTexture = new Dictionary<string, Material>();
                Mats[baseMat] = perTexture;
            }
            if (perTexture.TryGetValue(textureName, out var m) && m != null) return m;
            m = new Material(baseMat);
            var tex = GetTexture(textureName);
            if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", tex);
            if (m.HasProperty("_MainTex")) m.SetTexture("_MainTex", tex);
            perTexture[textureName] = m;
            return m;
        }

        private static Material ShardMat(Material baseMat) { return Mat(baseMat, "soft"); }

        private static Texture2D GetTexture(string name)
        {
            if (Textures.TryGetValue(name, out var t) && t != null) return t;
            const int N = 64;
            var tex = new Texture2D(N, N, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            var px = new Color32[N * N];
            for (int y = 0; y < N; y++)
            {
                for (int x = 0; x < N; x++)
                {
                    float dx = (x + 0.5f) / N * 2f - 1f, dy = (y + 0.5f) / N * 2f - 1f;
                    float a = Alpha(name, dx, dy);
                    px[y * N + x] = new Color32(255, 255, 255, (byte)Mathf.Clamp(Mathf.RoundToInt(a * 255f), 0, 255));
                }
            }
            tex.SetPixels32(px);
            tex.Apply(false, true);
            Textures[name] = tex;
            return tex;
        }

        private static float Alpha(string name, float dx, float dy)
        {
            float r = Mathf.Sqrt(dx * dx + dy * dy);
            switch (name)
            {
                case "ring":
                {
                    float edge = (r - 0.84f) / 0.07f;
                    float line = Mathf.Exp(-edge * edge);
                    float inner = r < 0.8f ? 0.10f * (1f - r) : 0f;
                    float tick = r > 0.62f && r < 0.74f ? 0.35f * Mathf.Pow(Mathf.Max(0f, Mathf.Sin(Mathf.Atan2(dy, dx) * 12f)), 6f) : 0f;   // rune marks
                    return Mathf.Clamp01(line + inner + tick) * (r < 1f ? 1f : 0f);
                }
                case "swirl":
                {
                    if (r >= 0.98f || r < 0.08f) return 0f;
                    float theta = Mathf.Atan2(dy, dx);
                    float arm = Mathf.Max(0f, Mathf.Sin(3f * theta + r * 9f));
                    return arm * arm * Mathf.Pow(1f - r, 0.6f) * Mathf.Clamp01((r - 0.08f) * 6f);
                }
                case "star":
                {
                    float ax = Mathf.Abs(dx), ay = Mathf.Abs(dy);
                    float cross = Mathf.Exp(-ax * 18f) * (1f - ay) + Mathf.Exp(-ay * 18f) * (1f - ax);
                    float glow = Mathf.Pow(Mathf.Max(0f, 1f - r), 3f);
                    return Mathf.Clamp01(Mathf.Max(0f, cross) * 0.9f + glow) * (r < 1f ? 1f : 0f);
                }
                case "leaf":
                {
                    float e = (dx / 0.42f) * (dx / 0.42f) + dy * dy * 0.9f;
                    return Mathf.Clamp01((1f - e) * 3f);
                }
                default:   // soft round dot
                {
                    float a = Mathf.Clamp01(1f - r);
                    return a * a * (r < 1f ? 1f : 0f);
                }
            }
        }

        /// <summary>A small elongated crystal (bipyramid) used for ice shards.</summary>
        private static Mesh Crystal()
        {
            if (_crystal != null) return _crystal;
            var m = new Mesh { name = "DuelCrystal" };
            const float w = 0.35f, hgt = 0.9f;
            m.vertices = new[]
            {
                new Vector3(0f, hgt, 0f), new Vector3(0f, -hgt, 0f),
                new Vector3(w, 0f, 0f), new Vector3(0f, 0f, w), new Vector3(-w, 0f, 0f), new Vector3(0f, 0f, -w)
            };
            m.uv = new[]
            {
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)
            };
            m.triangles = new[]
            {
                0, 3, 2, 0, 4, 3, 0, 5, 4, 0, 2, 5,
                1, 2, 3, 1, 3, 4, 1, 4, 5, 1, 5, 2
            };
            m.RecalculateNormals();
            m.RecalculateBounds();
            _crystal = m;
            return m;
        }
    }
}
