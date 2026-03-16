# ⚡ Erebus
[![License](https://img.shields.io/github/license/itzkitb/Erebus)](https://github.com/SillyApps/Erebus)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![English](https://img.shields.io/badge/README-english_version-EN.svg)](README.md)

Безопасный менеджер паролей с зашифрованными хранилищами для десктопных платформ.

![скриншот](https://github.com/itzkitb/Erebus/blob/main/img/screenshot.png?raw=true)

## 🚀 Возможности

- **Несколько хранилищ**: Создание и управление несколькими зашифрованными хранилищами
- **Типы записей**: Пароли, заметки, паспорта/документы, файлы
- **Военное шифрование**: AES-256-GCM для данных, Argon2id для хеширования паролей
- **Десктопное приложение**: Кроссплатформенное Blazor приложение на Photino
- **Безопасность сессии**: Настраиваемый таймаут автоблокировки
- **Здоровье паролей**: Анализ стойкости и проверка на утечки
- **Экспорт/Импорт**: Безопасное резервное копирование и восстановление
- **Тёмная тема**: Современный UI с тёмной темой по умолчанию

## 🛠 Технологический стек

- **Язык**: C# (.NET 10)
- **UI фреймворк**: Blazor (Photino.NET для десктопа)
- **База данных**: SQLite с шифрованием SQLCipher
- **Криптография**: 
  - AES-256-GCM (шифрование данных)
  - Argon2id (хеширование паролей)
  - HKDF-SHA256 (деривация ключей)
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Логирование**: Кастомный безопасный логгер с фильтрацией чувствительных данных

## 📦 Установка

### Из исходного кода

```bash
# Клонировать репозиторий
git clone https://github.com/SillyApps/Erebus.git
cd Erebus

# Собрать решение
dotnet build --configuration Release

# Запустить десктопное приложение
dotnet run --project Erebus.Desktop
```

### Требования

- .NET 10 SDK или новее
- Windows 10/11, Linux или macOS

## ⚙️ Использование

### Запуск приложения

```bash
# Запуск из директории проекта
dotnet run --project Erebus.Desktop

# Или публикация и запуск standalone
dotnet publish Erebus.Desktop -c Release -r win-x64 --self-contained
```

### Первоначальная настройка

1. Запустите приложение
2. Нажмите "Создать первое хранилище"
3. Введите название хранилища и мастер-пароль
4. Начните добавлять записи паролей, заметки или файлы

### Структура проекта

```
Erebus/
├── Erebus.Core/           # Domain модели, интерфейсы, DTOs
├── Erebus.Cryptography/   # Шифрование, хеширование, деривация ключей
├── Erebus.Infrastructure/ # Репозитории, сервисы, файловая система
├── Erebus.App.Shared/     # Blazor компоненты и страницы
├── Erebus.Desktop/        # Photino десктоп хост
└── Erebus.Tests/          # Unit тесты
```

## 🔒 Безопасность

- Мастер-пароль никогда не хранится, только хеш верификации
- Каждый файл шифруется уникальным производным ключом (HKDF)
- Безопасная очистка памяти для чувствительных данных
- Сравнение паролей за константное время
- SQLCipher для шифрования базы данных

## 📄 Лицензия

Распространяется под лицензией MIT. См. `LICENSE` для дополнительной информации.

---

**Разработчик**: SillyDev (SillyApps)  
**GitHub**: [@SillyApps](https://github.com/itzkitb)
