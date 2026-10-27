# Манифест файлов «Энергоключ»

Снимок состава: 27 октября 2026 года. Окончательная ревизия поставки идентифицируется аннотированным тегом `pp-pm13-release`; отдельная fresh-clone проверка перечислена как `NOT VERIFIED`.

## Основной состав проекта

| Раздел | Фактический состав | Проверка |
|---|---:|---|
| Unity-сцены `Assets/Scenes/*.unity` | 6 | MainMenu, LevelSelect, Level1, Level2, Level3, Prototype |
| Prefab `Assets/Resources/Prefabs/*.prefab` | 4 | Player, Collectible, Hazard, UIButton |
| C#-файлы в `Assets` | 24 | Файловый подсчёт |
| `.meta` в `Assets` | 86 | Файловый подсчёт |
| Финальные PNG в `Assets/Resources/Art/Final` | 16 | Unity import validation: Sprite, asset loads |
| WAV в `Assets/Resources/Audio` | 6 | mono, 44.1 kHz, ненулевая длина |
| Скриншоты `EVIDENCE/screenshots` | 57 PNG | 54 обязательных + 3 дополнительные карточки обучения |
| PNG в `Release` | 9 | icon, promo, mockup и 6 игровых кадров |
| EditMode tests | 24/24 Passed | `EditModeTests-final.xml` |
| Unity structural checks | 165/165 Passed | `PROJECT_VALIDATION.md` |

Структурная проверка Unity подтверждает наличие всех сцен и `.meta`, первый Build Settings scene `MainMenu`, четыре prefab, импорт графики/звука, размеры release-материалов и документы. Дополнительно подтверждены пакет `com.unity.2d.sprite` и режим `m_DefaultBehaviorMode: 1`. Итог: `165 PASS, 0 FAIL`.

## Обязательные корневые элементы

| Путь | Назначение | Состояние |
|---|---|---|
| `Assets/` | Сцены, код, ресурсы, prefab и `.meta` | Есть |
| `Packages/manifest.json` | Зависимости Unity | Есть |
| `Packages/packages-lock.json` | Зафиксированные версии пакетов | Есть |
| `ProjectSettings/` | Настройки проекта и Player Settings | Есть |
| `.gitignore` | Исключения Unity | Есть |
| `README.md` | Инструкция проекта | Есть |
| `Release/` | Материалы представления продукта | Есть |
| `EVIDENCE/` | Скриншоты, тесты, протоколы и логи | Есть |

## Матрица скриншотов

Обязательная матрица из `03_SCREENSHOT_MATRIX.md` полна: 54 из 54 имён присутствуют.

### УП.ПМ.12 — 12

`UP12_01_ProjectFolders.png`, `UP12_02_SceneOverview.png`, `UP12_03_GameRunning.png`, `UP12_04_PlayerInspector.png`, `UP12_05_CollectibleInspector.png`, `UP12_06_HazardInspector.png`, `UP12_07_InitialState.png`, `UP12_08_Gameplay.png`, `UP12_09_SpecialRule.png`, `UP12_10_Victory.png`, `UP12_11_Defeat.png`, `UP12_12_Restart.png`.

### ПП.ПМ.12 — 12

`PP12_01_Level1.png`, `PP12_02_Level2.png`, `PP12_03_Level3.png`, `PP12_04_MainMenu_LevelSelect.png`, `PP12_05_Pause.png`, `PP12_06_Victory.png`, `PP12_07_Defeat.png`, `PP12_08_SavedProgressAfterRestart.png`, `PP12_09_SavedVolume.png`, `PP12_10_ProfilerBefore.png`, `PP12_11_ProfilerAfter.png`, `PP12_12_Settings.png`.

`PP12_08_SavedProgressAfterRestart.png` идентичен по содержимому `PP12_04_MainMenu_LevelSelect.png`, а `PP12_09_SavedVolume.png` — `PP12_12_Settings.png`. Поэтому эти файлы не считаются независимым доказательством перезапуска приложения; соответствующие тесты имеют статус `NOT VERIFIED`.

### УП.ПМ.13 — 13

`UP13_01_BeforeStyling.png`, `UP13_02_ImportedSpriteInspector.png`, `UP13_03_ObjectInspector.png`, `UP13_04_PrefabsFolder.png`, `UP13_05_CanvasScaler.png`, `UP13_06_RectTransformAnchors.png`, `UP13_07_ReadyHUD.png`, `UP13_08_EventFeedback.png`, `UP13_09_MainMenu.png`, `UP13_10_Pause.png`, `UP13_11_Victory.png`, `UP13_12_Defeat.png`, `UP13_13_FinalProduct.png`.

Переснятые реальные кадры показывают Sprite Inspector и установленный пакет 2D (`UP13_02`), Hazard Inspector (`UP13_03`) и четыре prefab-файла Player/Collectible/Hazard/UIButton в Project (`UP13_04`). Их актуальные SHA-256 приведены ниже.

### ПП.ПМ.13 — 11

`PP13_01_BeforeReleaseChanges.png`, `PP13_02_16x9.png`, `PP13_03_18x9.png`, `PP13_04_4x3.png`, `PP13_05_SafeAreaSimulation.png`, `PP13_06_Tutorial.png`, `PP13_07_PauseTransition.png`, `PP13_08_ProductPageMockup.png`, `PP13_09_AboutScreen.png`, `PP13_10_BeforeUserFix.png`, `PP13_11_AfterUserFix.png`.

### Store — 6

`STORE_01_Menu.png`, `STORE_02_Tutorial.png`, `STORE_03_Gameplay.png`, `STORE_04_Pause.png`, `STORE_05_Result.png`, `STORE_06_About.png`.

### Дополнительные — 3

`TUTORIAL_01_Controls.png`, `TUTORIAL_02_SpecialRule.png`, `TUTORIAL_03_Completion.png`.

Все 57 файлов в `EVIDENCE/screenshots` имеют MIME-тип `image/png` по самостоятельной проверке. Кадры Profiler имеют размер 1800×1212; макет страницы — 1440×1800; остальные кадры Unity — 3420×2148 или 3420×2224.

## Release

| Файл | Размер изображения | Байты | SHA-256 |
|---|---:|---:|---|
| `Release/icon_512.png` | 512×512 | 145742 | `9f1680da77d711aa3254514009a74a8ab579ab7b1cb4a55a20ea0a6cc1d4e7d0` |
| `Release/promo_1280x720.png` | 1280×720 | 338413 | `04c79f520ce9e1c8be4216c706b6e49e13262ec5d74c00f59f99e7b90c1f3dd2` |
| `Release/product_page_mockup.png` | 1440×1800 | 573145 | `440fda568be2aa49c34185fcadf32d7ed68c00f9cc8f2e3a247c0bdb2bff95d6` |
| `Release/Screenshots/STORE_01_Menu.png` | 3420×2148 | 741538 | `42c652ccde36f92594ef839e5d6fdf8266cb1524bc04da001a6b9f338ebb9027` |
| `Release/Screenshots/STORE_02_Tutorial.png` | 3420×2148 | 842882 | `2220d98750b7f8bb428625973c0500a4520bb5c2ee5c7b80e8e82c09ab8038c0` |
| `Release/Screenshots/STORE_03_Gameplay.png` | 3420×2148 | 869440 | `e52aa6183ffa5225f157ad4db7b88628dd7b0c9b826d27c28c97088a7d51a759` |
| `Release/Screenshots/STORE_04_Pause.png` | 3420×2148 | 779960 | `c779e11224dcbbf0037824e8921553394aac51001970be47e68c25d7bf474b28` |
| `Release/Screenshots/STORE_05_Result.png` | 3420×2148 | 797120 | `fbe58e79cf12feac53f1c8b7d10198996a381b24b94dcee8b0c0c2d572fb6665` |
| `Release/Screenshots/STORE_06_About.png` | 3420×2148 | 710464 | `e2d5cd39a15673a8a3ff32d627dd514dffc2ed3b2c1b64e7c5143b89bcc12839` |

Документы Release:

- `Release/Docs/DESCRIPTION.md` — краткое описание 73 символа, полное 988 символов по структурной проверке;
- `Release/Docs/PLAYER_GUIDE.md`;
- `Release/Docs/CREDITS.md`;
- `Release/Docs/CHANGELOG.md`;
- `Release/storyboard.md` — 7 кадров, заявленная длительность 80 секунд;
- `Release/README.md` — навигация по release-комплекту; 2986 байт; SHA-256 `403459a1cde37b8012c668381451804533f4d1ae145f19f2c7e9d449042bd602`.

## Контрольные доказательства

| Файл | Байты | SHA-256 |
|---|---:|---|
| `EVIDENCE/EditModeTests-final.xml` | 21906 | `f525b39058a4379298e227e8a9de1f5e6867d2d4d165c98fb786bd9a31cb2314` |
| `EVIDENCE/PROJECT_VALIDATION.md` | 25702 | `e8ebf66893837bd62fa94c93dcb8f1d4184e1092514ad2728055fbc82b854714` |
| `EVIDENCE/PROJECT_VALIDATION.json` | 50459 | `592a1ce9384c8927c457a0bb4d390b262e002e33b3818f0b4e0bdc755e4b6b38` |
| `EVIDENCE/logs/final-compile.log` | 22538 | `13b7a528e7ac030ed25ec806bbebb65b9cc55cf7b88c4f36a06033b4347dcb85` |
| `EVIDENCE/logs/editmode-tests-final.log` | 26389 | `bfd0af54c299526188368241e58907c19ff1d0d245193b0914383385a8e94dcd` |
| `EVIDENCE/logs/project-validation-final.log` | 21976 | `0c4ad6b75bafb926be659d0432ebb0d344b6b49b3d8d31d7320364a919156807` |
| `EVIDENCE/screenshots/PP12_10_ProfilerBefore.png` | 441171 | `47d38c25654cbbe64a117f193aa3ec9d8757eb6a6fcd25c20456fb7a22b54b90` |
| `EVIDENCE/screenshots/PP12_11_ProfilerAfter.png` | 349039 | `110573f61f2eb9bca6fe6e23f14b9553afb25c60be23a680f8fcee3d6e57e106` |
| `EVIDENCE/screenshots/PP13_10_BeforeUserFix.png` | 737539 | `977582567374fe71dc5f1b6c7a2cb2acb18d1d174c5ad984ecf0ae8e76b59506` |
| `EVIDENCE/screenshots/PP13_11_AfterUserFix.png` | 783415 | `56a2f7ec7be47d3d3b66e0c5d13612239e31fb3b5536aa7ea8cee6b95cabf05e` |
| `EVIDENCE/screenshots/UP13_02_ImportedSpriteInspector.png` | 1153495 | `cd20515377c1791bb6b853d6aede99722b80f4ba226ade6e75c0e885e274a3a5` |
| `EVIDENCE/screenshots/UP13_03_ObjectInspector.png` | 1865066 | `98b00755e3bd2af7a8944b35945a541e84fb72843b23e876f2b23ef7227e5724` |
| `EVIDENCE/screenshots/UP13_04_PrefabsFolder.png` | 590704 | `bb4db5de05a037928dea83d89da77a0558a0067c7921ab47568e3432a1e64500` |
| `EVIDENCE/screenshots/TUTORIAL_01_Controls.png` | 821514 | `3636def0caef60259498f1c73d54931767a888091fc866ba445c743ec2d27fcc` |
| `EVIDENCE/screenshots/TUTORIAL_02_SpecialRule.png` | 842882 | `2220d98750b7f8bb428625973c0500a4520bb5c2ee5c7b80e8e82c09ab8038c0` |
| `EVIDENCE/screenshots/TUTORIAL_03_Completion.png` | 878101 | `b49dd8a951a63849fc972f9ef08a11c20b281583f0b2e7bebe216e7cfabfcdbc` |

## Git-исключения и упаковка

`git ls-files` не показывает файлов внутри `Library`, `Temp`, `Logs`, `obj`, `UserSettings`, `Build` или `Builds`. Эти каталоги могут существовать в рабочей копии как локальные продукты Unity, но не отслеживаются Git и должны быть исключены из итогового ZIP.

В составе доказательств отсутствуют лог запуска чистой копии ревизии `pp-pm13-release`, размер и SHA-256 итогового ZIP. Эти проверки остаются `NOT VERIFIED`; см. `MISSING_EVIDENCE.md`.
