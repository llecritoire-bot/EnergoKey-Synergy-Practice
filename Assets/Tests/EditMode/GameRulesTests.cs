using NUnit.Framework;

namespace EnergoKey.Tests.EditMode
{
    public sealed class GameRulesTests
    {
        [Test]
        public void Constants_MatchDesignSpecification()
        {
            Assert.That(GameRules.MaxLevel, Is.EqualTo(3));
            Assert.That(GameRules.StartingLives, Is.EqualTo(3));
        }

        [TestCase(0, 3, 150)]
        [TestCase(3, 3, 450)]
        [TestCase(5, 1, 550)]
        [TestCase(-2, -1, 0)]
        public void CalculateScore_UsesCrystalsAndLivesAndClampsNegatives(
            int collectedCrystals,
            int remainingLives,
            int expected)
        {
            Assert.That(GameRules.CalculateScore(collectedCrystals, remainingLives), Is.EqualTo(expected));
        }

        [TestCase(0, 3)]
        [TestCase(1, 3)]
        [TestCase(2, 4)]
        [TestCase(3, 5)]
        [TestCase(4, 5)]
        public void CrystalCountForLevel_ClampsToSupportedRange(int level, int expected)
        {
            Assert.That(GameRules.CrystalCountForLevel(level), Is.EqualTo(expected));
        }

        [TestCase(0, 2)]
        [TestCase(1, 2)]
        [TestCase(2, 4)]
        [TestCase(3, 6)]
        [TestCase(4, 6)]
        public void HazardCountForLevel_ClampsToSupportedRange(int level, int expected)
        {
            Assert.That(GameRules.HazardCountForLevel(level), Is.EqualTo(expected));
        }
    }
}
