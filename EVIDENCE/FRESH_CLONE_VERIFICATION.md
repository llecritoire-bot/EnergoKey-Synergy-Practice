# Проверка свежей копии candidate commit

Дата проверки: 27 октября 2026 года.

## Объект проверки

| Поле | Фактическое значение |
|---|---|
| Candidate commit | `91bff8f861632c1846e512aa57306d21d4e9d3f1` |
| Временный путь клона | `/private/tmp/energokey-candidate.Aa5d3b/EnergoKey` |
| Unity | `6000.6.2f1` (`770e33f6875c`) |
| Среда | macOS 27.0 (26A428), arm64, без Rosetta |
| Статус до запуска Unity | Чистая рабочая копия; `git status --short` вернул 0 строк |
| Общий результат candidate fresh clone | PASS для компиляции, EditMode tests и structural validation |

Проверка относится именно к указанному candidate commit. Она не выдаётся за проверку будущего аннотированного тега `pp-pm13-release` или итогового ZIP.

## Проверки Git и комплекта

Read-only verifier подтвердил HEAD candidate commit и чистое рабочее дерево:

- `git.head`: PASS, `91bff8f861632c1846e512aa57306d21d4e9d3f1`;
- `git.clean_worktree`: PASS;
- итог verifier: `PASS=27`, `FAIL=1`, `NOT VERIFIED=3`;
- единственный `FAIL`: отсутствует будущий тег `pp-pm13-release`;
- `NOT VERIFIED`: автоматический verifier не доказывает визуальную подлинность скриншотов, runtime-версию About и соответствие README текущему candidate HEAD.

Последние три пункта описывают границы автоматического verifier. Реальный кадр About и визуальный набор проверяются отдельными Evidence-артефактами, но результат самого verifier здесь не повышается задним числом.

Доказательство: `EVIDENCE/logs/fresh-clone-candidate-verifier.log`.

Проверочные команды для Git/verifier:

```bash
cd "/tmp/energokey-candidate.Aa5d3b/EnergoKey"
git rev-parse HEAD
git status --short
python3 Tools/verify_deliverable.py
```

## Unity batch compile

Фактический результат: PASS. Unity завершил процесс с кодом 0; лог содержит `*** Tundra build success`; ошибок компилятора C# не обнаружено.

Команда по аргументам, записанным самим Unity в лог:

```bash
"/Applications/Unity/Hub/Editor/6000.6.2f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -nographics -quit \
  -projectPath "/tmp/energokey-candidate.Aa5d3b/EnergoKey" \
  -logFile "[USER_HOME]/Desktop/untitled folder 2/EnergoKey/EVIDENCE/logs/fresh-clone-candidate-compile.log"
```

Доказательство: `EVIDENCE/logs/fresh-clone-candidate-compile.log`.

## EditMode tests

Фактический результат: PASS, 24/24; 0 Failed, 0 Skipped. Начало прогона: `2026-10-27 14:02:48Z`. Лог содержит `Running tests`, `Saving results` и `Test run completed` с кодом 0.

Команда по аргументам Unity:

```bash
"/Applications/Unity/Hub/Editor/6000.6.2f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -nographics \
  -projectPath "/tmp/energokey-candidate.Aa5d3b/EnergoKey" \
  -runTests -testPlatform EditMode \
  -testResults "[USER_HOME]/Desktop/untitled folder 2/EnergoKey/EVIDENCE/FreshCloneTests-candidate.xml" \
  -logFile "[USER_HOME]/Desktop/untitled folder 2/EnergoKey/EVIDENCE/logs/fresh-clone-candidate-tests.log"
```

Доказательства:

- `EVIDENCE/FreshCloneTests-candidate.xml`;
- `EVIDENCE/logs/fresh-clone-candidate-tests.log`.

## Structural validation

Фактический результат: PASS, 165/165. Лог содержит `ENERGOKEY_VALIDATION_PASS: 165/165 checks passed`.

Команда по аргументам Unity:

```bash
"/Applications/Unity/Hub/Editor/6000.6.2f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -nographics -quit \
  -projectPath "/tmp/energokey-candidate.Aa5d3b/EnergoKey" \
  -executeMethod EnergoKey.Editor.EnergoKeyValidation.RunAll \
  -logFile "[USER_HOME]/Desktop/untitled folder 2/EnergoKey/EVIDENCE/logs/fresh-clone-candidate-validation.log"
```

Доказательство: `EVIDENCE/logs/fresh-clone-candidate-validation.log`.

## Контрольные суммы

| Файл | Байты | SHA-256 |
|---|---:|---|
| `EVIDENCE/FreshCloneTests-candidate.xml` | 21908 | `5446aa51d3ad12e5bae54e6bc7e572d9d015bed3d5589eaf641580b1dab7717b` |
| `EVIDENCE/logs/fresh-clone-candidate-verifier.log` | 3754 | `381feaec6e42d6f3019a6c07358bd189a9524d97e3a928be4037612e3c4f0002` |
| `EVIDENCE/logs/fresh-clone-candidate-compile.log` | 298084 | `22ba120bf22e6345fa7b558350fbb5c9bd561dfea91d79648ea0efa02569da32` |
| `EVIDENCE/logs/fresh-clone-candidate-tests.log` | 26367 | `0e3c259794ee48ea58caad0a094bf47cac23ab9f03acacb917f0a1584ae54143` |
| `EVIDENCE/logs/fresh-clone-candidate-validation.log` | 21951 | `a7c835c69edb2e2583bce3a259f76537aaa0960cff5baed48bbb8efd16215da6` |

## Ограничение результата

Candidate commit успешно проверен в чистой копии. Exact final tag и итоговый ZIP проверяются отдельно после финального commit: необходимо разрешить полный hash через `git rev-list -n 1 pp-pm13-release`, клонировать tagged revision и записать размер/SHA-256 архива. До этого тег, hash окончательной ревизии и ZIP остаются `NOT VERIFIED`.
