using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace EnergoKey.Tests.PlayMode
{
    public sealed class ResponsiveRenderIntegrationTests : PlayModeTestHarness
    {
        private readonly struct CaptureSize
        {
            public readonly int Width;
            public readonly int Height;

            public CaptureSize(int width, int height)
            {
                Width = width;
                Height = height;
            }

            public string Label => $"{Width}x{Height}";
        }

        private static readonly CaptureSize[] RequiredSizes =
        {
            new CaptureSize(1280, 720),
            new CaptureSize(1440, 720),
            new CaptureSize(1024, 768)
        };

        [UnityTest]
        public IEnumerator SevenRequiredStatesRenderAtThreeTargetResolutions()
        {
            string outputDirectory = Path.GetFullPath(Path.Combine(Application.dataPath, "../EVIDENCE/responsive"));
            Directory.CreateDirectory(outputDirectory);

            SaveService.RegisterVictory(2, 650);
            SaveService.MarkTutorialSeen();

            yield return LoadSceneAndWait("MainMenu");
            yield return CaptureCurrentState("MainMenu", outputDirectory);

            yield return LoadSceneAndWait("LevelSelect");
            yield return CaptureCurrentState("LevelSelect", outputDirectory);

            yield return LoadSceneAndWait("Level1");
            yield return CaptureCurrentState("Gameplay", outputDirectory);

            yield return LoadSceneAndWait("Level1");
            PauseController pause = FindInActiveScene<PauseController>();
            pause.PauseGame();
            yield return new WaitForSecondsRealtime(0.3f);
            Assert.That(pause.IsPaused, Is.True);
            Assert.That(FindGroup("PausePanel").alpha, Is.EqualTo(1f).Within(0.001f));
            yield return CaptureCurrentState("Pause", outputDirectory);

            yield return LoadSceneAndWait("Level1");
            GameController victoryController = FindInActiveScene<GameController>();
            yield return CollectAllAndEnterExit(victoryController);
            Assert.That(victoryController.State, Is.EqualTo(GameRunState.Victory));
            yield return CaptureCurrentState("Victory", outputDirectory);

            yield return LoadSceneAndWait("Level1");
            GameController defeatController = FindInActiveScene<GameController>();
            defeatController.DamagePlayer();
            yield return new WaitForSecondsRealtime(0.85f);
            defeatController.DamagePlayer();
            yield return new WaitForSecondsRealtime(0.85f);
            defeatController.DamagePlayer();
            yield return null;
            Assert.That(defeatController.State, Is.EqualTo(GameRunState.Defeat));
            yield return CaptureCurrentState("Defeat", outputDirectory);

            yield return LoadSceneAndWait("MainMenu");
            Click(FindButton("SettingsButton"));
            CanvasGroup settings = FindGroup("SettingsPanel");
            Assert.That(settings.alpha, Is.EqualTo(1f).Within(0.001f));
            yield return CaptureCurrentState("Settings", outputDirectory);

            string[] images = Directory.GetFiles(outputDirectory, "RESP_*.png");
            Assert.That(images.Length, Is.EqualTo(21),
                $"Expected exactly 21 responsive captures, but found {images.Length} in '{outputDirectory}'.");
        }

        [UnityTest]
        public IEnumerator SavedProgressAndVolumeEvidenceAreIndependentRuntimeRenders()
        {
            string screenshotDirectory = Path.GetFullPath(Path.Combine(Application.dataPath, "../EVIDENCE/screenshots"));
            string savedProgressPath = Path.Combine(screenshotDirectory, "PP12_08_SavedProgressAfterRestart.png");
            string savedVolumePath = Path.Combine(screenshotDirectory, "PP12_09_SavedVolume.png");
            string levelSelectComparison = Path.Combine(screenshotDirectory, "PP12_04_MainMenu_LevelSelect.png");
            string settingsComparison = Path.Combine(screenshotDirectory, "PP12_12_Settings.png");
            CaptureSize evidenceSize = new CaptureSize(1366, 768);

            Assert.That(Directory.Exists(screenshotDirectory), Is.True);
            Assert.That(File.Exists(levelSelectComparison), Is.True);
            Assert.That(File.Exists(settingsComparison), Is.True);

            SaveService.RegisterVictory(2, 650);
            SaveService.SetMasterVolume(0.37f);
            SaveService.MarkTutorialSeen();

            yield return LoadSceneAndWait("MainMenu");
            Text progress = FindObject("ProgressSummary").GetComponent<Text>();
            Assert.That(progress.text, Does.Contain("3/3"));
            Assert.That(progress.text, Does.Contain("650"));
            yield return RenderCurrentFrame(savedProgressPath, "SavedProgress", evidenceSize);

            Click(FindButton("SettingsButton"));
            CanvasGroup settings = FindGroup("SettingsPanel");
            Text volume = FindObject(settings.transform, "VolumeValue").GetComponent<Text>();
            Assert.That(settings.alpha, Is.EqualTo(1f).Within(0.001f));
            Assert.That(volume.text, Is.EqualTo("37%"));
            Assert.That(progress.text, Does.Contain("3/3"));
            Assert.That(progress.text, Does.Contain("650"));
            yield return RenderCurrentFrame(savedVolumePath, "SavedVolume", evidenceSize);

            Assert.That(File.Exists(savedProgressPath), Is.True);
            Assert.That(File.Exists(savedVolumePath), Is.True);
            AssertFilesHaveDifferentHashes(savedProgressPath, levelSelectComparison);
            AssertFilesHaveDifferentHashes(savedVolumePath, settingsComparison);
            AssertFilesHaveDifferentHashes(savedProgressPath, savedVolumePath);
        }

        private static IEnumerator CaptureCurrentState(string stateName, string outputDirectory)
        {
            SafeAreaAdapter safeAreaAdapter = FindInActiveScene<SafeAreaAdapter>();
            Assert.That(safeAreaAdapter, Is.Not.Null, $"State '{stateName}' has no SafeAreaAdapter.");
            safeAreaAdapter.SetEditorSimulation(true);
            yield return null;
            Canvas.ForceUpdateCanvases();

            Assert.That(safeAreaAdapter.IsSimulating, Is.True,
                $"State '{stateName}' did not enable the Editor Safe Area simulation.");
            Assert.That(safeAreaAdapter.SafeAreaRoot.anchorMin.x, Is.EqualTo(0.06f).Within(0.001f));
            Assert.That(safeAreaAdapter.SafeAreaRoot.anchorMin.y, Is.EqualTo(0.06f).Within(0.001f));
            Assert.That(safeAreaAdapter.SafeAreaRoot.anchorMax.x, Is.EqualTo(0.94f).Within(0.001f));
            Assert.That(safeAreaAdapter.SafeAreaRoot.anchorMax.y, Is.EqualTo(0.94f).Within(0.001f));

            foreach (CaptureSize size in RequiredSizes)
            {
                string fileName = $"RESP_{size.Label}_{stateName}.png";
                string outputPath = Path.Combine(outputDirectory, fileName);
                yield return RenderCurrentFrame(outputPath, stateName, size);
            }

            safeAreaAdapter.SetEditorSimulation(false);
        }

        private static IEnumerator RenderCurrentFrame(string outputPath, string stateName, CaptureSize size)
        {
            Canvas canvas = FindInActiveScene<Canvas>("AppCanvas");
            Camera camera = Camera.main;
            Assert.That(canvas, Is.Not.Null, $"State '{stateName}' has no AppCanvas.");
            Assert.That(camera, Is.Not.Null, $"State '{stateName}' has no main camera.");

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            Assert.That(scaler, Is.Not.Null, "AppCanvas has no CanvasScaler.");

            RenderMode previousRenderMode = canvas.renderMode;
            Camera previousWorldCamera = canvas.worldCamera;
            float previousPlaneDistance = canvas.planeDistance;
            float previousScaleFactor = canvas.scaleFactor;
            bool previousScalerEnabled = scaler.enabled;
            RenderTexture previousTarget = camera.targetTexture;
            float previousAspect = camera.aspect;
            float previousOrthographicSize = camera.orthographicSize;
            RenderTexture previousActive = RenderTexture.active;

            RenderTexture target = new RenderTexture(size.Width, size.Height, 24, RenderTextureFormat.ARGB32)
            {
                name = $"EnergoKey_{stateName}_{size.Label}",
                antiAliasing = 1,
                filterMode = FilterMode.Bilinear
            };
            target.Create();

            Texture2D screenshot = null;
            try
            {
                camera.targetTexture = target;
                camera.aspect = (float)size.Width / size.Height;
                camera.orthographicSize = Mathf.Max(4.5f, 8f / camera.aspect);

                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.planeDistance = 1f;

                // CanvasScaler uses Screen.width in the Editor, not the camera's
                // RenderTexture. Apply the exact equivalent ScaleWithScreenSize
                // factor for the target pixels before rendering offscreen.
                scaler.enabled = false;
                canvas.scaleFactor = CalculateScaleFactor(scaler, size.Width, size.Height);

                yield return null;
                Canvas.ForceUpdateCanvases();
                yield return null;
                Canvas.ForceUpdateCanvases();

                camera.aspect = (float)size.Width / size.Height;
                camera.orthographicSize = Mathf.Max(4.5f, 8f / camera.aspect);
                ValidateVisibleControlsInsideSafeArea(canvas, camera, stateName, size);

                camera.Render();
                RenderTexture.active = target;
                screenshot = new Texture2D(size.Width, size.Height, TextureFormat.RGBA32, false, false);
                screenshot.ReadPixels(new Rect(0, 0, size.Width, size.Height), 0, 0, false);
                screenshot.Apply(false, false);

                Assert.That(screenshot.width, Is.EqualTo(size.Width));
                Assert.That(screenshot.height, Is.EqualTo(size.Height));
                AssertFrameHasVisibleContent(screenshot, stateName, size);

                byte[] png = screenshot.EncodeToPNG();
                Assert.That(png, Is.Not.Null.And.Not.Empty);
                File.WriteAllBytes(outputPath, png);
                Assert.That(File.Exists(outputPath), Is.True);
                Assert.That(new FileInfo(outputPath).Length, Is.GreaterThan(10_000),
                    $"Rendered PNG '{outputPath}' is unexpectedly small.");
            }
            finally
            {
                RenderTexture.active = previousActive;
                camera.targetTexture = previousTarget;
                camera.aspect = previousAspect;
                camera.orthographicSize = previousOrthographicSize;
                canvas.renderMode = previousRenderMode;
                canvas.worldCamera = previousWorldCamera;
                canvas.planeDistance = previousPlaneDistance;
                canvas.scaleFactor = previousScaleFactor;
                scaler.enabled = previousScalerEnabled;

                if (screenshot != null)
                {
                    Object.Destroy(screenshot);
                }

                target.Release();
                Object.Destroy(target);
            }

            yield return null;
        }

        private static float CalculateScaleFactor(CanvasScaler scaler, int width, int height)
        {
            Vector2 reference = scaler.referenceResolution;
            float logWidth = Mathf.Log(width / reference.x, 2f);
            float logHeight = Mathf.Log(height / reference.y, 2f);
            float logWeightedAverage = Mathf.Lerp(logWidth, logHeight, scaler.matchWidthOrHeight);
            return Mathf.Pow(2f, logWeightedAverage);
        }

        private static void ValidateVisibleControlsInsideSafeArea(
            Canvas canvas,
            Camera camera,
            string stateName,
            CaptureSize size)
        {
            GameObject safeAreaObject = FindObject("SafeArea");
            Assert.That(safeAreaObject, Is.Not.Null);
            RectTransform safeArea = safeAreaObject.GetComponent<RectTransform>();
            Assert.That(safeArea, Is.Not.Null);

            Rect safePixels = ScreenRect(camera, safeArea);
            IReadOnlyList<Selectable> controls = FindAllInActiveScene<Selectable>();
            int visibleControlCount = 0;
            foreach (Selectable control in controls)
            {
                if (!IsVisiblyRendered(control.transform))
                {
                    continue;
                }

                RectTransform rect = control.transform as RectTransform;
                if (rect == null || rect.rect.width <= 0f || rect.rect.height <= 0f)
                {
                    continue;
                }

                visibleControlCount++;
                Rect pixels = ScreenRect(camera, rect);
                const float tolerance = 2f;
                Assert.That(pixels.xMin, Is.GreaterThanOrEqualTo(safePixels.xMin - tolerance),
                    $"{stateName} {size.Label}: '{control.name}' extends past the safe area's left edge.");
                Assert.That(pixels.yMin, Is.GreaterThanOrEqualTo(safePixels.yMin - tolerance),
                    $"{stateName} {size.Label}: '{control.name}' extends past the safe area's bottom edge.");
                Assert.That(pixels.xMax, Is.LessThanOrEqualTo(safePixels.xMax + tolerance),
                    $"{stateName} {size.Label}: '{control.name}' extends past the safe area's right edge.");
                Assert.That(pixels.yMax, Is.LessThanOrEqualTo(safePixels.yMax + tolerance),
                    $"{stateName} {size.Label}: '{control.name}' extends past the safe area's top edge.");
            }

            Assert.That(visibleControlCount, Is.GreaterThan(0),
                $"State '{stateName}' rendered no visible UI controls at {size.Label}.");

            if (stateName == "LevelSelect")
            {
                Rect backButton = ScreenRect(camera, FindObject("BackButton").GetComponent<RectTransform>());
                Rect unlockHint = ScreenRect(camera, FindObject("UnlockHint").GetComponent<RectTransform>());
                Assert.That(backButton.Overlaps(unlockHint), Is.False,
                    $"LevelSelect {size.Label}: the unlock hint overlaps the Back button.");
            }
        }

        private static bool IsVisiblyRendered(Transform transform)
        {
            if (transform == null || !transform.gameObject.activeInHierarchy)
            {
                return false;
            }

            for (Transform current = transform; current != null; current = current.parent)
            {
                CanvasGroup group = current.GetComponent<CanvasGroup>();
                if (group != null && group.alpha <= 0.01f)
                {
                    return false;
                }
            }

            Graphic graphic = transform.GetComponent<Graphic>();
            return graphic == null || (graphic.enabled && graphic.color.a > 0.01f);
        }

        private static Rect ScreenRect(Camera camera, RectTransform transform)
        {
            Vector3[] corners = new Vector3[4];
            transform.GetWorldCorners(corners);
            Vector2 bottomLeft = camera.WorldToScreenPoint(corners[0]);
            Vector2 topRight = camera.WorldToScreenPoint(corners[2]);
            return Rect.MinMaxRect(
                Mathf.Min(bottomLeft.x, topRight.x),
                Mathf.Min(bottomLeft.y, topRight.y),
                Mathf.Max(bottomLeft.x, topRight.x),
                Mathf.Max(bottomLeft.y, topRight.y));
        }

        private static void AssertFrameHasVisibleContent(Texture2D screenshot, string stateName, CaptureSize size)
        {
            Color32[] pixels = screenshot.GetPixels32();
            HashSet<uint> sampledColors = new HashSet<uint>();
            int stride = Mathf.Max(1, pixels.Length / 4096);
            for (int index = 0; index < pixels.Length; index += stride)
            {
                Color32 color = pixels[index];
                uint packed = ((uint)color.r << 24) | ((uint)color.g << 16) | ((uint)color.b << 8) | color.a;
                sampledColors.Add(packed);
            }

            Assert.That(sampledColors.Count, Is.GreaterThan(8),
                $"State '{stateName}' at {size.Label} produced an empty or nearly uniform frame.");
        }

        private static void AssertFilesHaveDifferentHashes(string firstPath, string secondPath)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] firstHash = sha256.ComputeHash(File.ReadAllBytes(firstPath));
                byte[] secondHash = sha256.ComputeHash(File.ReadAllBytes(secondPath));
                Assert.That(firstHash.SequenceEqual(secondHash), Is.False,
                    $"Evidence files must be independent renders, but their SHA-256 hashes match: '{firstPath}' and '{secondPath}'.");
            }
        }
    }
}
