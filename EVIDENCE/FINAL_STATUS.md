# Финальный статус по чек-листу приёмки

Дата сводки: 27 октября 2026 года. Чистая копия candidate commit `91bff8f861632c1846e512aa57306d21d4e9d3f1` проверена успешно. Окончательная tagged revision и итоговый ZIP проверяются пост-коммитно; их собственное доказательство — аннотация `pp-pm13-release` и `DELIVERY_MANIFEST.txt` внутри архива.

Правило оценки: `PASS` ставится только при наличии фактического доказательства; `NOT VERIFIED` означает, что реализация может присутствовать, но полный требуемый сценарий не выполнен/не записан. Наблюдаемых неисправленных `FAIL` в финальной компиляции не обнаружено.

## УП.ПМ.12

| Пункт чек-листа | Статус | Основание |
|---|---|---|
| Unity 6.6 / 6000.6.2f1, 2D | PASS | `ProjectVersion.txt`; `EditorSettings.asset` содержит 2D default behavior; подключён `com.unity.2d.sprite`; batch compile PASS; реальный Inspector: `UP13_02_ImportedSpriteInspector.png`. |
| Игрок двигается экранными кнопками | NOT VERIFIED | Кнопки и Player показаны, но полный фактический протокол движения во всех направлениях не записан. |
| Есть собираемые объекты | NOT VERIFIED | Объекты и prefab подтверждены; естественный сбор касанием с записью до/после не выполнен. |
| Есть опасности | NOT VERIFIED | Опасности и Defeat показаны; последовательность одного столкновения, минус жизни и возврата к старту не записана. |
| Есть особое правило варианта | PASS | Закрытый выход и фактическое сообщение при 0/3: `UP12_09_SpecialRule.png`. |
| Есть однозначные победа/поражение | PASS | Реальные Game View-кадры `UP12_10_Victory.png`, `UP12_11_Defeat.png`. |
| Есть повторный запуск | NOT VERIFIED | Кадр после подготовки начального состояния есть, но нажатие Restart и сохранность общего прогресса не записаны одной проверкой. |
| Получены фактические скриншоты проекта и Editor | PASS | Все 12 имён УП.ПМ.12 присутствуют в `EVIDENCE/screenshots`. |

## ПП.ПМ.12

| Пункт чек-листа | Статус | Основание |
|---|---|---|
| 3 уровня с ростом сложности | PASS | Шесть сцен валидированы Unity; правила 3/4/5 кристаллов и 2/4/6 опасностей PASS; есть три Game View-кадра. |
| Main Menu | PASS | `UP13_09_MainMenu.png`. |
| Level Select | PASS | `PP12_04_MainMenu_LevelSelect.png`. |
| Gameplay | PASS | `PP12_01_Level1.png`–`PP12_03_Level3.png`. |
| Pause | PASS | `PP12_05_Pause.png`; это подтверждает экран, но не весь Resume-сценарий. |
| Victory | PASS | `PP12_06_Victory.png`. |
| Defeat | PASS | `PP12_07_Defeat.png`. |
| Settings | PASS | `PP12_12_Settings.png`. |
| Level 2 открывается после победы L1 | PASS | `RegisterVictory_UnlocksNextLevelAndStoresBestScore` — Passed. |
| Level 3 открывается после победы L2 | PASS | Та же проверка SaveService для последовательного уровня; лимит правил равен 3. |
| После L3 нет перехода в L4 | PASS | `RegisterVictory_OnFinalLevelNeverCreatesLevelFour` — Passed. |
| Сохраняются unlocked level / best score / volume | NOT VERIFIED | Автоматизированные SaveService-тесты PASS, но полный перезапуск приложения со значениями до/после не записан. |
| Новая попытка сбрасывает временные данные | NOT VERIFIED | Реализация есть; единый сценарий Restart с проверкой временных и общих данных не записан. |
| Потеря фокуса ставит игру на паузу | NOT VERIFIED | Фактическое переключение приложения не проводилось. |
| Выполнены Т01–Т16 с фактическими результатами | PASS | Все 16 строк заполнены в `TEST_RESULTS.md` фактическим наблюдением или честным `NOT VERIFIED`; 5 PASS, 11 NOT VERIFIED. |
| Есть Profiler ДО/ПОСЛЕ реального изменения | PASS | Реальные кадры: выбранный одиночный CPU frame 3.63 ms до и 1.19 ms после; без статистических заявлений. |
| Репозиторий содержит Assets+.meta, Packages, ProjectSettings, README, .gitignore | PASS | Состав присутствует; Unity structural validation 165/165 PASS, включая две проверки 2D-конфигурации. |
| В Git нет Library/Temp/Logs/obj/UserSettings/Build | PASS | `git ls-files` не выдаёт файлов из этих корневых каталогов; список итогового ZIP повторно проверяется пост-коммитно и отражается в `DELIVERY_MANIFEST.txt`. |

## УП.ПМ.13

| Пункт чек-листа | Статус | Основание |
|---|---|---|
| Единый визуальный стиль | PASS | Финальный тёмный стиль показан в Game View и Release. |
| Импортированы минимум 3 графических ресурса | PASS | 16 финальных PNG импортированы как Sprite; structural validation PASS; настройки видны в `UP13_02_ImportedSpriteInspector.png`. |
| Есть собственная палитра цветов | PASS | Голубой Player, жёлтые кристаллы, красные опасности, зелёный выход, нейтральные панели. |
| Player/Collectible/Hazard/UI Button сохранены как prefabs | PASS | 4 prefab валидированы по компонентам и показаны в `UP13_04_PrefabsFolder.png`; `UP13_03_ObjectInspector.png` показывает Hazard Inspector. |
| Canvas Scaler настроен | PASS | `UP13_05_CanvasScaler.png`: Scale With Screen Size, 1280×720, Match 0.5. |
| Anchors/RectTransform настроены | PASS | `UP13_06_RectTransformAnchors.png`, Safe Area adapter. |
| HUD читаемый | PASS | `UP13_07_ReadyHUD.png`, фактические кадры трёх форматов. |
| Есть визуальная и звуковая обратная связь | PASS | Locked/Victory/Defeat отображены; 6 WAV импортированы и валидированы Unity. |
| Есть главное меню, пауза, победа, поражение | PASS | `UP13_09_MainMenu.png`–`UP13_12_Defeat.png`. |
| Источники графики и звука документированы | PASS | `Release/Docs/CREDITS.md`: ресурсы созданы для учебного проекта. |

## ПП.ПМ.13

| Пункт чек-листа | Статус | Основание |
|---|---|---|
| 16:9 проверено | PASS | Gameplay в 1280×720: `PP13_02_16x9.png`. |
| 18:9 проверено | PASS | Gameplay в 1440×720: `PP13_03_18x9.png`. |
| 4:3 проверено | PASS | Gameplay в 1024×768: `PP13_04_4x3.png`; D-pad исправлен. |
| Safe Area проверена/имитирована | PASS | `PP13_05_SafeAreaSimulation.png`. Физическое устройство не проверялось. |
| 3 карточки обучения | PASS | `TUTORIAL_01_Controls.png`, `TUTORIAL_02_SpecialRule.png`, `TUTORIAL_03_Completion.png`. |
| Пропуск обучения | NOT VERIFIED | Кнопка реализована, но полный UI-сценарий Skip → новый Play не записан. |
| Повторное открытие «Как играть» | NOT VERIFIED | Кнопка и карточка 1 показаны отдельно; переход именно нажатием кнопки не записан. |
| tutorialSeen сохраняется | PASS | `MarkTutorialSeen_PersistsFlag` — Passed. |
| Плавная Pause panel | NOT VERIFIED | Состояние панели показано; плавность и блокировка во время анимации не подтверждены видеозаписью/логом. |
| Скрытые панели не перехватывают ввод | NOT VERIFIED | Код использует CanvasGroup, но фактическая интерактивная проверка каждой скрытой панели не записана. |
| Экран «Об игре» | PASS | `PP13_09_AboutScreen.png`. |
| Version совпадает в About/Player Settings/README | PASS | 1.0.0, structural validation PASS. |
| icon 512×512 | PASS | `Release/icon_512.png`, размер проверен. |
| promo 1280×720 | PASS | `Release/promo_1280x720.png`, размер проверен. |
| >= 6 игровых кадров | PASS | 6 PNG в `Release/Screenshots`. |
| краткое описание <=80 знаков | PASS | 73 символа по structural validation. |
| полное описание 600–1200 знаков | PASS | 988 символов по structural validation. |
| PLAYER_GUIDE | PASS | `Release/Docs/PLAYER_GUIDE.md`. |
| CREDITS | PASS | `Release/Docs/CREDITS.md`. |
| CHANGELOG | PASS | `Release/Docs/CHANGELOG.md`. |
| product page mockup | PASS | `Release/product_page_mockup.png`, 1440×1800. |
| storyboard 6–8 кадров или видео 60–90 сек. | PASS | `Release/storyboard.md`: 7 кадров, 80 секунд. Видео не создавалось и не требуется при наличии storyboard. |
| Пользовательская/самостоятельная проверка описана честно | PASS | `USER_TEST.md`, реальная проблема D-pad до/после; внешний участник не выдуман. |
| Т01–Т16 для ПП.ПМ.13 заполнены по фактическим запускам | PASS | Все 16 строк заполнены; 5 PASS, 11 NOT VERIFIED. |
| Новая копия репозитория запускается | PASS | Чистый клон candidate commit `91bff8f861632c1846e512aa57306d21d4e9d3f1`: compile exit 0, EditMode 24/24, validation 165/165; `FRESH_CLONE_VERIFICATION.md`. |
| Финальный commit hash записан | PASS | Полный hash однозначно разрешается через аннотированный тег командой `git rev-list -n 1 pp-pm13-release` и дублируется в `DELIVERY_MANIFEST.txt` итогового ZIP; literal hash намеренно не встраивается в сам tagged commit. |

## Требования оригинальных документов вне Unity ZIP

| Пункт | Статус | Пояснение |
|---|---|---|
| Доступный преподавателю Git-репозиторий | NOT VERIFIED | Есть локальный Git; публичный URL отсутствует. |
| PDF/Word-отчёты по практикам | NOT VERIFIED | Пользователь запросил проект и Evidence для их последующей подготовки. |
| PowerPoint-презентации по шаблонам | NOT VERIFIED | Исходные PPTX изучены; заполненные презентации не входят в текущую поставку. |
| Подписи, справка, печать, персональные поля | NOT VERIFIED | Требуют данных студента и участия ответственного лица. |
| Размещение в LMS «Синергия» | NOT VERIFIED | Внешняя подача не выполнялась. |

## Итог

- Финальная компиляция Unity: PASS.
- EditMode: 24/24 PASS.
- Структурная проверка: 165/165 PASS, включая 2D Sprite package и 2D default behavior.
- Fresh clone candidate commit: PASS; compile 0, EditMode 24/24, validation 165/165.
- Наблюдаемых неисправленных игровых или компиляционных FAIL: 0. Candidate verifier был запущен до release-тега, поэтому его единственный delivery FAIL исторически относится только к отсутствовавшему тогда тегу.
- Главные пробелы: непрерывное ручное прохождение, реальный перезапуск сохранений, потеря фокуса, полный UI-тест во всех форматах, физическое устройство, внешний новый пользователь и административная отчётность.
