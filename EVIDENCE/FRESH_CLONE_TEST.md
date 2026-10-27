# Проверка свежего публичного клона

| Поле | Фактическое значение |
|---|---|
| URL | `https://github.com/llecritoire-bot/EnergoKey-Synergy-Practice.git` |
| Дата клонирования и проверки | 27 октября 2026 года |
| Проверенная ревизия | `87a0b2642b7f7d02bcdb8165dccedccf2dc2164f` |
| Ветка | `main` |
| Unity | `6000.6.2f1` |
| Способ клонирования | HTTPS без credential helper, `GIT_TERMINAL_PROMPT=0` |
| Состояние сразу после clone | чистое |
| Итог | PASS |

## Последовательность и результаты

1. Публичный репозиторий клонирован в новую временную папку без использования локальной `Library` и без авторизационных данных.
2. Полученный HEAD совпал с опубликованной веткой `main`: `87a0b2642b7f7d02bcdb8165dccedccf2dc2164f`.
3. Unity импортировал проект с нуля; batch compile завершился кодом 0, ошибок C# нет.
4. EditMode: 24/24 Passed, 0 Failed, 0 Skipped.
5. PlayMode: 17/17 Passed, 0 Failed, 0 Skipped.
6. Структурная проверка: 165/165 PASS.
7. Main Menu, переход в Level Select и запуск Gameplay проверены тестом `T03_MainMenuLevelSelectAndGameplayControlsAcceptPointerInput`.
8. Первый уровень, сбор кристаллов, открытый выход и Victory проверены тестом `T06_NaturalTriggerFlowCollectsAll_ExitWins_AndPersistsUnlockAndBestScore`.
9. Pause/Resume и остановка `Time.timeScale` проверены тестом `T11_PauseStopsTimeAndTimerDuringFade_ResumeContinuesBoth`.
10. Полный L1–L3 flow и отсутствие Level 4 проверены тестом `T09AndT15_FullThreeLevelFlowUnlocksSequentially_AndFinalLevelReturnsToMenuWithoutLevelFour`.
11. В итоговых compile, EditMode, PlayMode и validation логах не обнаружены ошибки компиляции или упавшие тесты.

## Доказательства

- `FreshCloneEditModeTests.xml`;
- `FreshClonePlayModeTests.xml`;
- `logs/fresh-clone-compile.log`;
- `logs/fresh-clone-editmode.log`;
- `logs/fresh-clone-playmode.log`;
- `logs/fresh-clone-validation.log`;
- `logs/fresh-clone-preimport-verifier.log`.

Локальные пути, имя пользователя, hostname, локальные IP и Unity runtime IDs в приложенных текстовых результатах обезличены без изменения статусов и числовых результатов.
