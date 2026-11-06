# MARKGG NEXUS — Windows EXE

Это исходный проект настоящего **Windows Forms лаунчера** на C# (.NET 8). Программа содержит поиск, карточки 6 проектов, открытие сайтов в браузере и кнопку проверки доступности GitHub Pages.

## Сборка на GitHub

Workflow `.github/workflows/build-windows.yml` запускается автоматически после обновления файлов Windows-проекта в ветке main и может запускаться вручную в GitHub Actions.

После успешного запуска:
- В **Actions** появляется ZIP-артефакт с `MARKGG.NEXUS.exe`.
- В **Releases** появляется выпуск **MARKGG NEXUS — Windows EXE** с приложением.

Сборка: `dotnet publish windows/MarkggNexus.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o dist`

Для запуска на Windows 10/11 x64 не требуется устанавливать Python или .NET. Сам лаунчер — нативное приложение Windows Forms; его шесть веб-приложений открываются в браузере, поэтому для них требуется интернет и работающий GitHub Pages.

Приложение не подписано коммерческим сертификатом. Windows SmartScreen может показать предупреждение для неизвестного издателя.

© MARKGG NEXUS
