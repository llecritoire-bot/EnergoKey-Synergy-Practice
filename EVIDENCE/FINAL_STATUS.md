# Финальный статус по чек-листу приёмки

Дата сводки: 27 октября 2026 года.

Правило оценки: `PASS` ставится только при фактическом доказательстве; `NOT VERIFIED` означает отсутствие полного подтверждения. Пользовательское сообщение выделяется как `PASS (USER-REPORTED)` и не подменяет новый пользовательский тест или Android/iOS.

## УП.ПМ.12

| Пункт чек-листа | Статус | Основание |
|---|---|---|
| Unity 6.6 / 6000.6.2f1, 2D | PASS | `ProjectVersion.txt`; 2D default behavior и `com.unity.2d.sprite`; compile PASS. |
| Игрок двигается экранными кнопками | PASS | PlayMode `T01_ScreenButtonsMoveInFourDirections_AndBoundaryWallBlocksPlayer`. |
| Есть собираемые объекты | PASS | PlayMode `T02_PhysicsContactCollectsOnce_DisablesCrystal_AndUpdatesHudAndScore`. |
| Есть опасности | PASS | PlayMode `T04_HazardContactRemovesOneLife_AndRespawnsAtStart`. |
| Есть особое правило варианта | PASS | Закрытый выход и сообщение при 0/3: `UP12_09_SpecialRule.png`. |
| Есть однозначные победа/поражение | PASS | Runtime-тесты и кадры `UP12_10_Victory.png`, `UP12_11_Defeat.png`. |
| Есть повторный запуск | PASS | PlayMode `T05_DefeatAndRestartResetRun_ButPreservePersistentProgress`. |
| Получены фактические скриншоты проекта и Editor | PASS | Все 12 обязательных имён УП.ПМ.12 присутствуют. |

## ПП.ПМ.12

| Пункт чек-листа | Статус | Основание |
|---|---|---|
| 3 уровня с ростом сложности | PASS | Правила 3/4/5 кристаллов и 2/4/6 опасностей валидированы; полный L1–L3 PlayMode-flow прошёл. |
| Main Menu | PASS | `UP13_09_MainMenu.png`, responsive runtime-render. |
| Level Select | PASS | `PP12_04_MainMenu_LevelSelect.png`, responsive runtime-render. |
| Gameplay | PASS | Кадры трёх уровней и PlayMode physics tests. |
| Pause | PASS | PlayMode подтвердил `timeScale=0`, остановку таймера, fade и Resume. |
| Victory | PASS | Естественный trigger-flow и `PP12_06_Victory.png`. |
| Defeat | PASS | Damage/Defeat/Restart PlayMode-flow и `PP12_07_Defeat.png`. |
| Settings | PASS | Pointer/UI PlayMode-test и runtime-render. |
| Level 2 открывается после победы L1 | PASS | Полный PlayMode-flow и SaveService tests. |
| Level 3 открывается после победы L2 | PASS | Полный PlayMode-flow и SaveService tests. |
| После L3 нет перехода в L4 | PASS | `T09AndT15_FullThreeLevelFlowUnlocksSequentially_AndFinalLevelReturnsToMenuWithoutLevelFour`. |
| Сохраняются unlocked level / best score / volume | PASS | Независимые Unity-процессы WRITE/READ: `PERSISTENCE_RESTART_PROBE.md`; уникальные runtime-renders 3/3+650 и 37%. |
| Новая попытка сбрасывает временные данные | PASS | PlayMode Restart сбросил попытку и сохранил постоянный прогресс. |
| Потеря фокуса ставит игру на паузу | PASS | PlayMode проверил оба callback; отдельная macOS Standalone-сборка при реальном системном сворачивании зарегистрировала `hasFocus=False`/`paused=True`, открыла Pause и остановила таймер (`FOCUS_LOSS_TEST.md`). |
| Выполнены Т01–Т16 с фактическими результатами | PASS | `TEST_RESULTS.md`: 16 PASS, 0 FAIL, 0 NOT VERIFIED. |
| Есть Profiler ДО/ПОСЛЕ реального изменения | PASS | Реальные GUI-кадры плюс повторный ProfilerRecorder benchmark: полный HUD-marker 600/600 кадров до и 5/600 после, примерно 31,96× по суммарному времени marker: `PROFILER_BENCHMARK.md`. |
| Репозиторий содержит Assets+.meta, Packages, ProjectSettings, README, .gitignore | PASS | Structural validation 165/165. |
| В Git нет Library/Temp/Logs/obj/UserSettings/Build | PASS | Проверка tracked-файлов и состава архива. |

## УП.ПМ.13

| Пункт чек-листа | Статус | Основание |
|---|---|---|
| Единый визуальный стиль | PASS | Финальный тёмный стиль показан в Game View и Release. |
| Импортированы минимум 3 графических ресурса | PASS | 16 финальных PNG импортированы как Sprite; validation PASS. |
| Есть собственная палитра цветов | PASS | Голубой Player, жёлтые кристаллы, красные опасности, зелёный выход, нейтральные панели. |
| Player/Collectible/Hazard/UI Button сохранены как prefabs | PASS | Четыре prefab валидированы и показаны в `UP13_04_PrefabsFolder.png`. |
| Canvas Scaler настроен | PASS | `UP13_05_CanvasScaler.png`: 1280×720, Match 0.5. |
| Anchors/RectTransform настроены | PASS | `UP13_06_RectTransformAnchors.png`; 21 render прошёл bounds assertions. |
| HUD читаемый | PASS | `UP13_07_ReadyHUD.png` и responsive renders. |
| Есть визуальная и звуковая обратная связь | PASS | Locked/Victory/Defeat показаны; шесть WAV импортированы и валидированы. |
| Есть главное меню, пауза, победа, поражение | PASS | Runtime UI/Gameplay tests и renders. |
| Источники графики и звука документированы | PASS | `Release/Docs/CREDITS.md`. |

## ПП.ПМ.13

| Пункт чек-листа | Статус | Основание |
|---|---|---|
| 16:9 проверено | PASS | Семь состояний в 1280×720: `responsive/RESP_1280x720_*.png`. |
| 18:9 проверено | PASS | Семь состояний в 1440×720: `responsive/RESP_1440x720_*.png`. |
| 4:3 проверено | PASS | Семь состояний в 1024×768: `responsive/RESP_1024x768_*.png`; overlap Level Select исправлен. |
| Safe Area проверена/имитирована | PASS | `PP13_05_SafeAreaSimulation.png`; visible-control bounds assertions для 21 render. |
| 3 карточки обучения | PASS | Кадры и PlayMode navigation 1/3 → 2/3 → 3/3. |
| Пропуск обучения | PASS | PlayMode Skip сохранил флаг и прогресс, после reload автопоказа не было. |
| Повторное открытие «Как играть» | PASS | PlayMode открыл карточку 1 после ранее просмотренного обучения. |
| tutorialSeen сохраняется | PASS | EditMode, PlayMode и двухпроцессный PlayerPrefs probe. |
| Плавная Pause panel | PASS | PlayMode проверил fade, `timeScale=0`, блокировку перехода и Resume. |
| Скрытые панели не перехватывают ввод | PASS | PlayMode raycast assertions и рабочая underlying D-pad. |
| Экран «Об игре» | PASS | `PP13_09_AboutScreen.png`. |
| Version совпадает в About/Player Settings/README | PASS | 1.0.0; standalone runtime marker также сообщил 1.0.0. |
| icon 512×512 | PASS | `Release/icon_512.png`. |
| promo 1280×720 | PASS | `Release/promo_1280x720.png`. |
| >= 6 игровых кадров | PASS | Шесть PNG в `Release/Screenshots`. |
| краткое описание <=80 знаков | PASS | 73 символа. |
| полное описание 600–1200 знаков | PASS | 988 символов. |
| PLAYER_GUIDE | PASS | `Release/Docs/PLAYER_GUIDE.md`. |
| CREDITS | PASS | `Release/Docs/CREDITS.md`. |
| CHANGELOG | PASS | `Release/Docs/CHANGELOG.md`. |
| product page mockup | PASS | `Release/product_page_mockup.png`, 1440×1800. |
| storyboard 6–8 кадров или видео 60–90 сек. | PASS | `Release/storyboard.md`: семь кадров, 80 секунд. |
| Пользовательская/самостоятельная проверка описана честно | PASS | `USER_TEST.md`, `USER_ACCEPTANCE.md`; user-reported PASS на MacBook M3, без заявления о новом пользователе. |
| Т01–Т16 для ПП.ПМ.13 заполнены по фактическим запускам | PASS | `TEST_RESULTS.md`: 15 PASS, 0 FAIL, 1 NOT VERIFIED (Т14 — новый пользователь). |
| Новая копия репозитория запускается | PASS | Exact clone тега `pp-pm13-release` проверяется после неизменяемого тега; compile, EditMode, PlayMode, validation и verifier фиксируются в `POST_COMMIT_VERIFICATION` итогового ZIP. |
| Финальный commit hash записан | PASS | Полный hash разрешается аннотированным тегом `pp-pm13-release` и записывается пост-коммитно в `DELIVERY_MANIFEST.txt` итогового ZIP. |

## Дополнительные фактические проверки

| Проверка | Статус | Основание |
|---|---|---|
| macOS Standalone на физическом MacBook Apple M3 | PASS | Development Build: 0 ошибок/0 предупреждений; фактический старт и реальная проверка системного сворачивания: `MAC_STANDALONE_TEST.md`, `FOCUS_LOSS_TEST.md`. |
| Пользовательская приёмка на MacBook M3 | PASS (USER-REPORTED) | Дословное сообщение: `USER_ACCEPTANCE.md`. |
| Физическое мобильное Android/iOS-устройство | NOT VERIFIED | macOS MacBook не является мобильным устройством; мобильная сборка не создавалась. |
| Новый внешний пользователь | NOT VERIFIED | Пользователь-заказчик заранее знаком с ТЗ и не считается новым пользователем. |
| Реальное системное сворачивание macOS | PASS | Runtime-маркеры `hasFocus=False` и `paused=True`, снимок панели Pause после возврата: `FOCUS_LOSS_TEST.md`, `PP12_13_FocusLossAutoPause.png`. |

## Требования оригинальных документов вне Unity ZIP

| Пункт | Статус | Пояснение |
|---|---|---|
| Доступный преподавателю Git-репозиторий | NOT VERIFIED | Есть локальный Git; публичный URL отсутствует. |
| PDF/Word-отчёты и PowerPoint-презентации | NOT VERIFIED | Unity ZIP подготовлен как фактическая основа для последующего оформления. |
| Подписи, справка, печать, персональные поля | NOT VERIFIED | Требуют данных студента и участия ответственного лица. |
| Размещение в LMS «Синергия» | NOT VERIFIED | Внешняя подача не выполнялась. |

## Итог

- Editor compile: PASS.
- EditMode: 24/24 PASS.
- PlayMode: 17/17 PASS.
- Structural validation: 165/165 PASS.
- Двухпроцессное сохранение PlayerPrefs: PASS.
- 21 runtime-render (семь состояний × три формата) и Safe Area/bounds: PASS.
- macOS Standalone build/smoke: PASS на физическом MacBook Apple M3.
- ProfilerRecorder до/после: PASS.
- ПП.ПМ.12 Т01–Т16: 16 PASS, 0 FAIL, 0 NOT VERIFIED.
- ПП.ПМ.13 Т01–Т16: 15 PASS, 0 FAIL, 1 NOT VERIFIED.
- Наблюдаемых неисправленных игровых или компиляционных FAIL: 0.
- Остаются честно неподтверждёнными: новый внешний пользователь, Android/iOS и внешние административные действия.
