# Публичный репозиторий

- URL: `https://github.com/llecritoire-bot/EnergoKey-Synergy-Practice`
- Основная ветка: `main`
- Видимость: `PUBLIC`
- Проверенная техническая ревизия: `87a0b2642b7f7d02bcdb8165dccedccf2dc2164f`.
- Точный hash итогового пакета: `git rev-parse submission-final^{commit}` после создания неизменяемого тега на финальном метадата-коммите.
- Hash экспортированной архивной ревизии: `$Format:%H$` (подставляется `git archive`).
- Дата проверки доступности: 27 октября 2026 года.

## Этапные теги для публикации

- `up-pm12-prototype`
- `pp-pm12-three-levels`
- `pm13-before-visual-style`
- `up-pm13-styled`
- `profiler-before`
- `pp-pm13-release`
- `submission-final` — итоговый тег, создаваемый после фиксации этого протокола.

## Текущий этап проверки

Репозиторий опубликован. Неавторизованный `git ls-remote` с отключённым credential helper завершился кодом 0 и вернул:

- `refs/heads/main` → `87a0b2642b7f7d02bcdb8165dccedccf2dc2164f`;
- все шесть этапных тегов и их commit refs.

GitHub сообщил видимость `PUBLIC`, основную ветку `main`; публичная HTML-страница вернула HTTP 200. Локальный HEAD и `origin/main` совпали перед проверкой свежего клона. После метадата-коммита ветка и итоговый тег проверяются повторно.
