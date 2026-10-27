# Манифест поставки «Энергоключ»

Состояние файлов зафиксировано 27 октября 2026 года. Этот документ описывает содержимое Unity-проекта и приложенные доказательства без привязки к промежуточным commit hash или изменяемым контрольным суммам.

## Unity-проект

| Раздел | Фактический состав |
|---|---:|
| Сцены `Assets/Scenes/*.unity` | 6 |
| Prefab `Assets/Resources/Prefabs/*.prefab` | 4 |
| C#-файлы в `Assets` | 30 |
| Файлы `.meta` в `Assets` | 95 |
| Финальные PNG в `Assets/Resources/Art/Final` | 16 |
| WAV в `Assets/Resources/Audio` | 6 |

Сцены: `MainMenu`, `LevelSelect`, `Level1`, `Level2`, `Level3`, `Prototype`.

Prefab: `Player`, `Collectible`, `Hazard`, `UIButton`.

Обязательные корневые элементы присутствуют: `Assets`, `Packages`, `ProjectSettings`, `.gitignore`, `README.md`, `Release` и `EVIDENCE`. Вместе с файлами проекта сохранены соответствующие Unity `.meta`.

## Автоматизированные проверки

| Доказательство | Фактический результат |
|---|---|
| `EditModeTests-final.xml` | 24/24 Passed, 0 Failed, 0 Skipped |
| `PlayModeTests-final.xml` | 17/17 Passed, 0 Failed, 0 Skipped |
| `PROJECT_VALIDATION.md` и `.json` | 165/165 structural checks Passed |
| `PERSISTENCE_RESTART_PROBE.md` и `.json` | Два отдельных Unity-процесса WRITE/READ; level 3, score 650, volume 0.37 и `tutorialSeen=true` восстановлены |
| `PROFILER_BENCHMARK.md` и два JSON | Сопоставимый ProfilerRecorder benchmark HUD до/после |
| `ProfilerBenchmarkBeforeTests.xml`, `ProfilerBenchmarkAfterTests.xml` | Машинные результаты повторного benchmark-прогона |
| `MAC_STANDALONE_TEST.md` | macOS Standalone Development Build собран и запущен |
| `FOCUS_LOSS_TEST.md` | Реальное системное сворачивание macOS Standalone вызвало автоматическую Pause |

Связанные журналы находятся в `EVIDENCE/logs`, включая итоговую компиляцию, EditMode, PlayMode, structural validation, macOS build/runtime и focus-loss. Для Profiler сохранены JSON, NUnit XML, воспроизводимый harness и снимки до/после; временные полные логи удалены из сдаваемой версии.

## Скриншоты и responsive renders

В `EVIDENCE/screenshots` находятся 58 PNG:

- 54 обязательных имени из `Documentation/03_SCREENSHOT_MATRIX.md`;
- 3 дополнительные карточки обучения `TUTORIAL_01`–`TUTORIAL_03`;
- 1 дополнительный кадр реальной автоматической паузы `PP12_13_FocusLossAutoPause.png`.

`PP12_08_SavedProgressAfterRestart.png` и `PP12_09_SavedVolume.png` созданы как отдельные Unity runtime-renders. Они отличаются друг от друга и от сравнительных кадров `PP12_04_MainMenu_LevelSelect.png` и `PP12_12_Settings.png`.

В `EVIDENCE/responsive` находятся 22 PNG:

- 21 итоговый render: семь состояний × `1280×720`, `1440×720`, `1024×768`;
- `BEFORE_1024x768_LevelSelect_overlap.png` — состояние до исправления overlap.

Итоговые responsive renders получены в Unity PlayMode с включённой Editor-имитацией Safe Area 6% / 6% / 88% / 88%. Проверены границы видимых элементов и отсутствие overlap Level Select.

## Release

В `Release` находятся 9 PNG:

- `icon_512.png` — 512×512;
- `promo_1280x720.png` — 1280×720;
- `product_page_mockup.png` — макет страницы продукта;
- `Screenshots/STORE_01`–`STORE_06` — шесть игровых кадров.

Документы комплекта:

- `Docs/DESCRIPTION.md`;
- `Docs/PLAYER_GUIDE.md`;
- `Docs/CREDITS.md`;
- `Docs/CHANGELOG.md`;
- `storyboard.md` — семь кадров, 80 секунд;
- `README.md`.

## Сводные Evidence-файлы

- `REPORT_DATA.md` — фактические данные для последующих отчётов;
- `TEST_RESULTS.md` — результаты Т01–Т16;
- `USER_TEST.md` и `USER_ACCEPTANCE.md` — протокол реальной проверки новым пользователем;
- `FINAL_STATUS.md` — сверка с чек-листом;
- `MISSING_EVIDENCE.md` — честно обозначенные внешние ограничения.

MacBook с Apple M3 зафиксирован как физический компьютер macOS, а не как мобильное Android/iOS-устройство. Устройство внешнего пользовательского сеанса отдельно не фиксировалось.

## Git и итоговая упаковка

История содержит этапные теги:

- `up-pm12-prototype`;
- `pp-pm12-three-levels`;
- `pm13-before-visual-style`;
- `up-pm13-styled`;
- `profiler-before`;
- `pp-pm13-release`.

Публичный репозиторий, основная ветка, проверенная ревизия и опубликованные теги указаны в `REPOSITORY_URL.md`. Результат новой чистой копии фиксируется в `FRESH_CLONE_TEST.md`; состав итогового архива — в `ARCHIVE_CONTENTS.md` и внешнем файле SHA-256.

В итоговый ZIP не должны входить `Library`, `Temp`, `Logs`, `obj`, `UserSettings`, `Build` и `Builds`.
