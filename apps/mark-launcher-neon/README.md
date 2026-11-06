# ✦ MARK LAUNCHER NEON — Windows v1.0

Настоящее Windows-приложение на C# WinForms с четырьмя работающими вкладками.

## Возможности
- **Играть:** выбрать установленный EXE своей игры и запустить его; открыть папку игры.
- **Скачать:** ссылки на официальные сайты Minecraft, Xbox и MARKGG GitHub.
- **Настройки:** изменить путь к игре, очистить выбор, включить режим «Поверх других окон».
- **Astra:** локальный помощник с командами: играть, скачать, настройки, статус, журнал, помощь. Это не облачный искусственный интеллект.
- Настройки и журнал хранятся в папке %LOCALAPPDATA%\MarkLauncherNeon.

## Сборка EXE через GitHub Actions

Открой Actions в репозитории, выбери workflow **Mark Launcher Neon - Windows EXE**, нажми **Run workflow**.
После успешной сборки открой запуск и скачай артефакт **MarkLauncherNeon-Windows-x64**.
GitHub упаковывает артефакт в ZIP: распакуй его и запусти MarkLauncherNeon.exe.

EXE собирается автономным для Windows x64. На компьютере пользователя не нужны Python или отдельно установленный .NET.

## Сборка из исходников

Для разработчика с .NET 8 SDK на Windows:

    dotnet publish MarkLauncherNeon.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishTrimmed=false

Исполняемый файл будет в bin/Release/net8.0-windows/win-x64/publish.

## Важно

Это стартовая версия игрового центра, а не полный Minecraft-лаунчер с авторизацией аккаунтов. Она запускает только EXE, который пользователь сам выбрал на компьютере. Выбирай только доверенные приложения.

Файл EXE цифровой подписью пока не подписан; предупреждение Windows SmartScreen может появляться. Не отключай защиту системы ради запуска неизвестного файла.

Этот проект размещён в отдельном каталоге и не изменяет существующий сайт MARKGG NEXUS.
