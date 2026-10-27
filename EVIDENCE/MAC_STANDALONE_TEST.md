# Проверка macOS Standalone на физическом MacBook

- Сборка: `PASS`
- Фактический запуск собранного приложения: `PASS`
- Runtime-маркер готовности: `PASS`
- Цель: `macOS Standalone (Apple Silicon compatible)`
- Тип: `Development Build`, `StrictMode`
- Unity: `6000.6.2f1`
- Версия игры: `1.0.0`
- Сцен в сборке: `Standalone` / `6`
- Ошибок сборки: `0`
- Предупреждений сборки: `0`
- Размер сборки, байт: `169220405`
- Длительность сборки: `00:00:02.1539890`

Сама сборка создана во временной папке вне проекта и не включается в Git/ZIP. Фактический запуск подтверждается отдельным runtime-логом `logs/mac-standalone-smoke.log` со строкой `ENERGOKEY_RUNTIME_READY`.

MacBook с Apple M3 является физическим компьютером macOS, но не мобильным Android/iOS-устройством.
