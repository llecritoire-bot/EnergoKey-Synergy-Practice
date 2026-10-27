using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace EnergoKey.Tests.PlayMode
{
    public abstract class PlayModeTestHarness
    {
        private IDisposable temporarySaveKeys;
        private float previousAudioVolume;

        [SetUp]
        public void SetUpIsolatedRun()
        {
            Time.timeScale = 1f;
            previousAudioVolume = AudioListener.volume;
            temporarySaveKeys = SaveService.UseTemporaryKeysForTests(Guid.NewGuid().ToString("N"));
        }

        [UnityTearDown]
        public IEnumerator TearDownIsolatedRun()
        {
            Time.timeScale = 1f;
            yield return null;

            temporarySaveKeys?.Dispose();
            temporarySaveKeys = null;
            AudioListener.volume = previousAudioVolume;
        }

        protected static IEnumerator LoadSceneAndWait(string sceneName)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);

            for (int frame = 0; frame < 4; frame++)
            {
                yield return null;
            }

            Canvas.ForceUpdateCanvases();
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(sceneName),
                $"Expected scene '{sceneName}' to be active.");
            Assert.That(FindInActiveScene<SceneBootstrap>(), Is.Not.Null,
                $"Scene '{sceneName}' did not create its runtime bootstrap.");
        }

        protected static IEnumerator WaitForScene(string sceneName, int maximumFrames = 20)
        {
            for (int frame = 0; frame < maximumFrames; frame++)
            {
                if (SceneManager.GetActiveScene().name == sceneName)
                {
                    for (int settle = 0; settle < 3; settle++)
                    {
                        yield return null;
                    }

                    Canvas.ForceUpdateCanvases();
                    yield break;
                }

                yield return null;
            }

            Assert.Fail($"Scene '{sceneName}' was not reached within {maximumFrames} frames; current scene is '{SceneManager.GetActiveScene().name}'.");
        }

        protected static T FindInActiveScene<T>(string objectName = null) where T : Component
        {
            Scene activeScene = SceneManager.GetActiveScene();
            return Resources.FindObjectsOfTypeAll<T>()
                .FirstOrDefault(component =>
                    component != null &&
                    component.gameObject.scene == activeScene &&
                    (objectName == null || component.gameObject.name == objectName));
        }

        protected static IReadOnlyList<T> FindAllInActiveScene<T>() where T : Component
        {
            Scene activeScene = SceneManager.GetActiveScene();
            return Resources.FindObjectsOfTypeAll<T>()
                .Where(component => component != null && component.gameObject.scene == activeScene)
                .ToArray();
        }

        protected static GameObject FindObject(string objectName)
        {
            Scene activeScene = SceneManager.GetActiveScene();
            foreach (GameObject root in activeScene.GetRootGameObjects())
            {
                foreach (Transform candidate in root.GetComponentsInChildren<Transform>(true))
                {
                    if (candidate.name == objectName)
                    {
                        return candidate.gameObject;
                    }
                }
            }

            return null;
        }

        protected static GameObject FindObject(Transform parent, string objectName)
        {
            if (parent == null)
            {
                return null;
            }

            return parent.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(candidate => candidate.name == objectName)
                ?.gameObject;
        }

        protected static Button FindButton(string objectName)
        {
            GameObject buttonObject = FindObject(objectName);
            Assert.That(buttonObject, Is.Not.Null, $"Button object '{objectName}' was not found.");
            Button button = buttonObject.GetComponent<Button>();
            Assert.That(button, Is.Not.Null, $"Object '{objectName}' has no Button component.");
            return button;
        }

        protected static Button FindButton(Transform parent, string objectName)
        {
            GameObject buttonObject = FindObject(parent, objectName);
            Assert.That(buttonObject, Is.Not.Null, $"Button object '{objectName}' was not found below '{parent?.name}'.");
            Button button = buttonObject.GetComponent<Button>();
            Assert.That(button, Is.Not.Null, $"Object '{objectName}' has no Button component.");
            return button;
        }

        protected static CanvasGroup FindGroup(string objectName)
        {
            GameObject panel = FindObject(objectName);
            Assert.That(panel, Is.Not.Null, $"Panel '{objectName}' was not found.");
            CanvasGroup group = panel.GetComponent<CanvasGroup>();
            Assert.That(group, Is.Not.Null, $"Panel '{objectName}' has no CanvasGroup.");
            return group;
        }

        protected static void AssertHidden(CanvasGroup group, string message = null)
        {
            Assert.That(group, Is.Not.Null);
            CheckAll(() =>
            {
                Assert.That(group.alpha, Is.EqualTo(0f).Within(0.001f), message ?? group.name);
                Assert.That(group.interactable, Is.False, message ?? group.name);
                Assert.That(group.blocksRaycasts, Is.False, message ?? group.name);
            });
        }

        protected static PointerEventData CreatePointerData(RectTransform target)
        {
            Assert.That(EventSystem.current, Is.Not.Null, "The runtime UI did not create an EventSystem.");
            Canvas canvas = target.GetComponentInParent<Canvas>();
            Assert.That(canvas, Is.Not.Null, $"'{target.name}' is not inside a Canvas.");
            Camera eventCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            Vector3 worldCenter = target.TransformPoint(target.rect.center);
            return new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left,
                position = RectTransformUtility.WorldToScreenPoint(eventCamera, worldCenter)
            };
        }

        protected static IReadOnlyList<RaycastResult> Raycast(RectTransform target)
        {
            Canvas.ForceUpdateCanvases();
            Canvas canvas = target.GetComponentInParent<Canvas>();
            Assert.That(canvas, Is.Not.Null);
            GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
            Assert.That(raycaster, Is.Not.Null, $"Canvas '{canvas.name}' has no GraphicRaycaster.");

            List<RaycastResult> results = new List<RaycastResult>();
            raycaster.Raycast(CreatePointerData(target), results);
            return results;
        }

        protected static void AssertPointerReachable(Selectable selectable)
        {
            Assert.That(selectable, Is.Not.Null);
            Assert.That(selectable.gameObject.activeInHierarchy, Is.True, $"'{selectable.name}' is inactive.");
            Assert.That(selectable.IsInteractable(), Is.True, $"'{selectable.name}' is not interactable.");

            IReadOnlyList<RaycastResult> hits = Raycast(selectable.transform as RectTransform);
            bool hitSelectable = hits.Any(hit =>
                hit.gameObject == selectable.gameObject || hit.gameObject.transform.IsChildOf(selectable.transform));
            Assert.That(hitSelectable, Is.True,
                $"A pointer raycast at '{selectable.name}' did not reach the button. Hits: {string.Join(", ", hits.Select(hit => hit.gameObject.name))}");
        }

        protected static void Click(Button button)
        {
            AssertPointerReachable(button);
            PointerEventData eventData = CreatePointerData(button.transform as RectTransform);
            ExecuteEvents.Execute(button.gameObject, eventData, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(button.gameObject, eventData, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(button.gameObject, eventData, ExecuteEvents.pointerClickHandler);
        }

        protected static void PointerDown(MobileInputButton input)
        {
            Button button = input.GetComponent<Button>();
            AssertPointerReachable(button);
            PointerEventData eventData = CreatePointerData(input.transform as RectTransform);
            ExecuteEvents.Execute(input.gameObject, eventData, ExecuteEvents.pointerDownHandler);
        }

        protected static void PointerUp(MobileInputButton input)
        {
            PointerEventData eventData = CreatePointerData(input.transform as RectTransform);
            ExecuteEvents.Execute(input.gameObject, eventData, ExecuteEvents.pointerUpHandler);
        }

        protected static IEnumerator TeleportPlayerTo(PlayerController player, Vector3 target)
        {
            Rigidbody2D body = player.GetComponent<Rigidbody2D>();
            Assert.That(body, Is.Not.Null);
            body.linearVelocity = Vector2.zero;
            body.position = target;
            Physics2D.SyncTransforms();
            yield return new WaitForFixedUpdate();
            yield return null;
        }

        protected static IEnumerator CollectAllAndEnterExit(GameController controller)
        {
            PlayerController player = FindInActiveScene<PlayerController>();
            Assert.That(player, Is.Not.Null);

            Collectible[] collectibles = FindAllInActiveScene<Collectible>()
                .Where(item => item.gameObject.activeInHierarchy)
                .OrderBy(item => item.name)
                .ToArray();

            int expectedCollected = controller.CollectedCrystals;
            foreach (Collectible collectible in collectibles)
            {
                yield return TeleportPlayerTo(player, collectible.transform.position);
                expectedCollected++;
                Assert.That(controller.CollectedCrystals, Is.EqualTo(expectedCollected),
                    $"Physics contact with '{collectible.name}' did not collect exactly one crystal.");
                Assert.That(collectible.gameObject.activeSelf, Is.False,
                    $"Collected object '{collectible.name}' remained active.");
            }

            ExitGate exit = FindInActiveScene<ExitGate>();
            Assert.That(exit, Is.Not.Null);
            yield return TeleportPlayerTo(player, exit.transform.position);
            Assert.That(controller.State, Is.EqualTo(GameRunState.Victory));
        }

        protected static string ButtonLabel(Button button)
        {
            Text label = button.GetComponentInChildren<Text>(true);
            Assert.That(label, Is.Not.Null, $"Button '{button.name}' has no text label.");
            return label.text;
        }

        protected static bool IsDescendantOrSelf(Transform candidate, Transform possibleParent)
        {
            return candidate == possibleParent || candidate.IsChildOf(possibleParent);
        }

        // Unity 6000.6.2f1 bundles an NUnit API without Assert.Multiple.
        // This compatibility helper keeps related assertions visually grouped.
        protected static void CheckAll(Action assertions)
        {
            assertions();
        }
    }
}
