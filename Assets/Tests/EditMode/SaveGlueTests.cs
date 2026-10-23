// Project D | Codename: Zantetsuken — SaveGlueTests.cs (profile serialization + in-memory store)
using NUnit.Framework;
using ProjectD.P15.Save;

namespace ProjectD.Tests
{
    [TestFixture]
    public sealed class SaveGlueTests
    {
        private static PlayerProfileSave SampleProfile()
        {
            var p = new PlayerProfileSave
            {
                playerId = "p1", accountLevel = 3, xpIntoLevel = 45, kp = 1200, worldRiftRating = 1010
            };
            p.ownedChampions.Add("a");
            p.ownedChampions.Add("b");
            p.ownedCosmetics.Add("c");
            return p;
        }

        [Test]
        public void Serialize_UsesTheDocumentedPipeLayout()
        {
            Assert.AreEqual("p1|3|45|1200|1010|a,b|c", SaveGlue.Serialize(SampleProfile()));
        }

        [Test]
        public void Serialize_Null_IsEmptyString()
        {
            Assert.AreEqual("", SaveGlue.Serialize(null));
        }

        [Test]
        public void RoundTrip_PreservesEveryField()
        {
            var restored = SaveGlue.Deserialize(SaveGlue.Serialize(SampleProfile()));

            Assert.AreEqual("p1", restored.playerId);
            Assert.AreEqual(3, restored.accountLevel);
            Assert.AreEqual(45, restored.xpIntoLevel);
            Assert.AreEqual(1200L, restored.kp);
            Assert.AreEqual(1010, restored.worldRiftRating);
            CollectionAssert.AreEqual(new[] { "a", "b" }, restored.ownedChampions);
            CollectionAssert.AreEqual(new[] { "c" }, restored.ownedCosmetics);
            Assert.IsTrue(SaveGlue.RoundTrips(SampleProfile()));
        }

        [Test]
        public void RoundTrip_WithEmptyCollections()
        {
            var p = new PlayerProfileSave { playerId = "solo" };

            var restored = SaveGlue.Deserialize(SaveGlue.Serialize(p));

            Assert.AreEqual("solo", restored.playerId);
            Assert.AreEqual(0, restored.ownedChampions.Count);
            Assert.AreEqual(0, restored.ownedCosmetics.Count);
            Assert.IsTrue(SaveGlue.RoundTrips(p));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("garbage")]
        [TestCase("p|notanumber|2|3|4")]
        [TestCase("p|1|2")]
        public void Deserialize_BadInput_YieldsADefaultProfileInsteadOfThrowing(string data)
        {
            var restored = SaveGlue.Deserialize(data);

            Assert.IsNotNull(restored);
            Assert.AreEqual(1, restored.accountLevel);
            Assert.AreEqual(0L, restored.kp);
            Assert.AreEqual(1000, restored.worldRiftRating);
        }

        [Test]
        public void SaveStore_LoadingAnUnknownPlayer_ReturnsAFreshProfileWithTheId()
        {
            var store = new SaveStore();

            var p = store.Load("newbie");

            Assert.AreEqual("newbie", p.playerId);
            Assert.AreEqual(1, p.accountLevel);
            Assert.IsFalse(store.Has("newbie"));
        }

        [Test]
        public void SaveStore_SaveThenLoad_ReturnsTheSavedData_PerPlayer()
        {
            var store = new SaveStore();
            store.Save("p1", SampleProfile());

            Assert.IsTrue(store.Has("p1"));
            Assert.AreEqual(1200L, store.Load("p1").kp);
            Assert.IsFalse(store.Has("p2"));
        }
    }
}
