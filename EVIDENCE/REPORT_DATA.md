# Фактические данные для отчётов по практикам

Дата сводки: 27 октября 2026 года. Персональные и титульные данные студента намеренно не заполнены.

## Среда

- Unity version: `6000.6.2f1` (`770e33f6875c`).
- Устройство: физический MacBook с Apple M3.
- OS: macOS 27.0, build 26A428; Darwin 27.0.0; arm64; без Rosetta.
- Main scene: `Assets/Scenes/MainMenu.unity`.
- Orientation: Landscape Left по умолчанию; разрешены Landscape Left/Right; портретные ориентации отключены.
- Reference Resolution: `1280×720`.
- Screen Match Mode / Match: `Match Width Or Height`, `0.5`.
- Версия игры: `1.0.0`.
- Итоговая компиляция: PASS, `EVIDENCE/logs/final-compile.log`.
- EditMode: PASS, 24/24; итоговый прогон начат `2026-10-27 16:22:12Z`, `EVIDENCE/EditModeTests-final.xml`, `EVIDENCE/logs/editmode-tests-final.log`.
- PlayMode: PASS, 17/17; итоговый успешный прогон начат `2026-10-27 16:22:54Z`; проверены физика, UI, три уровня, Restart, обучение, Pause, callback-логика фокуса, responsive renders и независимые кадры сохранений, `EVIDENCE/PlayModeTests-final.xml`.
- Структурная проверка: PASS, 165/165, включая `ENV-2D-SPRITE-PACKAGE` и `ENV-2D-DEFAULT-MODE`, `EVIDENCE/PROJECT_VALIDATION.md`.
- macOS Standalone Development Build: PASS, 0 ошибок, 0 предупреждений; фактический запуск MainMenu подтверждён runtime-маркером, `EVIDENCE/MAC_STANDALONE_TEST.md`.
- PlayerPrefs между независимыми Unity-процессами: PASS, `EVIDENCE/PERSISTENCE_RESTART_PROBE.md`.

## Визуальная палитра

Точные значения финальных PNG заданы целочисленными RGBA-константами в `Tools/generate_original_assets.py`; float-цвета в `ThemeAssets.cs` используются отдельно как runtime tints и fallback и не выдаются за точные цвета пикселей assets.

| HEX | Область применения |
|---|---|
| `#050E1B` → `#081D2D` | Тёмный градиент игрового фона |
| `#0D1F30` с alpha `F2` | Нейтральная заливка UI-панелей |
| `#2F4E65` | Границы UI-панелей |
| `#43D9FF` / `#A6F4FF` | Player, голубые акценты и элементы управления |
| `#FFD84D` / `#FFF6AB` | Кристалл и золотой замок |
| `#FF4C5B` / `#84182B` | Опасность, урон и иконка жизни |
| `#49E38F` / `#B5FFD7` | Открытый выход и положительный акцент |
| `#E5F8FF` | Основной светлый текст и контуры |
| `#8EAEC1` | Вторичный текст Release-материалов |

## Import Settings финальной графики

Значения ниже подтверждены текущими `.meta`: `textureType: 8` соответствует Sprite (2D and UI), `filterMode: 1` — Bilinear; mipmaps отключены (`enableMipMap: 0`). Реальный Inspector показан в `EVIDENCE/screenshots/UP13_02_ImportedSpriteInspector.png`.

| Asset | Texture Type | Filter Mode | Pixels Per Unit |
|---|---|---|---:|
| `Assets/Resources/Art/Final/background.png` | Sprite (2D and UI) | Bilinear | 80 |
| `Assets/Resources/Art/Final/player.png` | Sprite (2D and UI) | Bilinear | 256 |
| `Assets/Resources/Art/Final/crystal.png` | Sprite (2D and UI) | Bilinear | 256 |
| `Assets/Resources/Art/Final/hazard.png` | Sprite (2D and UI) | Bilinear | 256 |

## Сопоставление имён скриптов из шаблона

Имена из методических материалов являются функциональными примерами. В проекте те же обязанности реализованы следующими фактическими файлами:

| Имя из шаблона | Фактический эквивалент | Реализованная функция |
|---|---|---|
| `VisualTheme.cs` | `Assets/Scripts/Core/ThemeAssets.cs`, `Tools/generate_original_assets.py` | Выбор Final/Prototype, загрузка спрайтов, runtime tints/fallback; генератор задаёт точную палитру финальных PNG. |
| `CollectibleVisual.cs` | `Assets/Scripts/Core/SceneBootstrap.cs`, `Assets/Scripts/Gameplay/Collectible.cs` | `CreateCollectible` назначает crystal sprite, масштаб и sorting order; `Collectible` обрабатывает контакт и скрывает собранный объект. Отдельного `CollectibleVisual.cs` нет. |
| `HUDView.cs` | `Assets/Scripts/UI/HUDController.cs`, `SceneBootstrap.CreateHud` | Создание HUD и событийное обновление кристаллов, жизней и счёта; таймер меняется только при смене секунды. |
| `AudioFeedback.cs` | `Assets/Scripts/Core/AudioService.cs`, вызовы в `GameController.cs` и `UIFactory.cs` | Загрузка и проигрывание cue `collect`, `error`, `damage`, `victory`, `defeat`, `button`. |
| `ScreenMockupController.cs` | `Assets/Scripts/Core/SceneBootstrap.cs`, `Assets/Scripts/UI/UIFactory.cs` | Построение реальных runtime-экранов Main Menu, Level Select, Gameplay, Settings, Tutorial, About, Pause, Victory и Defeat. Статический `Release/product_page_mockup.png` — отдельный release-макет, не игровой экран. |

## Вариант

- Условный номер варианта: официальный номер в исходных документах не указан; выбран самостоятельный простой сценарий collect-and-exit.
- Название: «Энергоключ».
- Жанр: 2D top-down arcade / collect-and-exit.
- Основное действие: перемещение игрока четырьмя экранными кнопками, сбор энергетических кристаллов и достижение выхода.
- Особое правило: выход заблокирован, пока не собраны все кристаллы уровня.
- Условия победы: собрать все кристаллы и войти в открытый выход.
- Условия поражения: потерять все 3 жизни при касаниях опасностей.
- Формула счёта: `collectedCrystals × 100 + remainingLives × 50`.

## Уровни

| Уровень | Кристаллы | Опасности | Отличие | Фактический результат |
|---|---:|---:|---|---|
| 1 | 3 | 2 | Простой маршрут, две внутренние стены | PASS по структурной проверке; Game View: `screenshots/PP12_01_Level1.png` |
| 2 | 4 | 4 | Более плотная схема и дополнительная стена | PASS по структурной проверке; Game View: `screenshots/PP12_02_Level2.png` |
| 3 | 5 | 6 | Наибольшее число объектов и пять внутренних стен | PASS по структурной проверке; Game View: `screenshots/PP12_03_Level3.png` |

Полный последовательный L1–L3 flow проверен PlayMode-тестом: уровни открываются по порядку, финал L3 возвращает в меню и не создаёт L4. Это автоматизированный runtime-прогон Unity, а не видеозапись ручного прохождения.

## Сохранения

- ключ unlockedLevel: `energokey.unlockedLevel`;
- ключ bestScore: `energokey.bestScore`;
- ключ masterVolume: `energokey.masterVolume`;
- ключ tutorialSeen: `energokey.tutorialSeen`.

Факты автоматизированной проверки:

- новая запись использует документированные значения по умолчанию;
- победа открывает следующий уровень и сохраняет лучший счёт;
- меньший результат не понижает прогресс или рекорд;
- победа на уровне 3 не создаёт уровень 4;
- громкость ограничивается диапазоном 0–1 и читается после записи;
- `tutorialSeen` сохраняется.

Источники: `EVIDENCE/EditModeTests-final.xml` (24/24), `EVIDENCE/PlayModeTests-final.xml` (17/17) и двухпроцессный `EVIDENCE/PERSISTENCE_RESTART_PROBE.md`. Процесс WRITE полностью завершился до запуска READ; повторно получены `unlockedLevel=3`, `bestScore=650`, `masterVolume=0.37`, `tutorialSeen=true`. Кадры `PP12_08_SavedProgressAfterRestart.png` и `PP12_09_SavedVolume.png` заново созданы Unity как независимые runtime-renders 1366×768 и отличаются от прежних comparison-кадров.

## Git

- URL: `https://github.com/llecritoire-bot/EnergoKey-Synergy-Practice`.
- Основная ветка: `main`.
- Первый рабочий прототип: `74ca90f5b76beb09c239c98e9516cb91c79b5727`.
- Тег прототипа: `up-pm12-prototype` → `6f7f133e526319f6c0a184514ff9bc1a99b6bb5a`.
- Тег трёх уровней: `pp-pm12-three-levels` → `47064d82b5b13fa1600ad69d2ee5432cea02da0e`.
- Тег до визуального стиля: `pm13-before-visual-style` → `47064d82b5b13fa1600ad69d2ee5432cea02da0e`.
- Тег оформленного этапа: `up-pm13-styled` → `d0b7433011ef097a5b92ea4799ac92d2887fa5c4`.
- Тег `profiler-before` → `d0b7433011ef097a5b92ea4799ac92d2887fa5c4`.
- Commit событийного HUD: `11771ef31459a3aad844614287306db68119c8d2`.
- Сведения о проверенной опубликованной ревизии, `git ls-remote` и тегах: `EVIDENCE/REPOSITORY_URL.md`.
- Результаты новой чистой копии после публикации: `EVIDENCE/FRESH_CLONE_TEST.md`.
- Старые авторы, даты и этапные теги сохранены без переписывания.

## Profiler

- Изменение: HUD переведён с полного обновления каждый кадр на подписку `GameController.StateChanged`; таймер обновляется только при смене секунды.
- Сравниваемые ревизии: `d0b7433011ef097a5b92ea4799ac92d2887fa5c4` до и `11771ef31459a3aad844614287306db68119c8d2` после.
- Одинаковый сценарий: 120 кадров прогрева, затем 600 измеренных кадров; один HUD, четыре Text и пять одинаковых изменений состояния.
- До: marker полного HUD refresh присутствовал в 600/600 кадрах, суммарно 1 845 755 ns.
- После: marker событийного refresh присутствовал в 5/600 кадрах, суммарно 57 749 ns.
- В одинаковом microbenchmark число полных обновлений снизилось с 600 до 5, суммарное время целевого marker уменьшилось примерно в 31.96 раза (на 96.87%). Это не заявляется как ускорение полного кадра всей игры.
- Машинные результаты: `PROFILER_BENCHMARK.md`, `PROFILER_BENCHMARK_BEFORE.json`, `PROFILER_BENCHMARK_AFTER.json`, `ProfilerBenchmarkBeforeTests.xml`, `ProfilerBenchmarkAfterTests.xml`.
- Реальные GUI-кадры Profiler также сохранены: `screenshots/PP12_10_ProfilerBefore.png` и `screenshots/PP12_11_ProfilerAfter.png`.

## Пользовательская проверка

- Внешний новый пользователь был: да; один участник, ранее не знакомый с проектом, без фиксации ФИО.
- Готовая версия предоставлялась без предварительного устного объяснения механики.
- Пользователь самостоятельно освоил экранные кнопки, понял правило «собрать все кристаллы → идти к выходу», завершил Level 1, открыл Pause и продолжил игру.
- Дополнительные пояснения не потребовались; вопросов и замечаний не возникло.
- Устройство внешнего пользовательского сеанса отдельно не фиксировалось; этот сеанс не считается проверкой Android/iOS.
- До внешнего сеанса самостоятельная техническая проверка выявила, что в 4:3 нижняя кнопка D-pad частично выходила за видимую область.
- Исправление: блок D-pad поднят на 35 px; нижняя кнопка перемещена с `y=15` на `y=50`.
- Повторная проверка: PASS в Unity Game View 1024×768; кнопка отображается целиком.
- Дополнительное найденное отклонение: в Level Select 1024×768 подсказка пересекалась с кнопкой «Назад»; положение исправлено и закреплено assertion `BackButton.Overlaps(UnlockHint) == false`.
- Подробности: `EVIDENCE/USER_TEST.md`, `EVIDENCE/USER_ACCEPTANCE.md`, `EVIDENCE/responsive/BEFORE_1024x768_LevelSelect_overlap.png`, `EVIDENCE/responsive/RESP_1024x768_LevelSelect.png`.

## Тесты ПП.ПМ.13

| ID | Фактическое наблюдение | Статус | Рисунок/лог |
|---|---|---|---|
| T01 | Unity создал 21 runtime-render: Main Menu, Level Select, Gameplay, Pause, Victory, Defeat и Settings в 1280×720, 1440×720 и 1024×768; bounds assertions PASS. | PASS | `responsive/RESP_*.png`, `PlayModeTests-final.xml` |
| T02 | Имитация Safe Area показана; все видимые controls в 21 render прошли проверку границ. | PASS | `screenshots/PP13_05_SafeAreaSimulation.png`, `PlayModeTests-final.xml` |
| T03 | Pointer-тест прошёл обязательные кнопки Main Menu → Level Select → Gameplay и проверил D-pad/Pause. | PASS | `PlayModeTests-final.xml` |
| T04 | Скрытые панели исключены из raycasts; underlying D-pad сохранил ввод. | PASS | `PlayModeTests-final.xml` |
| T05 | При чистом `tutorialSeen=false` автоматически открылась карточка «Управление», 1/3. | PASS | `PlayModeTests-final.xml` |
| T06 | «Далее» последовательно открыл 1/3, 2/3, 3/3 и вернул Main Menu. | PASS | `PlayModeTests-final.xml` |
| T07 | Skip сохранил tutorialSeen, level, score и volume; после reload автопоказа не было. | PASS | `PlayModeTests-final.xml`, `PERSISTENCE_RESTART_PROBE.md` |
| T08 | «Как играть» повторно открыл карточку «Управление», 1/3. | PASS | `PlayModeTests-final.xml` |
| T09 | Во время fade подтверждены `Time.timeScale=0`, Pause и состояние transition. | PASS | `PlayModeTests-final.xml` |
| T10 | Быстрые повторные Toggle/Pause/Resume проигнорированы до окончания transition; Resume завершился корректно. | PASS | `PlayModeTests-final.xml` |
| T11 | Version 1.0.0 совпадает в About, Player Settings и README. | PASS | `screenshots/PP13_09_AboutScreen.png`, `PROJECT_VALIDATION.md` |
| T12 | Обязательная PNG-матрица полна; дополнительно созданы 21 валидный Unity runtime-render точных размеров. | PASS | `FILE_MANIFEST.md`, `responsive/RESP_*.png` |
| T13 | UX-01 и UX-02 найдены самостоятельной технической проверкой и исправлены до внешнего сеанса; новый пользователь затем проверил исправленную версию. | PASS | `USER_TEST.md`, `USER_ACCEPTANCE.md`, `responsive/BEFORE_1024x768_LevelSelect_overlap.png`, `responsive/RESP_1024x768_LevelSelect.png` |
| T14 | Новый пользователь без устного объяснения понял необходимость собрать все кристаллы до выхода и успешно завершил Level 1. | PASS | `USER_TEST.md`, `USER_ACCEPTANCE.md` |
| T15 | PlayMode прошёл L1–L3, Victory, Defeat/Restart; независимые процессы подтвердили сохранения. | PASS | `PlayModeTests-final.xml`, `PERSISTENCE_RESTART_PROBE.md` |
| T16 | Проверка свежего клона выполняется после первой публикации финальной ветки; фактическая ревизия и результат будут записаны в отдельном протоколе. | NOT VERIFIED | `FRESH_CLONE_TEST.md`, `REPOSITORY_URL.md` |

Промежуточный итог до проверки свежего клона: 15 PASS, 0 FAIL, 1 NOT VERIFIED.

Полные ожидаемые и фактические результаты приведены в `EVIDENCE/TEST_RESULTS.md`.

## Ошибки и исправления

| Ошибка | Причина | Исправление | Проверка после исправления |
|---|---|---|---|
| `UX-01`: обрезание нижней кнопки D-pad в 1024×768 | Вертикальная позиция Down была 15 px при высоте кнопки 72 px | Весь D-pad поднят на 35 px; Down установлена на 50 px | PASS, `screenshots/PP13_10_BeforeUserFix.png` и `PP13_11_AfterUserFix.png` |
| `UX-02`: подсказка Level Select пересекалась с кнопкой «Назад» в 1024×768 | Нижняя подсказка и BackButton занимали пересекающиеся области 4:3 | BackButton поднята с `y=-275` до `y=-200`; anchors подсказки перенесены ниже (`0.02–0.09`) | PASS, кадры `responsive/BEFORE_1024x768_LevelSelect_overlap.png` и `responsive/RESP_1024x768_LevelSelect.png`; assertion отсутствия overlap во всех трёх форматах при включённой Safe Area simulation |

## Ограничения

- Непроверенные платформы: физические Android/iOS-устройства; мобильная сборка не создавалась. macOS Standalone на физическом MacBook Apple M3 собран и запущен успешно.
- Известные ошибки Editor/Standalone-компиляции: не обнаружены; macOS build завершён с 0 errors и 0 warnings.
- Реальное системное сворачивание macOS Standalone проверено: runtime зарегистрировал `hasFocus=False`/`paused=True`, а после возврата отображалась Pause с остановленным таймером (`FOCUS_LOSS_TEST.md`).
- Полный L1–L3 flow, физические контакты, Restart, Pause и сохранения подтверждены автоматизированными runtime-прогонами Unity, но отдельное непрерывное видео ручного прохождения не создавалось.
- Внешний новый пользователь проверил уже исправленную версию; устройство сеанса отдельно не фиксировалось.
- Публичный URL создан; проверка ветки, тегов и свежего клона фиксируется после первой отправки ревизии.
- Word/PDF-отчёты, PowerPoint-презентации, подписи, справка о практике и загрузка в LMS не входят в этот Unity ZIP и требуют ручной подготовки/оформления.
