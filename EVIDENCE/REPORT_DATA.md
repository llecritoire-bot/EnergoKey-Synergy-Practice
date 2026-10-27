# Фактические данные для отчётов по практикам

Дата сводки: 27 октября 2026 года. Персональные и титульные данные студента намеренно не заполнены.

## Среда

- Unity version: `6000.6.2f1` (`770e33f6875c`).
- OS: macOS 27.0, build 26A428; Darwin 27.0.0; arm64; без Rosetta.
- Main scene: `Assets/Scenes/MainMenu.unity`.
- Orientation: Landscape Left по умолчанию; разрешены Landscape Left/Right; портретные ориентации отключены.
- Reference Resolution: `1280×720`.
- Screen Match Mode / Match: `Match Width Or Height`, `0.5`.
- Версия игры: `1.0.0`.
- Итоговая компиляция: PASS, `EVIDENCE/logs/final-compile.log`.
- EditMode: PASS, 24/24; итоговый прогон начат `2026-10-27 13:59:08Z`, `EVIDENCE/EditModeTests-final.xml`, `EVIDENCE/logs/editmode-tests-final.log`.
- Структурная проверка: PASS, 165/165, включая `ENV-2D-SPRITE-PACKAGE` и `ENV-2D-DEFAULT-MODE`, `EVIDENCE/PROJECT_VALIDATION.md`.

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

Непрерывное ручное прохождение всех трёх уровней от начала до финала не зафиксировано; этот пункт указан в `MISSING_EVIDENCE.md`.

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

Источник: `EVIDENCE/EditModeTests-final.xml`, 24/24 Passed. Фактический перезапуск приложения с записью значений до/после не проведён.

## Git

- URL: отсутствует; публичный удалённый репозиторий не создавался.
- Первый рабочий прототип: `3ca04d3`.
- Тег прототипа: `up-pm12-prototype` → `29ffcf0812c3308e3e4ec85f645c88e8a3d8bbd2`.
- Тег трёх уровней: `pp-pm12-three-levels` → `816092b00750fcb556d9996a8dfe7c01e77a8d85`.
- Тег до визуального стиля: `pm13-before-visual-style` → `816092b00750fcb556d9996a8dfe7c01e77a8d85`.
- Тег оформленного этапа: `up-pm13-styled` → `f616afd3be9b8586eba9f7d76af568f11bd2e9c3`.
- profiler-before commit/tag: `profiler-before` → `f616afd3be9b8586eba9f7d76af568f11bd2e9c3`.
- Commit событийного HUD: `2c60fe4ea10b9e95401f5909dffa915f09d407cd`.
- Идентификатор финальной ревизии: аннотированный тег `pp-pm13-release`; полный hash разрешается командой `git rev-list -n 1 pp-pm13-release`. Hash намеренно не встраивается в саму tagged revision, чтобы не создавать самоссылочное изменение; наличие и разрешение тега являются отдельной проверкой упаковки.
- Проверенный candidate commit: `91bff8f861632c1846e512aa57306d21d4e9d3f1`.
- Дата fresh-clone проверки candidate: 27 октября 2026 года.
- Путь временной копии: `/private/tmp/energokey-candidate.Aa5d3b/EnergoKey`.
- Чистота до Unity: PASS, `git status --short` — 0 строк.
- Read-only verifier candidate-копии: 27 PASS, 1 FAIL, 3 NOT VERIFIED; единственный FAIL возник потому, что на candidate-этапе release-тег ещё не существовал.
- Результат запуска candidate-копии: PASS — compile exit 0, EditMode 24/24, structural validation 165/165.
- Exact final tag и итоговый ZIP проверяются пост-коммитно; полный hash и результат tagged-clone/архивной проверки фиксируются в аннотации `pp-pm13-release` и `DELIVERY_MANIFEST.txt`, чтобы не создавать самоссылочное изменение tagged commit.
- Протокол: `EVIDENCE/FRESH_CLONE_VERIFICATION.md`.

## Profiler

- Сцена и условия: Unity Editor Play Mode, CPU Usage; реальные снимки Profiler до и после изменения. Оба кадра относятся к работе игры в Editor, но отображают разные выбранные номера кадров и разные режимы детализации нижней панели.
- Что изменено: HUD переведён с обновления текстов каждый кадр на подписку на `GameController.StateChanged`; таймер обновляется только при смене целой секунды.
- До: на выбранном одиночном кадре видно `CPU: 3.63 ms` — `screenshots/PP12_10_ProfilerBefore.png`.
- После: на выбранном одиночном кадре видно `CPU: 1.19 ms` — `screenshots/PP12_11_ProfilerAfter.png`.
- Вывод: на двух выбранных кадрах значение после ниже. Эти два числа не являются средним, медианой или серией измерений; статистическая значимость и точный вклад изменения не заявляются.

## Пользовательская проверка

- Внешний новый пользователь был: нет.
- Самостоятельная проверка выполнена: да.
- Затруднение: в 4:3 нижняя кнопка D-pad частично выходила за видимую область.
- Исправление: блок D-pad поднят на 35 px; нижняя кнопка перемещена с `y=15` на `y=50`.
- Повторная проверка: PASS в Unity Game View 1024×768; кнопка отображается целиком.
- Подробности: `EVIDENCE/USER_TEST.md`.

## Тесты ПП.ПМ.13

| ID | Фактическое наблюдение | Статус | Рисунок/лог |
|---|---|---|---|
| T01 | Gameplay проверен в 16:9, 18:9 и 4:3; не все семь экранов проверены во всех трёх форматах. | NOT VERIFIED | `screenshots/PP13_02_16x9.png`–`PP13_04_4x3.png` |
| T02 | Safe Area включена в Editor, HUD и кнопки видны внутри границы. | PASS | `screenshots/PP13_05_SafeAreaSimulation.png` |
| T03 | Полный обход всех кнопок мышью не записан. | NOT VERIFIED | `MISSING_EVIDENCE.md` |
| T04 | Фактическая проверка клика через каждую скрытую панель не записана. | NOT VERIFIED | `MISSING_EVIDENCE.md` |
| T05 | Карточка 1 показана; автоматический первый запуск на чистом ключе не записан. | NOT VERIFIED | `screenshots/TUTORIAL_01_Controls.png` |
| T06 | Показаны три карточки; полный переход «Далее» до меню не записан. | NOT VERIFIED | `screenshots/TUTORIAL_01_Controls.png`–`TUTORIAL_03_Completion.png` |
| T07 | Сохранение `tutorialSeen` прошло EditMode-тест; полный UI-сценарий Skip/Finish и новый Play не выполнен. | NOT VERIFIED | `EditModeTests-final.xml` |
| T08 | Кнопка и карточка 1 существуют; переход именно через «Как играть» не записан. | NOT VERIFIED | `screenshots/UP13_09_MainMenu.png` |
| T09 | Панель паузы показана; числовое `Time.timeScale` во время появления не записано. | NOT VERIFIED | `screenshots/PP13_07_PauseTransition.png` |
| T10 | Серия быстрых повторных кликов не проводилась. | NOT VERIFIED | `MISSING_EVIDENCE.md` |
| T11 | Version 1.0.0 совпадает в About, Player Settings и README. | PASS | `screenshots/PP13_09_AboutScreen.png`, `PROJECT_VALIDATION.md` |
| T12 | 57 Evidence PNG и 9 Release PNG распознаны; обязательная матрица полна. | PASS | `FILE_MANIFEST.md` |
| T13 | Честный self-test с реальной проблемой 4:3 и кадрами до/после. | PASS | `USER_TEST.md`, `screenshots/PP13_10_BeforeUserFix.png`, `screenshots/PP13_11_AfterUserFix.png` |
| T14 | Правило показано, но понимание новым пользователем не проверено. | NOT VERIFIED | `screenshots/TUTORIAL_02_SpecialRule.png` |
| T15 | Отдельные состояния трёх уровней, Victory, Defeat и сохранений показаны; полного прохождения с перезапуском нет. | NOT VERIFIED | `TEST_RESULTS.md` |
| T16 | Чистая копия candidate commit `91bff8f861632c1846e512aa57306d21d4e9d3f1` успешно прошла compile, EditMode 24/24 и validation 165/165. Exact tagged clone и ZIP дополнительно проверяются пост-коммитно с записью в `DELIVERY_MANIFEST.txt`. | PASS | `FRESH_CLONE_VERIFICATION.md`, `FreshCloneTests-candidate.xml`, `logs/fresh-clone-candidate-*.log`, архивный `DELIVERY_MANIFEST.txt` |

Полные ожидаемые и фактические результаты приведены в `EVIDENCE/TEST_RESULTS.md`.

## Ошибки и исправления

| Ошибка | Причина | Исправление | Проверка после исправления |
|---|---|---|---|
| `UX-01`: обрезание нижней кнопки D-pad в 1024×768 | Вертикальная позиция Down была 15 px при высоте кнопки 72 px | Весь D-pad поднят на 35 px; Down установлена на 50 px | PASS, `screenshots/PP13_10_BeforeUserFix.png` и `PP13_11_AfterUserFix.png` |

## Ограничения

- Непроверенные платформы: физические Android/iOS-устройства; отдельная мобильная сборка не создавалась.
- Известные ошибки финальной компиляции: не обнаружены; batch compile завершился кодом 0.
- Не подтверждены непрерывным ручным сценарием: полное прохождение L1–L3, естественные столкновения Player с кристаллом/опасностью/выходом, Restart, потеря фокуса, сохранения после закрытия приложения, быстрые повторные клики Pause.
- Внешний новый пользователь не привлекался.
- Публичный Git URL отсутствует; fresh clone candidate commit подтверждён внутренними логами, а точная итоговая ревизия и состав ZIP подтверждаются пост-коммитным `DELIVERY_MANIFEST.txt` и аннотацией `pp-pm13-release`.
- Word/PDF-отчёты, PowerPoint-презентации, подписи, справка о практике и загрузка в LMS не входят в этот Unity ZIP и требуют ручной подготовки/оформления.
