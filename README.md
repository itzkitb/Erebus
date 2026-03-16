# ⚡ Erebus
[![License](https://img.shields.io/github/license/itzkitb/Erebus)](https://github.com/itzkitb/Erebus)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Russian](https://img.shields.io/badge/README-%D0%B2%D0%B5%D1%80%D1%81%D0%B8%D1%8F_%D0%BD%D0%B0_%D1%80%D1%83%D1%81%D1%81%D0%BA%D0%BE%D0%BC-RU.svg)](README.ru.md)

Secure password manager with encrypted vault storage for desktop platforms.

## 🚀 Features

- **Multiple Vaults**: Create and manage multiple encrypted vaults
- **Record Types**: Store passwords, notes, passports/documents, and files
- **Military-Grade Encryption**: AES-256-GCM for data, Argon2id for password hashing
- **Desktop Application**: Cross-platform Blazor desktop app powered by Photino
- **Session Security**: Configurable auto-lock timeout
- **Password Health**: Analyze password strength and check for breaches
- **Export/Import**: Secure backup and restore functionality
- **Dark Theme**: Modern UI with dark theme by default

## 🛠 Tech Stack

- **Language**: C# (.NET 10)
- **UI Framework**: Blazor (Photino.NET for desktop)
- **Database**: SQLite with SQLCipher encryption
- **Cryptography**: 
  - AES-256-GCM (data encryption)
  - Argon2id (password hashing)
  - HKDF-SHA256 (key derivation)
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Logging**: Custom secure logger with sensitive data filtering

## 📦 Installation

### From Source

```bash
# Clone the repository
git clone https://github.com/SillyApps/Erebus.git
cd Erebus

# Build the solution
dotnet build --configuration Release

# Run the desktop application
dotnet run --project Erebus.Desktop
```

### Requirements

- .NET 10 SDK or later
- Windows 10/11, Linux, or macOS

## ⚙️ Usage

### Launch Application

```bash
# Run from project directory
dotnet run --project Erebus.Desktop

# Or publish and run standalone
dotnet publish Erebus.Desktop -c Release -r win-x64 --self-contained
```

### First Time Setup

1. Launch the application
2. Click "Create First Vault"
3. Enter vault name and master password
4. Start adding password records, notes, or files

### Project Structure

```
Erebus/
├── Erebus.Core/           # Domain models, interfaces, DTOs
├── Erebus.Cryptography/   # Encryption, hashing, key derivation
├── Erebus.Infrastructure/ # Repositories, services, file system
├── Erebus.App.Shared/     # Blazor components and pages
├── Erebus.Desktop/        # Photino desktop host
└── Erebus.Tests/          # Unit tests
```

## 🔒 Security

- Master password never stored, only verification hash
- Each file encrypted with unique derived key (HKDF)
- Secure memory clearing for sensitive data
- Constant-time password comparison
- SQLCipher for database encryption

## 📄 License

Distributed under the MIT License. See `LICENSE` for more information.

---

**Developer**: SillyDev (SillyApps)  
**GitHub**: [@SillyApps](https://github.com/itzkitb)
