using System;
using NUnit.Framework;
using UnityEngine;

namespace EnergoKey.Tests.EditMode
{
    public sealed class SaveServiceTests
    {
        private IDisposable temporaryKeys;
        private float previousListenerVolume;

        [SetUp]
        public void SetUp()
        {
            previousListenerVolume = AudioListener.volume;
            temporaryKeys = SaveService.UseTemporaryKeysForTests(Guid.NewGuid().ToString("N"));
        }

        [TearDown]
        public void TearDown()
        {
            temporaryKeys?.Dispose();
            temporaryKeys = null;
            AudioListener.volume = previousListenerVolume;
        }

        [Test]
        public void NewSave_UsesDocumentedDefaults()
        {
            Assert.That(SaveService.UnlockedLevel, Is.EqualTo(1));
            Assert.That(SaveService.BestScore, Is.EqualTo(0));
            Assert.That(SaveService.MasterVolume, Is.EqualTo(0.8f).Within(0.0001f));
            Assert.That(SaveService.TutorialSeen, Is.False);
        }

        [Test]
        public void RegisterVictory_UnlocksNextLevelAndStoresBestScore()
        {
            SaveService.RegisterVictory(1, 450);

            Assert.That(SaveService.UnlockedLevel, Is.EqualTo(2));
            Assert.That(SaveService.BestScore, Is.EqualTo(450));
        }

        [Test]
        public void RegisterVictory_DoesNotLowerProgressOrBestScore()
        {
            SaveService.RegisterVictory(2, 650);
            SaveService.RegisterVictory(1, 200);

            Assert.That(SaveService.UnlockedLevel, Is.EqualTo(3));
            Assert.That(SaveService.BestScore, Is.EqualTo(650));
        }

        [Test]
        public void RegisterVictory_OnFinalLevelNeverCreatesLevelFour()
        {
            SaveService.RegisterVictory(2, 500);
            SaveService.RegisterVictory(3, 700);

            Assert.That(SaveService.UnlockedLevel, Is.EqualTo(GameRules.MaxLevel));
            Assert.That(SaveService.BestScore, Is.EqualTo(700));
        }

        [TestCase(-0.4f, 0f)]
        [TestCase(0.35f, 0.35f)]
        [TestCase(1.8f, 1f)]
        public void SetMasterVolume_ClampsAndPersists(float requested, float expected)
        {
            SaveService.SetMasterVolume(requested);

            Assert.That(SaveService.MasterVolume, Is.EqualTo(expected).Within(0.0001f));
            Assert.That(AudioListener.volume, Is.EqualTo(expected).Within(0.0001f));
        }

        [Test]
        public void MarkTutorialSeen_PersistsFlag()
        {
            SaveService.MarkTutorialSeen();

            Assert.That(SaveService.TutorialSeen, Is.True);
        }

        [Test]
        public void ResetProgressForDevelopment_ClearsProgressButPreservesVolume()
        {
            SaveService.RegisterVictory(1, 450);
            SaveService.SetMasterVolume(0.25f);
            SaveService.MarkTutorialSeen();

            SaveService.ResetProgressForDevelopment();

            Assert.That(SaveService.UnlockedLevel, Is.EqualTo(1));
            Assert.That(SaveService.BestScore, Is.EqualTo(0));
            Assert.That(SaveService.TutorialSeen, Is.False);
            Assert.That(SaveService.MasterVolume, Is.EqualTo(0.25f).Within(0.0001f));
        }
    }
}
