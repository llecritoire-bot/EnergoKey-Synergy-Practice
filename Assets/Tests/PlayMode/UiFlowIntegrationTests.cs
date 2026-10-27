using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace EnergoKey.Tests.PlayMode
{
    public sealed class UiFlowIntegrationTests : PlayModeTestHarness
    {
        [UnityTest]
        public IEnumerator T03_MainMenuLevelSelectAndGameplayControlsAcceptPointerInput()
        {
            SaveService.RegisterVictory(2, 650);
            SaveService.MarkTutorialSeen();
            yield return LoadSceneAndWait("MainMenu");

            Button settingsButton = FindButton("SettingsButton");
            Click(settingsButton);
            CanvasGroup settings = FindGroup("SettingsPanel");
            CheckAll(() =>
            {
                Assert.That(settings.alpha, Is.EqualTo(1f).Within(0.001f));
                Assert.That(settings.interactable, Is.True);
                Assert.That(settings.blocksRaycasts, Is.True);
            });
            Click(FindButton(settings.transform, "CloseButton"));
            AssertHidden(settings);

            Button aboutButton = FindButton("AboutButton");
            Click(aboutButton);
            CanvasGroup about = FindGroup("AboutPanel");
            Assert.That(about.alpha, Is.EqualTo(1f).Within(0.001f));
            Click(FindButton(about.transform, "CloseButton"));
            AssertHidden(about);

            Button tutorialButton = FindButton("TutorialButton");
            Click(tutorialButton);
            TutorialController tutorial = FindInActiveScene<TutorialController>();
            CheckAll(() =>
            {
                Assert.That(tutorial.IsOpen, Is.True);
                Assert.That(tutorial.CurrentCardIndex, Is.Zero);
            });
            Click(FindButton(FindGroup("TutorialPanel").transform, "SkipButton"));
            Assert.That(tutorial.IsOpen, Is.False);

            Click(FindButton("PlayButton"));
            yield return WaitForScene("LevelSelect");

            for (int level = 1; level <= GameRules.MaxLevel; level++)
            {
                GameObject card = FindObject($"Level{level}Card");
                Assert.That(card, Is.Not.Null);
                AssertPointerReachable(FindButton(card.transform, "StartButton"));
            }

            Click(FindButton(FindObject("Level1Card").transform, "StartButton"));
            yield return WaitForScene("Level1");

            foreach (string directionButton in new[] { "LeftButton", "RightButton", "UpButton", "DownButton" })
            {
                AssertPointerReachable(FindButton(directionButton));
            }

            AssertPointerReachable(FindButton("PauseButton"));
        }

        [UnityTest]
        public IEnumerator T04_HiddenPanelsDoNotAppearInRaycasts_OrBlockUnderlyingControls()
        {
            SaveService.MarkTutorialSeen();
            yield return LoadSceneAndWait("MainMenu");

            CanvasGroup[] menuGroups =
            {
                FindGroup("SettingsPanel"),
                FindGroup("TutorialPanel"),
                FindGroup("AboutPanel")
            };
            foreach (CanvasGroup group in menuGroups)
            {
                AssertHidden(group);
            }

            Button play = FindButton("PlayButton");
            var menuHits = Raycast(play.transform as RectTransform);
            foreach (CanvasGroup group in menuGroups)
            {
                Assert.That(menuHits.Any(hit => IsDescendantOrSelf(hit.gameObject.transform, group.transform)), Is.False,
                    $"Hidden panel '{group.name}' intercepted the Play button raycast.");
            }

            Click(play);
            yield return WaitForScene("LevelSelect");
            Click(FindButton(FindObject("Level1Card").transform, "StartButton"));
            yield return WaitForScene("Level1");

            CanvasGroup[] gameplayGroups =
            {
                FindGroup("PausePanel"),
                FindGroup("VictoryPanel"),
                FindGroup("DefeatPanel")
            };
            foreach (CanvasGroup group in gameplayGroups)
            {
                AssertHidden(group);
            }

            Button left = FindButton("LeftButton");
            var gameplayHits = Raycast(left.transform as RectTransform);
            foreach (CanvasGroup group in gameplayGroups)
            {
                Assert.That(gameplayHits.Any(hit => IsDescendantOrSelf(hit.gameObject.transform, group.transform)), Is.False,
                    $"Hidden panel '{group.name}' intercepted the movement-button raycast.");
            }

            PlayerController player = FindInActiveScene<PlayerController>();
            player.SetSpawn(Vector3.zero);
            yield return new WaitForFixedUpdate();
            Vector2 before = player.transform.position;
            MobileInputButton input = left.GetComponent<MobileInputButton>();
            PointerDown(input);
            yield return new WaitForSecondsRealtime(0.14f);
            PointerUp(input);
            yield return new WaitForFixedUpdate();
            Assert.That(player.transform.position.x, Is.LessThan(before.x - 0.2f));
        }

        [UnityTest]
        public IEnumerator T05_CleanSaveAutomaticallyOpensTutorialAtCardOne()
        {
            Assert.That(SaveService.TutorialSeen, Is.False);
            yield return LoadSceneAndWait("MainMenu");

            TutorialController tutorial = FindInActiveScene<TutorialController>();
            CanvasGroup group = FindGroup("TutorialPanel");
            Text title = FindObject(group.transform, "Title").GetComponent<Text>();
            Text progress = FindObject(group.transform, "Progress").GetComponent<Text>();

            CheckAll(() =>
            {
                Assert.That(tutorial.IsOpen, Is.True);
                Assert.That(tutorial.CurrentCardIndex, Is.Zero);
                Assert.That(group.alpha, Is.EqualTo(1f).Within(0.001f));
                Assert.That(group.interactable, Is.True);
                Assert.That(group.blocksRaycasts, Is.True);
                Assert.That(title.text, Is.EqualTo("Управление"));
                Assert.That(progress.text, Is.EqualTo("1 / 3"));
            });
        }

        [UnityTest]
        public IEnumerator T06_NextButtonTraversesThreeTutorialCards_ThenReturnsToMenu()
        {
            SaveService.RegisterVictory(2, 650);
            SaveService.SetMasterVolume(0.4f);
            yield return LoadSceneAndWait("MainMenu");

            TutorialController tutorial = FindInActiveScene<TutorialController>();
            CanvasGroup group = FindGroup("TutorialPanel");
            Text title = FindObject(group.transform, "Title").GetComponent<Text>();
            Text progress = FindObject(group.transform, "Progress").GetComponent<Text>();
            Button next = FindButton(group.transform, "NextButton");

            Assert.That(title.text, Is.EqualTo("Управление"));
            Click(next);
            CheckAll(() =>
            {
                Assert.That(tutorial.CurrentCardIndex, Is.EqualTo(1));
                Assert.That(title.text, Is.EqualTo("Особое правило"));
                Assert.That(progress.text, Is.EqualTo("2 / 3"));
            });

            Click(next);
            CheckAll(() =>
            {
                Assert.That(tutorial.CurrentCardIndex, Is.EqualTo(2));
                Assert.That(title.text, Is.EqualTo("Завершение и ошибка"));
                Assert.That(progress.text, Is.EqualTo("3 / 3"));
                Assert.That(ButtonLabel(next), Is.EqualTo("ГОТОВО"));
            });

            Click(next);
            CheckAll(() =>
            {
                Assert.That(tutorial.IsOpen, Is.False);
                Assert.That(FindObject("MainMenuScreen").activeInHierarchy, Is.True);
                Assert.That(SaveService.TutorialSeen, Is.True);
                Assert.That(SaveService.UnlockedLevel, Is.EqualTo(3));
                Assert.That(SaveService.BestScore, Is.EqualTo(650));
                Assert.That(SaveService.MasterVolume, Is.EqualTo(0.4f).Within(0.001f));
            });
            AssertHidden(group);
        }

        [UnityTest]
        public IEnumerator T07_SkipPersistsTutorialFlag_PreservesProgress_AndPreventsNextAutomaticDisplay()
        {
            SaveService.RegisterVictory(2, 700);
            SaveService.SetMasterVolume(0.25f);
            yield return LoadSceneAndWait("MainMenu");

            TutorialController tutorial = FindInActiveScene<TutorialController>();
            Assert.That(tutorial.IsOpen, Is.True);
            Click(FindButton(FindGroup("TutorialPanel").transform, "SkipButton"));

            CheckAll(() =>
            {
                Assert.That(tutorial.IsOpen, Is.False);
                Assert.That(SaveService.TutorialSeen, Is.True);
                Assert.That(SaveService.UnlockedLevel, Is.EqualTo(3));
                Assert.That(SaveService.BestScore, Is.EqualTo(700));
                Assert.That(SaveService.MasterVolume, Is.EqualTo(0.25f).Within(0.001f));
            });

            yield return LoadSceneAndWait("MainMenu");
            TutorialController afterReload = FindInActiveScene<TutorialController>();
            CheckAll(() =>
            {
                Assert.That(afterReload.IsOpen, Is.False);
                Assert.That(SaveService.UnlockedLevel, Is.EqualTo(3));
                Assert.That(SaveService.BestScore, Is.EqualTo(700));
            });
            AssertHidden(FindGroup("TutorialPanel"));
        }

        [UnityTest]
        public IEnumerator T08_HowToPlayReopensTutorialAtCardOne_AfterItWasPreviouslySeen()
        {
            SaveService.MarkTutorialSeen();
            yield return LoadSceneAndWait("MainMenu");

            TutorialController tutorial = FindInActiveScene<TutorialController>();
            Assert.That(tutorial.IsOpen, Is.False);
            Click(FindButton("TutorialButton"));

            CanvasGroup group = FindGroup("TutorialPanel");
            CheckAll(() =>
            {
                Assert.That(tutorial.IsOpen, Is.True);
                Assert.That(tutorial.CurrentCardIndex, Is.Zero);
                Assert.That(FindObject(group.transform, "Title").GetComponent<Text>().text, Is.EqualTo("Управление"));
                Assert.That(FindObject(group.transform, "Progress").GetComponent<Text>().text, Is.EqualTo("1 / 3"));
            });
        }

        [UnityTest]
        public IEnumerator T09AndT10_PauseIsAlreadyStoppedDuringFade_AndRapidRepeatedCommandsAreIgnored()
        {
            yield return LoadSceneAndWait("Level1");

            PauseController pause = FindInActiveScene<PauseController>();
            CanvasGroup group = FindGroup("PausePanel");
            Button pauseButton = FindButton("PauseButton");
            Click(pauseButton);

            float alphaDuringFade = group.alpha;
            pause.TogglePause();
            pause.PauseGame();
            pause.ResumeGame();

            CheckAll(() =>
            {
                Assert.That(Time.timeScale, Is.EqualTo(0f));
                Assert.That(pause.IsPaused, Is.True);
                Assert.That(pause.IsTransitioning, Is.True);
                Assert.That(group.alpha, Is.EqualTo(alphaDuringFade).Within(0.001f));
                Assert.That(group.interactable, Is.False);
                Assert.That(group.blocksRaycasts, Is.True);
            });

            yield return new WaitForSecondsRealtime(0.3f);
            Assert.That(pause.IsTransitioning, Is.False);
            Button resume = FindButton(group.transform, "ResumeButton");
            Click(resume);
            pause.TogglePause();
            pause.ResumeGame();
            pause.PauseGame();

            CheckAll(() =>
            {
                Assert.That(Time.timeScale, Is.EqualTo(0f));
                Assert.That(pause.IsPaused, Is.True);
                Assert.That(pause.IsTransitioning, Is.True);
            });

            yield return new WaitForSecondsRealtime(0.3f);
            CheckAll(() =>
            {
                Assert.That(Time.timeScale, Is.EqualTo(1f));
                Assert.That(pause.IsPaused, Is.False);
                Assert.That(pause.IsTransitioning, Is.False);
            });
            AssertHidden(group);
        }
    }
}
