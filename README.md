# ⚡ MARKGG NEXUS v3

Неоновый портал **шести работающих веб-приложений** для игр, творчества, разработки и 3D-печати.

## Приложения

| Проект | Возможности | Код |
| --- | --- | --- |
| 🚀 Neon Arena | Стрельба, волны, щит, рекорд, кнопки для телефона | [Открыть](apps/neon-arena/index.html) |
| 🎨 Pixel Studio | Кисть, ластик, заливка, отмена, скачивание PNG | [Открыть](apps/pixel-studio/index.html) |
| 🎬 Creator Studio | Планировщик видео, этапы работы, резервные копии JSON | [Открыть](apps/creator-studio/index.html) |
| 🧊 3D Print Lab | Модели с размерами в мм, просмотр, скачивание STL | [Открыть](apps/3d-lab/index.html) |
| 💻 Mark Code Lab | HTML/CSS/JS редактор с изолированным предпросмотром и экспортом | [Открыть](apps/code-lab/index.html) |
| 🌈 Color Forge | Генератор градиентов, CSS, палитры и экспорт SVG | [Открыть](apps/color-forge/index.html) |

## Возможности портала

- Качественный адаптивный интерфейс, неоновая и светлая темы
- Поиск проектов по Ctrl+K и работающие фильтры
- Манифест веб-приложения и Service Worker с сетевым приоритетом и офлайн-кэшем (после первого успешного визита)
- Пользовательская страница 404, favicon, sitemap, robots.txt
- Все шесть приложений автономные: чистые HTML, CSS и JavaScript, без платных API или внешних библиотек

## GitHub Pages — публикация через GitHub Actions

Сайт находится в ветке `main`, а автоматическая сборка и публикация описаны в [`.github/workflows/deploy-pages.yml`](.github/workflows/deploy-pages.yml).

**Включи GitHub Pages один раз** под аккаунтом с правом администрирования репозитория:
1. Открой [Settings → Pages](https://github.com/Markgg17/-1000-/settings/pages).
2. В разделе **Build and deployment → Source** выбери **GitHub Actions** (не `Deploy from a branch`).
3. Открой [Actions → Publish MARKGG NEXUS website](https://github.com/Markgg17/-1000-/actions/workflows/deploy-pages.yml) и нажми **Run workflow → main → Run workflow**.
4. Дождись зелёного статуса **Deploy site** и открой **https://markgg17.github.io/-1000-/**.

Если видишь `There isn't a GitHub Pages site here` или в логах `Get Pages site failed: Not Found`, сайт ещё не активирован в настройках Pages. Обычный `GITHUB_TOKEN` workflow не может самостоятельно создать сайт: для этого требуются дополнительные права управления репозиторием. Файлы `index.html` и всех шести приложений уже есть в репозитории.

## Важно

- Все проекты хранят данные в браузере; Creator Studio позволяет сохранять копии JSON.
- Mark Code Lab запускает код пользователя в iframe без доступа к основному сайту; не открывай неизвестный HTML из непроверенных источников.
- Готовые STL рекомендуется проверять в слайсере перед печатью.
- EXE-приложения Windows из предыдущих чатов **не загружены** на GitHub и не являются частью этого веб-сайта.

© MARKGG NEXUS
