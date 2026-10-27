# Результаты проверок проекта «Энергоключ»

Дата проверки: 27 октября 2026 года. Основная среда: Unity Editor 6000.6.2f1, macOS 27.0 (26A428), Apple M3. Дополнительно собрана и запущена macOS Standalone Development Build.

Статусы:

- `PASS` — требование подтверждено фактическим запуском, автоматизированным тестом либо однозначным артефактом Unity;
- `FAIL` — проверка выполнена и получила результат, противоречащий ожидаемому;
- `NOT VERIFIED` — полного фактического подтверждения нет.

Пользовательская проверка проведена одним новым внешним пользователем, ранее не знакомым с игрой. Устройство сеанса отдельно не фиксировалось; эта проверка не выдаётся за испытание Android/iOS.

## Контрольные прогоны

| Проверка | Фактический результат | Статус | Доказательство |
|---|---|---|---|
| Компиляция Editor-проекта | Batch-прогон завершён с `*** Tundra build success`, код 0; ошибок C# нет. | PASS | `logs/final-compile.log` |
| EditMode | 24 Passed, 0 Failed, 0 Skipped. | PASS | `EditModeTests-final.xml`, `logs/editmode-tests-final.log` |
| PlayMode integration | 17 Passed, 0 Failed, 0 Skipped; проверены физические контакты, управление, Restart, три уровня, UI, обучение, Pause, уникальные runtime-render сохранений и callback-логика потери фокуса. | PASS | `PlayModeTests-final.xml`, `logs/playmode-tests-final.log` |
| Сохранения между процессами | Отдельный Unity-процесс WRITE завершился до запуска READ; повторно прочитаны level 3, score 650, volume 0.37 и `tutorialSeen=true`. | PASS | `PERSISTENCE_RESTART_PROBE.md`, `PERSISTENCE_RESTART_PROBE.json` |
| Адаптивность | Unity сформировал 21 runtime-кадр: семь обязательных состояний × три разрешения; проверки границ, Safe Area и отсутствия overlap прошли. | PASS | `responsive/RESP_*.png`, `PlayModeTests-final.xml` |
| macOS Standalone | Development Build: 0 ошибок, 0 предупреждений; собранное приложение запущено, получен runtime-маркер MainMenu/version 1.0.0. | PASS | `MAC_STANDALONE_TEST.md`, `logs/mac-standalone-build.log`, `logs/mac-standalone-smoke.log` |
| ProfilerRecorder HUD | Одинаковый сценарий 600 кадров до/после: полное обновление HUD вызывалось 600 раз до и 5 раз после; 1 845 755 ns против 57 749 ns, оба NUnit-прогона 1/1 PASS. | PASS | `PROFILER_BENCHMARK.md`, два JSON, `ProfilerBenchmarkBeforeTests.xml`, `ProfilerBenchmarkAfterTests.xml` |
| Структурная проверка Unity | 165/165 PASS, включая пакет 2D Sprite и 2D default behavior. | PASS | `PROJECT_VALIDATION.md`, `PROJECT_VALIDATION.json`, `logs/final-validation.log` |
| Свежая копия публичного репозитория | Проверяется после первой публикации итоговой ветки; фактический hash и результаты фиксируются в отдельном протоколе. | NOT VERIFIED | `FRESH_CLONE_TEST.md` |
| Пользовательская проверка | Новый пользователь без устного объяснения освоил управление, понял особое правило, завершил Level 1 и проверил Pause/Resume. | PASS | `USER_ACCEPTANCE.md`, `USER_TEST.md` |

## ПП.ПМ.12 — программирование мобильных игр

Сценарии ниже составлены по техническому заданию проекта и матрице доказательств.

| ID | Проверка | Ожидаемый результат | Фактическое наблюдение | Статус | Доказательство |
|---|---|---|---|---|---|
| Т01 | Управление игроком | Экранные кнопки двигают игрока в четырёх направлениях; стены непроходимы. | PlayMode-тест удерживал четыре экранные кнопки, проверил движение и блокировку левой граничной стеной. | PASS | `PlayModeTests-final.xml`: `T01_ScreenButtonsMoveInFourDirections_AndBoundaryWallBlocksPlayer` |
| Т02 | Сбор кристалла и HUD | Кристалл засчитывается один раз, исчезает, HUD и счёт обновляются. | Физический trigger-contact собрал объект один раз, отключил его и обновил HUD/счёт. | PASS | `PlayModeTests-final.xml`: `T02_PhysicsContactCollectsOnce_DisablesCrystal_AndUpdatesHudAndScore` |
| Т03 | Закрытый выход | До сбора всех кристаллов выход не завершает уровень и даёт обратную связь. | В Level 1 при 0/3 показаны закрытый выход и сообщение «Выход заблокирован. Осталось кристаллов: 3». | PASS | `screenshots/UP12_09_SpecialRule.png` |
| Т04 | Опасность, жизнь и возврат | Опасность отнимает одну жизнь и возвращает игрока на старт. | Физический контакт с Hazard уменьшил жизни на одну и вернул Player в стартовую точку. | PASS | `PlayModeTests-final.xml`: `T04_HazardContactRemovesOneLife_AndRespawnsAtStart` |
| Т05 | Поражение и Restart | При 0 жизней открывается Defeat; Restart создаёт новую попытку с 3 жизнями, сохраняя общий прогресс. | PlayMode-сценарий довёл жизни до нуля, выполнил Restart и проверил сброс попытки при сохранении постоянных данных. | PASS | `PlayModeTests-final.xml`: `T05_DefeatAndRestartResetRun_ButPreservePersistentProgress` |
| Т06 | Победа, счёт и открытие уровня | Все кристаллы + выход дают Victory, лучший счёт и следующий уровень. | Естественный trigger-flow собрал все кристаллы, вошёл в выход, получил Victory и сохранил unlock/best score. | PASS | `PlayModeTests-final.xml`: `T06_NaturalTriggerFlowCollectsAll_ExitWins_AndPersistsUnlockAndBestScore` |
| Т07 | Параметры уровня 1 | 3 кристалла, 2 опасности, простой маршрут. | Unity validation получил 3 кристалла и 2 опасности; Level 1 показан в Game View. | PASS | `PROJECT_VALIDATION.md`, `screenshots/PP12_01_Level1.png` |
| Т08 | Уровень 2 и блокировка | 4 кристалла, 4 опасности; доступ только после победы L1. | Значения 4/4 валидированы; последовательное открытие проверено полным PlayMode-проходом и EditMode-тестом SaveService. | PASS | `PROJECT_VALIDATION.md`, `PlayModeTests-final.xml`, `EditModeTests-final.xml` |
| Т09 | Уровень 3 и финал | 5 кристаллов, 6 опасностей; после победы нет L4. | Полный PlayMode-flow последовательно прошёл L1–L3 и вернулся в меню без создания уровня 4. | PASS | `PlayModeTests-final.xml`: `T09AndT15_FullThreeLevelFlowUnlocksSequentially_AndFinalLevelReturnsToMenuWithoutLevelFour` |
| Т10 | Экраны и переходы | Main Menu, Level Select, Gameplay, Pause, Victory, Defeat и Settings открываются по назначению. | Все семь состояний построены реальным runtime-кодом, пройдены UI/Gameplay-тестами и отрендерены Unity. | PASS | `PlayModeTests-final.xml`, `responsive/RESP_*.png` |
| Т11 | Ручная пауза | `Time.timeScale = 0`; таймер остановлен; Resume продолжает игру. | Во время fade `timeScale` и таймер оставались остановлены; после Resume оба продолжили работу. | PASS | `PlayModeTests-final.xml`: `T11_PauseStopsTimeAndTimerDuringFade_ResumeContinuesBoth` |
| Т12 | Прогресс и рекорд между запусками | Максимальный уровень и лучший результат сохраняются после нового запуска. | Независимые процессы WRITE/READ повторно получили level 3 и score 650; runtime-кадр сохранённого прогресса создан отдельно. | PASS | `PERSISTENCE_RESTART_PROBE.md`, `screenshots/PP12_08_SavedProgressAfterRestart.png` |
| Т13 | Громкость между запусками | Громкость применяется и сохраняется после закрытия/повторного запуска. | Независимый READ-процесс получил volume 0.37; Settings runtime-render показывает 37%. | PASS | `PERSISTENCE_RESTART_PROBE.md`, `screenshots/PP12_09_SavedVolume.png` |
| Т14 | Потеря фокуса | Callback потери фокуса/паузы автоматически ставит активную игру на паузу. | PlayMode подтвердил оба callback. Дополнительно реальное системное сворачивание macOS Standalone зарегистрировало `hasFocus=False`/`paused=True`; после возврата видна Pause, таймер остановлен. | PASS | `PlayModeTests-final.xml`: `T14_FocusAndApplicationPauseCallbacksAutomaticallyPauseActiveGameplay`; `FOCUS_LOSS_TEST.md`; `screenshots/PP12_13_FocusLossAutoPause.png` |
| Т15 | Git и новая копия | Состав и история корректны; новая копия запускается. | Финальная проверка свежего клона выполняется после публикации ветки `main`. | NOT VERIFIED | `FRESH_CLONE_TEST.md` |
| Т16 | Profiler до/после | Реальное сравнение одного изменения при одинаковых условиях. | Детерминированный ProfilerRecorder microbenchmark ревизий `d0b7433011ef097a5b92ea4799ac92d2887fa5c4` и `11771ef31459a3aad844614287306db68119c8d2` измерил по 600 кадров: marker вызван 600 раз до и 5 раз после; 1 845 755 ns против 57 749 ns (31.96×, 96.87%). | PASS | `PROFILER_BENCHMARK.md`, два JSON, `ProfilerBenchmarkBeforeTests.xml`, `ProfilerBenchmarkAfterTests.xml`, GUI-кадры `PP12_10`/`PP12_11` |

Промежуточный итог до проверки свежего клона: **15 PASS, 0 FAIL, 1 NOT VERIFIED**. Реальная доставка focus/pause callback дополнительно подтверждена системным сворачиванием macOS Standalone.

## ПП.ПМ.13 — оформление игрового продукта

| ID | Проверка | Ожидаемый результат | Фактическое наблюдение | Статус | Доказательство |
|---|---|---|---|---|---|
| Т01 | Адаптивность семи экранов | Семь экранов читаемы в 16:9, 18:9 и 4:3. | Unity создал 21 runtime-кадр (7×3); все видимые controls прошли bounds/Safe Area assertions. | PASS | `responsive/RESP_*.png`, `PlayModeTests-final.xml`: `SevenRequiredStatesRenderAtThreeTargetResolutions` |
| Т02 | Safe Area | Кнопки остаются внутри имитируемой безопасной области. | Safe Area показана отдельным кадром; 21 runtime-render прошёл проверку границ видимых controls. | PASS | `screenshots/PP13_05_SafeAreaSimulation.png`, `PlayModeTests-final.xml` |
| Т03 | Ввод мышью | Обязательные кнопки доступны pointer-вводом. | PlayMode прошёл Main Menu → Level Select → Gameplay, нажал обязательные кнопки и проверил доступность D-pad/Pause. | PASS | `PlayModeTests-final.xml`: `T03_MainMenuLevelSelectAndGameplayControlsAcceptPointerInput` |
| Т04 | Скрытые панели | Скрытые панели не перехватывают нажатия. | Raycast-проверки Main Menu и Gameplay не обнаружили скрытые панели; underlying D-pad остался рабочим. | PASS | `PlayModeTests-final.xml`: `T04_HiddenPanelsDoNotAppearInRaycasts_OrBlockUnderlyingControls` |
| Т05 | Первый запуск обучения | Чистое сохранение открывает карточку 1. | При `tutorialSeen=false` автоматически открылась карточка «Управление», 1/3. | PASS | `PlayModeTests-final.xml`: `T05_CleanSaveAutomaticallyOpensTutorialAtCardOne` |
| Т06 | Три шага обучения | «Далее» проходит три карточки и возвращает меню. | Pointer-click последовательно открыл 1/3, 2/3, 3/3 и закрыл Tutorial в Main Menu. | PASS | `PlayModeTests-final.xml`: `T06_NextButtonTraversesThreeTutorialCards_ThenReturnsToMenu` |
| Т07 | Завершение и пропуск | Skip/Finish сохраняют прогресс; автопоказ больше не появляется. | Skip сохранил `tutorialSeen`, level, score и volume; после reload автопоказа не было. | PASS | `PlayModeTests-final.xml`: `T07_SkipPersistsTutorialFlag_PreservesProgress_AndPreventsNextAutomaticDisplay`; `PERSISTENCE_RESTART_PROBE.md` |
| Т08 | Повторное открытие | «Как играть» открывает карточку 1. | После ранее просмотренного обучения кнопка повторно открыла «Управление», 1/3. | PASS | `PlayModeTests-final.xml`: `T08_HowToPlayReopensTutorialAtCardOne_AfterItWasPreviouslySeen` |
| Т09 | Пауза во время появления | Панель появляется при `Time.timeScale = 0`. | Уже во время fade подтверждены `timeScale=0`, Pause и состояние transition. | PASS | `PlayModeTests-final.xml`: `T09AndT10_PauseIsAlreadyStoppedDuringFade_AndRapidRepeatedCommandsAreIgnored` |
| Т10 | Блокировка повторных кликов | Повторные команды до конца появления игнорируются. | Серия быстрых Toggle/Pause/Resume не изменила состояние до окончания transition; последующее Resume завершилось корректно. | PASS | `PlayModeTests-final.xml`: `T09AndT10_PauseIsAlreadyStoppedDuringFade_AndRapidRepeatedCommandsAreIgnored` |
| Т11 | Согласованность версии | Version совпадает в About, Player Settings и README. | Во всех трёх местах указано 1.0.0; standalone runtime сообщил version 1.0.0. | PASS | `screenshots/PP13_09_AboutScreen.png`, `ProjectSettings/ProjectSettings.asset`, `README.md`, `MAC_STANDALONE_TEST.md` |
| Т12 | Проверка PNG | PNG открываются и отражают состояния игры. | Обязательная матрица PNG полна; дополнительно созданы 21 валидный Unity runtime-render в точных размерах. | PASS | `FILE_MANIFEST.md`, `PROJECT_VALIDATION.md`, `responsive/RESP_*.png` |
| Т13 | Честность наблюдения | Наблюдение, исправление и повторная проверка описаны честно. | UX-01 и UX-02 выявлены при предварительной самостоятельной технической проверке и исправлены до внешнего сеанса. Новый пользователь проверил финальную версию; новых проблем не выявлено. | PASS | `USER_TEST.md`, `USER_ACCEPTANCE.md`, `responsive/BEFORE_1024x768_LevelSelect_overlap.png`, `responsive/RESP_1024x768_LevelSelect.png` |
| Т14 | Понятность правила новым пользователем | Новый пользователь понимает правило без устной подсказки. | Новый пользователь самостоятельно понял необходимость собрать все кристаллы до выхода и завершил Level 1 без устного объяснения. | PASS | `USER_TEST.md`, `USER_ACCEPTANCE.md` |
| Т15 | Полнота готовой игры | Три уровня, Victory, Defeat и прогресс работают. | PlayMode прошёл полный L1–L3 flow, Defeat/Restart и Victory; отдельные процессы подтвердили постоянные значения. | PASS | `PlayModeTests-final.xml`, `PERSISTENCE_RESTART_PROBE.md` |
| Т16 | Новая копия и единый commit | Новая копия запускается; комплект относится к одной опубликованной ревизии. | Финальная проверка выполняется после публикации ветки `main`. | NOT VERIFIED | `FRESH_CLONE_TEST.md`, `REPOSITORY_URL.md` |

Промежуточный итог до проверки свежего клона: **15 PASS, 0 FAIL, 1 NOT VERIFIED**.

## Вывод

Editor compile, 24 EditMode-теста, итоговые 17 PlayMode-тестов, двухпроцессная проверка PlayerPrefs, 21 responsive runtime-render с имитацией Safe Area, macOS Standalone smoke/focus-loss test, внешний пользовательский сценарий и воспроизводимый ProfilerRecorder benchmark завершились успешно. Первый повторный PlayMode-запуск с `-nographics` получил два FAIL только в тестах захвата изображения; повторный запуск с графическим контекстом завершился 17/17 PASS. До публикации остаётся проверить свежий клон. Android/iOS не проверялись.
