# Трассируемость требований проекта «Энергоключ»

Версия документа: 1.0.0. Этот файл связывает требования исходного пакета колледжа с реализационными файлами и ожидаемыми доказательствами. Он не является протоколом прохождения тестов и сам по себе не присваивает статус `PASS`.

## Источники и приоритет

1. Оригинальные DOCX/PPTX из `codex_synergy_practice_pack/source_requirements/`.
2. Скопированные без изменения рабочие спецификации `Documentation/00_README_FOR_CODEX.md` – `Documentation/04_EVIDENCE_DATA_TEMPLATE.md`.
3. Аудиты исходников в соседнем исходном пакете: `analysis/DOCX_REQUIREMENTS_AUDIT.md` и `analysis/PPTX_REQUIREMENTS_AUDIT.md`.
4. Код, настройки и фактические Evidence-файлы текущей версии проекта.

При расхождении приоритет имеет конкретное оригинальное задание соответствующей практики. Факт наличия кода или файла не считается подтверждением поведения: такое подтверждение появляется только после реального запуска, наблюдения и сохранения доказательства.

## Игровая реализация

| Требование | Реализационные файлы | Проверяемое доказательство |
|---|---|---|
| Unity `6000.6.2f1`, 2D, C#, landscape | `ProjectSettings/ProjectVersion.txt`, `ProjectSettings/ProjectSettings.asset`, `Packages/manifest.json` | Версия Editor в реальном запуске, Project/Player Settings, `EVIDENCE/FINAL_STATUS.md` |
| Один проект для четырёх практик | единые `Assets/`, `Packages/`, `ProjectSettings/`, `Release/` | Git-история этапов, `EVIDENCE/FILE_MANIFEST.md`; четыре отчётных комплекта готовятся отдельно |
| Движение четырьмя экранными кнопками; WASD/стрелки в Editor | `Assets/Scripts/Gameplay/PlayerController.cs`, `MobileInputButton.cs`, построение UI в `SceneBootstrap.cs` | `UP12_03_GameRunning.png`, ручная проверка Т01 ПП.ПМ.12 |
| Стены и физические границы | `SceneBootstrap.cs`, коллайдеры сцены | `UP12_02_SceneOverview.png`, `UP12_06_HazardInspector.png`, проверка столкновений |
| Сбор кристаллов | `Collectible.cs`, `GameController.Collect` | `UP12_05_CollectibleInspector.png`, `UP12_08_Gameplay.png` |
| Выход закрыт до сбора всех кристаллов; понятная ошибка | `ExitGate.cs`, `GameController.TryUseExit`, сигналы `collect`/`error` | `UP12_09_SpecialRule.png`, журнал/наблюдение визуальной и звуковой реакции |
| Опасность отнимает одну жизнь и возвращает на старт | `Hazard.cs`, `GameController.DamagePlayer`, `PlayerController.Respawn` | кадры до/после урона и фактическая запись изменения HUD |
| Три жизни, победа, поражение, Restart | `GameRules.StartingLives`, `GameController`, `SceneNavigator` | `UP12_10_Victory.png`, `UP12_11_Defeat.png`, `UP12_12_Restart.png` |
| HUD: X/Y, жизни, счёт; видимый таймер для PPTX ПП.ПМ.12 | `HUDController.cs`, `GameController.cs`, построение HUD в `SceneBootstrap.cs` | `UP13_07_ReadyHUD.png`; для паузы — наблюдение неизменного таймера и `PP12_05_Pause.png` |
| Формула счёта `crystals × 100 + lives × 50` | `GameRules.CalculateScore` | значения HUD до/после события и запись итогового счёта |

## Уровни, экраны и навигация

| Требование | Реализационные файлы | Проверяемое доказательство |
|---|---|---|
| L1: 3 кристалла, 2 опасности; L2: 4/4; L3: 5/6 | `GameRules.CrystalCountForLevel`, `GameRules.HazardCountForLevel`, схемы уровней в `SceneBootstrap.cs` | `PP12_01_Level1.png` – `PP12_03_Level3.png`, подсчёт объектов в Hierarchy |
| Рост сложности и проходимые маршруты | конфигурации стен/объектов уровней в `SceneBootstrap.cs` | фактическое прохождение всех трёх уровней и протокол Т07–Т09 ПП.ПМ.12 |
| L2 открывается после L1, L3 после L2 | `SaveService.RegisterVictory`, логика Level Select | `PP12_04_MainMenu_LevelSelect.png`, `PP12_08_SavedProgressAfterRestart.png` |
| После L3 нет L4; финал возвращает в меню | `GameController.NextOrMenu` и финальная панель уровня | запись финальной победы L3 и отсутствия перехода к `Level4` |
| Main Menu, Level Select, Gameplay, Pause, Victory, Defeat, Settings, Tutorial, About | `SceneBootstrap.cs`, `UIFactory.cs`, соответствующие UI-контроллеры | `PP12_04`–`PP12_07`, `PP12_12`, `PP13_06`, `PP13_09` |

## Сохранения, обучение и пауза

| Требование | Реализационные файлы | Проверяемое доказательство |
|---|---|---|
| Максимальный открытый уровень | PlayerPrefs-ключ `energokey.unlockedLevel` в `SaveService.cs` | состояние до/после нового запуска, `PP12_08_SavedProgressAfterRestart.png` |
| Лучший результат | `energokey.bestScore` в `SaveService.cs` | фактические значения до/после победы и после нового запуска |
| Громкость | `energokey.masterVolume`, `SettingsPanelController.cs`, `AudioService.cs` | `PP12_09_SavedVolume.png`, повторное открытие приложения |
| Факт просмотра обучения | `energokey.tutorialSeen`, `TutorialController.cs` | первый запуск, повторный Play и кнопка «Как играть» |
| Новая попытка сбрасывает временное, но не общий прогресс | `GameController.Start`, перезагрузка сцены через `SceneNavigator`, отдельный `SaveService` | Restart плюс проверка PlayerPrefs до/после |
| Три карточки, пропуск, повторное открытие с карточки 1 | `TutorialController.cs` | `PP13_06_Tutorial.png`, Т05–Т08 ПП.ПМ.13 |
| Пауза через `Time.timeScale`, плавное появление и блокировка повторных кликов | `PauseController.cs` | `PP12_05_Pause.png`, `PP13_07_PauseTransition.png`, Т09–Т10 ПП.ПМ.13 |
| Скрытые панели не перехватывают ввод | `CanvasGroup.interactable`/`blocksRaycasts` в `PauseController.cs` и `TutorialController.cs` | ручная проверка Т04 ПП.ПМ.13 |
| Потеря фокуса/сворачивание включает паузу | `PauseController.OnApplicationFocus`, `OnApplicationPause` | фактический журнал и видимая Pause panel после возврата |

## Оформление, UI и выпуск

| Требование | Реализационные файлы/артефакты | Проверяемое доказательство |
|---|---|---|
| Единый стиль и собственные PNG | `Assets/Resources/Art/`, `Tools/generate_original_assets.py`, `Release/Docs/CREDITS.md` | `UP13_01_BeforeStyling.png`, `UP13_02_ImportedSpriteInspector.png`, `UP13_13_FinalProduct.png` |
| Шесть собственных WAV-сигналов | `Assets/Resources/Audio/`, `AudioService.cs`, `CREDITS.md` | прослушивание в фактическом запуске и `EVIDENCE/FILE_MANIFEST.md` |
| Prefab: Player, Collectible, Hazard, UI Button | `Assets/Resources/Prefabs/*.prefab` и `.meta` | `UP13_04_PrefabsFolder.png`, Inspector-кадры |
| Canvas Scaler 1280×720 и anchors | `UIFactory.CreateCanvas`, `RectTransform` создаваемых элементов | `UP13_05_CanvasScaler.png`, `UP13_06_RectTransformAnchors.png` |
| 16:9, 18:9, 4:3 | адаптивные anchors/Canvas Scaler | один и тот же экран в `PP13_02_16x9.png`, `PP13_03_18x9.png`, `PP13_04_4x3.png` |
| Safe Area и Editor-имитация | `SafeAreaAdapter.cs`; переключение F8 существует только в Editor | `PP13_05_SafeAreaSimulation.png`, фиксация включённой имитации |
| Экран «Об игре»: название, Version 1.0.0, нейтральный автор, сведения о ресурсах | разметка About в `SceneBootstrap.cs`, `ProjectSettings.asset`, корневой `README.md` | `PP13_09_AboutScreen.png` и сверка трёх источников версии |
| Иконка 512×512, promo 1280×720, mockup | `Release/icon_512.png`, `Release/promo_1280x720.png`, `Release/product_page_mockup.png` | визуальная проверка и размеры в `EVIDENCE/FILE_MANIFEST.md` |
| Не менее 6 настоящих кадров | `Release/Screenshots/STORE_01_Menu.png` – `STORE_06_About.png` | настоящие кадры игрового UI; рекламные макеты не заменяют их |
| Описание, руководство, credits, changelog, storyboard | `Release/Docs/`, `Release/storyboard.md`, корневой `README.md` | наличие, длина описаний, storyboard 6–8 кадров / 60–90 секунд |

## Тестирование, Profiler, Git и Evidence

| Требование | Файл/механизм | Правило подтверждения |
|---|---|---|
| Т01–Т16 ПП.ПМ.12 | первая таблица `EVIDENCE/TEST_RESULTS.md` | оригинальные DOCX/PPTX требуют 16 тестов, но не дают формулировок; рабочие сценарии явно помечены как составленные по механике и спецификации |
| Т01–Т16 ПП.ПМ.13 | вторая таблица `EVIDENCE/TEST_RESULTS.md` | `Expected` перенесены из `otchet_o_vypolnenii_proekta1.docx`; фактические наблюдения заполняются только после запуска |
| Profiler до/после | `PP12_10_ProfilerBefore.png`, `PP12_11_ProfilerAfter.png` и Git-состояния до/после | одна сцена и сопоставимый сценарий; никаких чисел без реального измерения |
| Пользовательская проверка и UX до/после | `EVIDENCE/USER_TEST.md`, `PP13_10_BeforeUserFix.png`, `PP13_11_AfterUserFix.png` | внешний пользователь не выдумывается; проблема выбирается только после наблюдения |
| Полный итоговый статус | `EVIDENCE/FINAL_STATUS.md` | каждый пункт `02_ACCEPTANCE_CHECKLIST.md` получает `PASS`, `FAIL` или `NOT VERIFIED` с причиной |
| Состав поставки | `EVIDENCE/FILE_MANIFEST.md`, `Tools/verify_deliverable.py` | проверяются обязательные файлы и отсутствие запрещённых каталогов в итоговой поставке |
| Логичная Git-история и итоговый hash | локальный Git, README, Evidence | hash записывается только после финальной проверки; URL не указывается без реальной публикации |

## Дополнительные требования оригинальных DOCX/PPTX

- Нужны четыре отдельные презентации по четырём шаблонам, хотя Unity-проект один. Последний слайд каждой требует приложение `Отчет о выполнении.docx`.
- Все четыре презентации содержат организационный этап, сведения об охране труда/технике безопасности, структуре базы практики, обработке материала, пять выводов и список литературы. Эти разделы не заменяются файлами проекта.
- ПП.ПМ.13 дополнительно требует PDF-отчёт. Надёжная передача включает PPTX по шаблону и отдельный фактический PDF-экспорт.
- В титуле УП.ПМ.12 указана кафедра Информационного менеджмента им. профессора В.В. Дика; в остальных трёх шаблонах — кафедра Цифровой экономики. Унифицировать эти строки автоматически нельзя.
- Персональные поля титулов сохраняются пустыми по текущему заданию.
- Timestamp `2026-09-21 ...` в заготовках УП.ПМ.13 является текстом шаблона, а не датой проверки. Его нельзя переносить на реальные кадры как факт.
- PPTX ПП.ПМ.12 требует показать паузу с остановленным таймером; поэтому видимый таймер и его фактическая остановка должны быть проверены отдельно.
- DOCX ПП.ПМ.13 требует минимум одного нового пользователя, а шаблон отчёта допускает честно описать самостоятельную проверку при его отсутствии. Самостоятельная проверка не закрывает внешний тест и остаётся ограничением.
- Производственные задания требуют репозиторий, доступный преподавателю. Локальный Git подтверждает историю, но не подтверждает доступность; публичный URL нельзя выдумывать.

## Граница подтверждения

Текущие рабочие таблицы могут содержать `NOT VERIFIED`. Реальные статусы, Profiler-значения, состояние новой копии Git, итоговый commit hash, результаты внешнего пользователя и отсутствующие снимки вносятся только после соответствующих действий. Если действие невозможно, оно переносится в `EVIDENCE/MISSING_EVIDENCE.md` с конкретной инструкцией для ручного выполнения.
