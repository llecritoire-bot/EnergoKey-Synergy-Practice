using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace EnergoKey.Tests.PlayMode
{
    public sealed class GameplayIntegrationTests : PlayModeTestHarness
    {
        [UnityTest]
        public IEnumerator T01_ScreenButtonsMoveInFourDirections_AndBoundaryWallBlocksPlayer()
        {
            yield return LoadSceneAndWait("Level1");

            PlayerController player = FindInActiveScene<PlayerController>();
            Assert.That(player, Is.Not.Null);

            (string name, Vector2 direction)[] controls =
            {
                ("LeftButton", Vector2.left),
                ("RightButton", Vector2.right),
                ("UpButton", Vector2.up),
                ("DownButton", Vector2.down)
            };

            foreach ((string name, Vector2 direction) control in controls)
            {
                player.SetSpawn(Vector3.zero);
                yield return new WaitForFixedUpdate();

                MobileInputButton input = FindObject(control.name).GetComponent<MobileInputButton>();
                Assert.That(input, Is.Not.Null, $"{control.name} has no MobileInputButton.");

                Vector2 before = player.transform.position;
                PointerDown(input);
                yield return new WaitForSecondsRealtime(0.16f);
                PointerUp(input);
                yield return new WaitForFixedUpdate();

                Vector2 displacement = (Vector2)player.transform.position - before;
                float forwardDistance = Vector2.Dot(displacement, control.direction);
                Vector2 perpendicular = new Vector2(-control.direction.y, control.direction.x);
                Assert.That(forwardDistance, Is.GreaterThan(0.25f),
                    $"{control.name} did not move the player in its configured direction.");
                Assert.That(Mathf.Abs(Vector2.Dot(displacement, perpendicular)), Is.LessThan(0.12f),
                    $"{control.name} introduced unexpected perpendicular movement.");
            }

            player.SetSpawn(new Vector3(-7f, 0f, 0f));
            yield return new WaitForFixedUpdate();
            MobileInputButton left = FindObject("LeftButton").GetComponent<MobileInputButton>();
            PointerDown(left);
            yield return new WaitForSecondsRealtime(0.5f);
            PointerUp(left);
            yield return new WaitForFixedUpdate();

            Assert.That(player.transform.position.x, Is.GreaterThan(-7.7f),
                "The player crossed the left boundary wall while the on-screen button was held.");
        }

        [UnityTest]
        public IEnumerator T02_PhysicsContactCollectsOnce_DisablesCrystal_AndUpdatesHudAndScore()
        {
            yield return LoadSceneAndWait("Level1");

            GameController controller = FindInActiveScene<GameController>();
            PlayerController player = FindInActiveScene<PlayerController>();
            Collectible collectible = FindAllInActiveScene<Collectible>()
                .OrderBy(item => item.name)
                .First();

            CheckAll(() =>
            {
                Assert.That(controller.TotalCrystals, Is.EqualTo(3));
                Assert.That(controller.CollectedCrystals, Is.Zero);
                Assert.That(controller.Score, Is.EqualTo(150));
                Assert.That(collectible.gameObject.activeSelf, Is.True);
            });

            yield return TeleportPlayerTo(player, collectible.transform.position);

            Text crystalsText = FindObject("Crystals").GetComponent<Text>();
            Text scoreText = FindObject("Score").GetComponent<Text>();
            CheckAll(() =>
            {
                Assert.That(controller.CollectedCrystals, Is.EqualTo(1));
                Assert.That(controller.Score, Is.EqualTo(250));
                Assert.That(collectible.gameObject.activeSelf, Is.False);
                Assert.That(crystalsText.text, Does.Contain("1/3"));
                Assert.That(scoreText.text, Does.Contain("250"));
            });

            yield return new WaitForFixedUpdate();
            Assert.That(controller.CollectedCrystals, Is.EqualTo(1),
                "The same collectible was counted more than once.");
        }

        [UnityTest]
        public IEnumerator T04_HazardContactRemovesOneLife_AndRespawnsAtStart()
        {
            yield return LoadSceneAndWait("Level1");

            GameController controller = FindInActiveScene<GameController>();
            PlayerController player = FindInActiveScene<PlayerController>();
            Hazard hazard = FindAllInActiveScene<Hazard>().OrderBy(item => item.name).First();
            Vector3 spawn = player.transform.position;

            yield return TeleportPlayerTo(player, hazard.transform.position);

            CheckAll(() =>
            {
                Assert.That(controller.Lives, Is.EqualTo(2));
                Assert.That(controller.State, Is.EqualTo(GameRunState.Playing));
                Assert.That(Vector3.Distance(player.transform.position, spawn), Is.LessThan(0.05f));
                Assert.That(FindObject("Lives").GetComponent<Text>().text, Does.Contain("2"));
            });
        }

        [UnityTest]
        public IEnumerator T05_DefeatAndRestartResetRun_ButPreservePersistentProgress()
        {
            SaveService.RegisterVictory(2, 650);
            SaveService.SetMasterVolume(0.35f);
            yield return LoadSceneAndWait("Level1");

            GameController originalController = FindInActiveScene<GameController>();
            originalController.DamagePlayer();
            yield return new WaitForSecondsRealtime(0.85f);
            originalController.DamagePlayer();
            yield return new WaitForSecondsRealtime(0.85f);
            originalController.DamagePlayer();
            yield return null;

            CanvasGroup defeat = FindGroup("DefeatPanel");
            CheckAll(() =>
            {
                Assert.That(originalController.State, Is.EqualTo(GameRunState.Defeat));
                Assert.That(originalController.Lives, Is.Zero);
                Assert.That(Time.timeScale, Is.EqualTo(0f));
                Assert.That(defeat.alpha, Is.EqualTo(1f).Within(0.001f));
                Assert.That(defeat.interactable, Is.True);
                Assert.That(defeat.blocksRaycasts, Is.True);
            });

            Button restart = FindButton(defeat.transform, "RestartButton");
            Click(restart);
            yield return WaitForScene("Level1");

            GameController restartedController = FindInActiveScene<GameController>();
            CheckAll(() =>
            {
                Assert.That(restartedController, Is.Not.SameAs(originalController));
                Assert.That(restartedController.State, Is.EqualTo(GameRunState.Playing));
                Assert.That(restartedController.Lives, Is.EqualTo(GameRules.StartingLives));
                Assert.That(restartedController.CollectedCrystals, Is.Zero);
                Assert.That(Time.timeScale, Is.EqualTo(1f));
                Assert.That(SaveService.UnlockedLevel, Is.EqualTo(3));
                Assert.That(SaveService.BestScore, Is.EqualTo(650));
                Assert.That(SaveService.MasterVolume, Is.EqualTo(0.35f).Within(0.001f));
            });
        }

        [UnityTest]
        public IEnumerator T06_NaturalTriggerFlowCollectsAll_ExitWins_AndPersistsUnlockAndBestScore()
        {
            yield return LoadSceneAndWait("Level1");
            GameController controller = FindInActiveScene<GameController>();

            yield return CollectAllAndEnterExit(controller);

            CanvasGroup victory = FindGroup("VictoryPanel");
            CheckAll(() =>
            {
                Assert.That(controller.CollectedCrystals, Is.EqualTo(3));
                Assert.That(controller.Lives, Is.EqualTo(3));
                Assert.That(controller.Score, Is.EqualTo(450));
                Assert.That(controller.State, Is.EqualTo(GameRunState.Victory));
                Assert.That(victory.alpha, Is.EqualTo(1f).Within(0.001f));
                Assert.That(SaveService.UnlockedLevel, Is.EqualTo(2));
                Assert.That(SaveService.BestScore, Is.EqualTo(450));
            });
        }

        [UnityTest]
        public IEnumerator T09AndT15_FullThreeLevelFlowUnlocksSequentially_AndFinalLevelReturnsToMenuWithoutLevelFour()
        {
            SaveService.MarkTutorialSeen();
            yield return LoadSceneAndWait("Level1");

            for (int level = 1; level <= GameRules.MaxLevel; level++)
            {
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo($"Level{level}"));
                GameController controller = FindInActiveScene<GameController>();
                Assert.That(controller.LevelIndex, Is.EqualTo(level));

                yield return CollectAllAndEnterExit(controller);
                Assert.That(SaveService.UnlockedLevel, Is.EqualTo(Mathf.Min(level + 1, GameRules.MaxLevel)));

                CanvasGroup victory = FindGroup("VictoryPanel");
                Button next = FindButton(victory.transform, "NextButton");
                if (level < GameRules.MaxLevel)
                {
                    Assert.That(ButtonLabel(next), Does.Contain("СЛЕДУЮЩИЙ УРОВЕНЬ"));
                    Click(next);
                    yield return WaitForScene($"Level{level + 1}");
                }
                else
                {
                    Assert.That(ButtonLabel(next), Does.Contain("ГЛАВНОЕ МЕНЮ"));
                    Assert.That(FindObject("Level4"), Is.Null);
                    Click(next);
                    yield return WaitForScene("MainMenu");
                }
            }

            CheckAll(() =>
            {
                Assert.That(SaveService.UnlockedLevel, Is.EqualTo(3));
                Assert.That(SaveService.BestScore, Is.EqualTo(650));
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("MainMenu"));
            });
        }

        [UnityTest]
        public IEnumerator T11_PauseStopsTimeAndTimerDuringFade_ResumeContinuesBoth()
        {
            yield return LoadSceneAndWait("Level1");

            GameController game = FindInActiveScene<GameController>();
            PauseController pause = FindInActiveScene<PauseController>();
            CanvasGroup pauseGroup = FindGroup("PausePanel");
            Button pauseButton = FindButton("PauseButton");
            AssertHidden(pauseGroup);

            yield return new WaitForSecondsRealtime(0.12f);
            Click(pauseButton);
            float stoppedAt = game.ElapsedTime;

            CheckAll(() =>
            {
                Assert.That(Time.timeScale, Is.EqualTo(0f));
                Assert.That(pause.IsPaused, Is.True);
                Assert.That(pause.IsTransitioning, Is.True);
                Assert.That(pauseButton.interactable, Is.False);
                Assert.That(pauseGroup.interactable, Is.False);
                Assert.That(pauseGroup.blocksRaycasts, Is.True);
            });

            yield return new WaitForSecondsRealtime(0.3f);
            CheckAll(() =>
            {
                Assert.That(game.ElapsedTime, Is.EqualTo(stoppedAt).Within(0.02f));
                Assert.That(pause.IsTransitioning, Is.False);
                Assert.That(pauseGroup.alpha, Is.EqualTo(1f).Within(0.001f));
                Assert.That(pauseGroup.interactable, Is.True);
            });

            Button resume = FindButton(pauseGroup.transform, "ResumeButton");
            Click(resume);
            Assert.That(Time.timeScale, Is.EqualTo(0f), "Time resumed before the fade-out completed.");
            yield return new WaitForSecondsRealtime(0.3f);

            float resumedAt = game.ElapsedTime;
            yield return new WaitForSecondsRealtime(0.15f);
            CheckAll(() =>
            {
                Assert.That(Time.timeScale, Is.EqualTo(1f));
                Assert.That(pause.IsPaused, Is.False);
                Assert.That(pause.IsTransitioning, Is.False);
                Assert.That(game.ElapsedTime, Is.GreaterThan(resumedAt + 0.05f));
            });
            AssertHidden(pauseGroup);
        }

        [UnityTest]
        public IEnumerator T14_FocusAndApplicationPauseCallbacksAutomaticallyPauseActiveGameplay()
        {
            yield return LoadSceneAndWait("Level1");

            PauseController pause = FindInActiveScene<PauseController>();
            pause.gameObject.SendMessage("OnApplicationFocus", false, SendMessageOptions.RequireReceiver);

            CheckAll(() =>
            {
                Assert.That(pause.IsPaused, Is.True);
                Assert.That(Time.timeScale, Is.EqualTo(0f));
                Assert.That(FindGroup("PausePanel").blocksRaycasts, Is.True);
            });

            yield return new WaitForSecondsRealtime(0.3f);
            pause.gameObject.SendMessage("OnApplicationPause", true, SendMessageOptions.RequireReceiver);
            CheckAll(() =>
            {
                Assert.That(pause.IsPaused, Is.True);
                Assert.That(Time.timeScale, Is.EqualTo(0f));
            });
        }
    }
}
