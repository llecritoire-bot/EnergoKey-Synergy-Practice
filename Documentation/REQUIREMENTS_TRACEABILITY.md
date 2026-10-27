# Трассируемость требований проекта «Энергоключ»

Версия документа: 1.0.0. Таблицы связывают технические требования с реализацией и фактическими доказательствами. Наличие файла или фрагмента кода само по себе не подтверждает работоспособность: окончательный статус определяется только запуском, тестом, журналом или снимком.

## Игровая реализация

| Требование | Реализация | Доказательство |
|---|---|---|
| Unity `6000.6.2f1`, 2D, C#, landscape | `ProjectSettings/ProjectVersion.txt`, `ProjectSettings/ProjectSettings.asset`, `Packages/manifest.json` | версия Editor и `EVIDENCE/FINAL_STATUS.md` |
| Один проект для четырёх этапов практики | `Assets/`, `Packages/`, `ProjectSettings/`, `Release/` | Git-история, теги и `EVIDENCE/FILE_MANIFEST.md` |
| Экранное управление и клавиатура в Editor | `Assets/Scripts/Gameplay/PlayerController.cs`, `MobileInputButton.cs` | `UP12_03_GameRunning.png`, результаты Т01 ПП.ПМ.12 |
| Стены и границы | коллайдеры, построение уровней в `SceneBootstrap.cs` | `UP12_02_SceneOverview.png`, фактическая проверка столкновений |
| Сбор кристаллов | `Collectible.cs`, `GameController.Collect` | `UP12_05_CollectibleInspector.png`, `UP12_08_Gameplay.png` |
| Выход закрыт до сбора всех кристаллов | `ExitGate.cs`, `GameController.TryUseExit` | `UP12_09_SpecialRule.png`, визуальная и звуковая реакция |
| Опасность отнимает жизнь и возвращает игрока на старт | `Hazard.cs`, `GameController.DamagePlayer`, `PlayerController.Respawn` | изменение HUD и фактическая проверка возврата |
| Три жизни, победа, поражение и Restart | `GameRules.StartingLives`, `GameController`, `SceneNavigator` | `UP12_10_Victory.png` – `UP12_12_Restart.png` |
| HUD: кристаллы, жизни, счёт и таймер | `HUDController.cs`, `GameController.cs` | `UP13_07_ReadyHUD.png`, тест паузы |
| Счёт `crystals × 100 + lives × 50` | `GameRules.CalculateScore` | EditMode-тест и итоговый HUD |

## Уровни и навигация

| Требование | Реализация | Доказательство |
|---|---|---|
| Уровни 1–3: 3/2, 4/4 и 5/6 объектов | `GameRules`, конфигурации в `SceneBootstrap.cs` | `PP12_01_Level1.png` – `PP12_03_Level3.png` |
| Рост сложности и проходимость | схемы стен и объектов каждого уровня | фактическое прохождение и Т07–Т09 ПП.ПМ.12 |
| Последовательное открытие уровней | `SaveService.RegisterVictory`, Level Select | `PP12_04_MainMenu_LevelSelect.png`, `PP12_08_SavedProgressAfterRestart.png` |
| После уровня 3 нет уровня 4 | `GameController.NextOrMenu` | финальная победа и возврат в меню |
| Все обязательные экраны | UI в `SceneBootstrap.cs` и контроллерах | снимки Main Menu, Level Select, Gameplay, Pause, Victory, Defeat, Settings, Tutorial и About |

## Сохранения, обучение и пауза

| Требование | Реализация | Доказательство |
|---|---|---|
| Максимальный открытый уровень | `energokey.unlockedLevel` в `SaveService.cs` | состояние после независимого запуска |
| Лучший результат | `energokey.bestScore` | значения до и после победы и повторного запуска |
| Громкость | `energokey.masterVolume`, `SettingsPanelController.cs`, `AudioService.cs` | `PP12_09_SavedVolume.png`, повторное открытие приложения |
| Факт просмотра обучения | `energokey.tutorialSeen`, `TutorialController.cs` | первый запуск, следующий запуск и «Как играть» |
| Restart не стирает общий прогресс | `GameController`, `SceneNavigator`, `SaveService` | сравнение сохранений до и после Restart |
| Три карточки и пропуск | `TutorialController.cs` | `PP13_06_Tutorial.png`, Т05–Т08 ПП.ПМ.13 |
| Пауза через `Time.timeScale` | `PauseController.cs` | `PP12_05_Pause.png`, `PP13_07_PauseTransition.png` |
| Скрытые панели не перехватывают ввод | `CanvasGroup.interactable` и `blocksRaycasts` | Т04 ПП.ПМ.13 |
| Потеря фокуса включает паузу | `PauseController.OnApplicationFocus`, `OnApplicationPause` | журнал standalone-проверки |

## Оформление и выпуск

| Требование | Реализация | Доказательство |
|---|---|---|
| Единый визуальный стиль и PNG | `Assets/Resources/Art/`, `Tools/generate_original_assets.py` | снимки до и после оформления |
| Шесть WAV-сигналов | `Assets/Resources/Audio/`, `AudioService.cs` | manifest и фактическое воспроизведение |
| Prefab Player, Collectible, Hazard и UI Button | `Assets/Resources/Prefabs/` | `UP13_04_PrefabsFolder.png`, Inspector-снимки |
| Canvas Scaler и anchors | `UIFactory.CreateCanvas`, `RectTransform` элементов | `UP13_05_CanvasScaler.png`, `UP13_06_RectTransformAnchors.png` |
| Форматы 16:9, 18:9 и 4:3 | адаптивная разметка | `PP13_02_16x9.png` – `PP13_04_4x3.png` |
| Safe Area и имитация в Editor | `SafeAreaAdapter.cs` | `PP13_05_SafeAreaSimulation.png` |
| About и Version 1.0.0 | UI About, `ProjectSettings.asset`, `README.md` | `PP13_09_AboutScreen.png`, сверка версии |
| Иконка, promo и макет продукта | `Release/icon_512.png`, `Release/promo_1280x720.png`, `Release/product_page_mockup.png` | проверка размеров и `FILE_MANIFEST.md` |
| Не менее шести игровых кадров | `Release/Screenshots/STORE_01_Menu.png` – `STORE_06_About.png` | фактические изображения игрового UI |
| Описания, руководство, credits, changelog и storyboard | `Release/Docs/`, `Release/storyboard.md`, `README.md` | наличие и проверка объёма материалов |

## Тестирование, Profiler, Git и Evidence

| Требование | Файл или механизм | Правило подтверждения |
|---|---|---|
| Т01–Т16 ПП.ПМ.12 | первая таблица `EVIDENCE/TEST_RESULTS.md` | ожидаемый и фактический результат, статус и ссылка на доказательство |
| Т01–Т16 ПП.ПМ.13 | вторая таблица `EVIDENCE/TEST_RESULTS.md` | статус только после фактического запуска или наблюдения |
| Profiler до и после | `PP12_10_ProfilerBefore.png`, `PP12_11_ProfilerAfter.png`, журналы и Git-теги | одна сцена, сопоставимый сценарий и реальные измерения |
| Пользовательская проверка | `EVIDENCE/USER_TEST.md`, снимки до и после UX-исправлений | проблемы предварительной проверки и внешняя повторная проверка описываются раздельно |
| Итоговый статус | `EVIDENCE/FINAL_STATUS.md` | каждый пункт получает `PASS`, `FAIL`, `NOT VERIFIED` или `NOT APPLICABLE` с основанием |
| Состав поставки | `EVIDENCE/FILE_MANIFEST.md`, `Tools/verify_deliverable.py` | обязательные файлы присутствуют, запрещённые каталоги отсутствуют |
| Git и публичный репозиторий | локальная история, теги, `EVIDENCE/REPOSITORY_URL.md` | реальный URL, `git ls-remote`, совпадение локального и удалённого HEAD |
| Свежий клон | `EVIDENCE/FRESH_CLONE_TEST.md` | импорт в чистом каталоге, запуск, проверка Console и основного сценария |

## Граница подтверждения

Статусы в Evidence обновляются только после соответствующей проверки. Если действие технически не выполнено, применяется статус `NOT VERIFIED` и указывается конкретная причина. Скриншоты Unity Editor и Game View должны быть фактическими; макеты рекламных материалов не заменяют игровые кадры.
