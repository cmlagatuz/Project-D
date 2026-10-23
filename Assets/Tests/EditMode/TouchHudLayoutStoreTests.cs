// Project D | Codename: Zantetsuken — TouchHudLayoutStoreTests.cs (custom HUD layout persistence)
using NUnit.Framework;
using UnityEngine;
using ProjectD.UI.HudEditor;

namespace ProjectD.Tests
{
    [TestFixture]
    public sealed class TouchHudLayoutStoreTests
    {
        // Mirrors TouchHudLayoutStore's private key so the tests can stash and restore a
        // developer's real saved layout instead of clobbering it.
        private const string Key = "ProjectD.TouchHudLayout.v1";

        private bool _hadSaved;
        private string _savedValue;

        [SetUp]
        public void SetUp()
        {
            _hadSaved = PlayerPrefs.HasKey(Key);
            _savedValue = _hadSaved ? PlayerPrefs.GetString(Key) : null;
            TouchHudLayoutStore.ClearSaved();
        }

        [TearDown]
        public void TearDown()
        {
            if (_hadSaved) { PlayerPrefs.SetString(Key, _savedValue); PlayerPrefs.Save(); }
            else TouchHudLayoutStore.ClearSaved();
        }

        private static HudElementLayout Layout(string id, float x, float y, float scale, float alpha)
            => new HudElementLayout { ElementId = id, X = x, Y = y, Scale = scale, Alpha = alpha };

        [Test]
        public void NothingSaved_LoadsAnEmptySet()
        {
            Assert.IsFalse(TouchHudLayoutStore.HasSavedLayout());
            Assert.AreEqual(0, TouchHudLayoutStore.Load().Count);
        }

        [Test]
        public void SaveThenLoad_RoundTripsPositionScaleAndOpacity()
        {
            TouchHudLayoutStore.Save(new[]
            {
                Layout("MoveStick", 220f, 220.5f, 1.25f, 0.8f),
                Layout("Ability_Q", -250f, 210f, 0.75f, 1f)
            });

            var loaded = TouchHudLayoutStore.Load();

            Assert.IsTrue(TouchHudLayoutStore.HasSavedLayout());
            Assert.AreEqual(2, loaded.Count);
            Assert.AreEqual(220f, loaded["MoveStick"].X, 0.001f);
            Assert.AreEqual(220.5f, loaded["MoveStick"].Y, 0.001f);
            Assert.AreEqual(1.25f, loaded["MoveStick"].Scale, 0.001f);
            Assert.AreEqual(0.8f, loaded["MoveStick"].Alpha, 0.001f);
            Assert.AreEqual(-250f, loaded["Ability_Q"].X, 0.001f);
            Assert.AreEqual(0.75f, loaded["Ability_Q"].Scale, 0.001f);
        }

        [Test]
        public void Save_ReplacesThePreviousLayoutRatherThanMerging()
        {
            TouchHudLayoutStore.Save(new[] { Layout("Ability_Q", 1f, 2f, 1f, 1f) });
            TouchHudLayoutStore.Save(new[] { Layout("Ability_W", 3f, 4f, 1f, 1f) });

            var loaded = TouchHudLayoutStore.Load();

            Assert.AreEqual(1, loaded.Count);
            Assert.IsTrue(loaded.ContainsKey("Ability_W"));
        }

        [Test]
        public void ClearSaved_RemovesTheLayout()
        {
            TouchHudLayoutStore.Save(new[] { Layout("Ability_Q", 1f, 2f, 1f, 1f) });

            TouchHudLayoutStore.ClearSaved();

            Assert.IsFalse(TouchHudLayoutStore.HasSavedLayout());
            Assert.AreEqual(0, TouchHudLayoutStore.Load().Count);
        }

        [Test]
        public void Load_SkipsMalformedRows_AndKeepsTheGoodOnes()
        {
            PlayerPrefs.SetString(Key, "broken;ok~1~2~3~0.5;too~few~parts;nan~a~2~3~4");

            var loaded = TouchHudLayoutStore.Load();

            Assert.AreEqual(1, loaded.Count);
            Assert.AreEqual(2f, loaded["ok"].Y, 0.001f);
            Assert.AreEqual(0.5f, loaded["ok"].Alpha, 0.001f);
        }

        [Test]
        public void Save_WithNoElements_LoadsAsEmpty()
        {
            TouchHudLayoutStore.Save(new HudElementLayout[0]);

            Assert.AreEqual(0, TouchHudLayoutStore.Load().Count);
        }
    }
}
