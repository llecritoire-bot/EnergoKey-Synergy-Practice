# Итоговый протокол проверки exact `pp-pm13-release`

Дата протокола: 27 октября 2026 года.

## Объект проверки

Финальная поставка идентифицируется аннотированным Git-тегом `pp-pm13-release`. Проверяется точная ревизия, на которую указывает этот тег после завершения всех файлов проекта и Evidence.

Полный hash финального commit намеренно не записывается внутрь самой tagged revision: добавление такого значения изменило бы commit и создало самоссылку. После постановки тега фактический hash определяется командой:

```bash
git rev-list -n 1 pp-pm13-release
```

Он вместе с результатами exact-tag проверки фиксируется вне tagged tree:

- в `DELIVERY_MANIFEST.txt` внутри итогового ZIP;
- в каталоге `POST_COMMIT_VERIFICATION/` внутри итогового ZIP;
- SHA-256 самого ZIP — во внешнем sidecar-файле рядом с архивом.

## Последовательность post-commit проверки

1. Завершить финальный commit и создать/переставить аннотированный тег `pp-pm13-release` именно на него.
2. Разрешить полный hash тега и проверить, что тип объекта тега — `tag`, а commit тега совпадает с упаковываемой ревизией.
3. Создать отдельный чистый клон репозитория и checkout exact `pp-pm13-release`.
4. До запуска Unity убедиться, что `git status --porcelain` пуст.
5. В tagged clone выполнить read-only verifier комплекта.
6. Запустить Unity 6000.6.2f1 batch compile.
7. Запустить финальные EditMode и PlayMode tests.
8. Запустить structural validation.
9. Упаковать проект только из exact tagged clone, исключив `Library`, `Temp`, `Logs`, `obj`, `UserSettings`, `Build` и `Builds`.
10. Проверить список и целостность ZIP, вычислить SHA-256.
11. Записать фактические команды, коды возврата, количества тестов, полный commit hash и контрольные суммы в post-commit-слой поставки.

Минимальные Git-проверки:

```bash
RELEASE_HASH=$(git rev-list -n 1 pp-pm13-release)
test "$(git cat-file -t pp-pm13-release)" = tag
test -n "$RELEASE_HASH"

git clone --no-hardlinks "[REPOSITORY]" "[TEMP_TAGGED_CLONE]"
git -C "[TEMP_TAGGED_CLONE]" checkout --detach pp-pm13-release
test "$(git -C "[TEMP_TAGGED_CLONE]" rev-parse HEAD)" = "$RELEASE_HASH"
test -z "$(git -C "[TEMP_TAGGED_CLONE]" status --porcelain)"
```

Временный путь намеренно обозначен placeholder: локальный путь конкретного запуска не является частью доказательства и не должен попадать в сдаваемые файлы.

## Что должен содержать post-commit-слой ZIP

`DELIVERY_MANIFEST.txt` должен указывать:

- точный 40-символьный hash `pp-pm13-release`;
- подтверждение annotated tag и совпадения tag/упакованного HEAD;
- результат чистоты клона до Unity;
- результаты verifier, compile, EditMode, PlayMode и structural validation;
- отсутствие запрещённых каталогов;
- перечень файлов `POST_COMMIT_VERIFICATION/` с SHA-256;
- SHA-256 внешнего итогового ZIP sidecar.

`POST_COMMIT_VERIFICATION/` должен содержать фактические логи и XML именно exact tagged clone. Их имена и контрольные суммы перечисляются в `DELIVERY_MANIFEST.txt`; эти файлы не добавляются обратно в tagged commit, чтобы не менять проверенную ревизию.

## Связанные доказательства до упаковки

- `EditModeTests-final.xml` и `logs/editmode-tests-final.log`;
- `PlayModeTests-final.xml` и `logs/playmode-tests-final.log`;
- `PROJECT_VALIDATION.md`, `PROJECT_VALIDATION.json` и validation log;
- `PROFILER_BENCHMARK_BEFORE.json`, `PROFILER_BENCHMARK_AFTER.json`;
- `ProfilerBenchmarkBeforeTests.xml`, `ProfilerBenchmarkAfterTests.xml`;
- `logs/profiler-benchmark-before.log`, `logs/profiler-benchmark-after.log`;
- `PERSISTENCE_RESTART_PROBE.md` / `.json`;
- `MAC_STANDALONE_TEST.md` и `FOCUS_LOSS_TEST.md`.

## Честные ограничения

- публичный GitHub/GitLab URL отсутствует;
- физические Android/iOS-устройства и мобильные builds не проверялись;
- внешний новый пользователь не привлекался;
- пользователь-заказчик подтвердил работу на MacBook Apple M3, но он не считается новым внешним пользователем;
- Word/PDF/PPTX, подписи и загрузка в LMS остаются внешними административными действиями.
