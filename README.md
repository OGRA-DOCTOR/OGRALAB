# OGRALAB - Medical Laboratory Management System

## Overview
OGRALAB is a comprehensive medical laboratory management system built with WPF, C#, and .NET 8.0. This repository contains Phase 1 implementation focusing on the login window with complete authentication functionality.

## Features

### Phase 1 - Login System
- **Modern WPF Interface**: Clean, professional design with medical-themed color scheme
- **MVVM Architecture**: Complete separation of concerns using MVVM pattern
- **SQLite Database**: Entity Framework Core 8 with SQLite for data persistence
- **Password Security**: BCrypt hashing for secure password storage
- **User Experience**:
  - Username dropdown with recent usernames
  - Password visibility toggle
  - Remember Me functionality
  - Enter key support for login
  - Real-time status messages
- **Single Instance**: Prevents multiple application instances
- **Default User**: admin / Admin@123

## Technical Stack

- **.NET 8.0**: Latest .NET framework
- **WPF**: Windows Presentation Foundation for UI
- **Entity Framework Core 8**: Database ORM
- **SQLite**: Lightweight database engine
- **BCrypt.Net**: Password hashing
- **Dependency Injection**: Built-in .NET DI container
- **MVVM Pattern**: Model-View-ViewModel architecture

## Project Structure

```
OGRALAB/
├── src/OGRALAB/
│   ├── Commands/           # Command implementations for MVVM
│   ├── Data/              # Entity Framework DbContext
│   ├── Images/            # Application images and logo
│   ├── Models/            # Data models
│   ├── Resources/         # Application resources
│   ├── Services/          # Business logic services
│   ├── ViewModels/        # MVVM ViewModels
│   └── Views/             # WPF Windows and UserControls
├── OGRALAB.sln           # Visual Studio Solution
└── README.md             # This file
```

## Prerequisites

- Windows 10/11
- .NET 8.0 SDK
- Visual Studio 2022 (recommended) or Visual Studio Code with C# extension

## Installation & Setup

1. **Clone the repository**:
   ```bash
   git clone <repository-url>
   cd OGRALAB
   ```

2. **Restore packages**:
   ```bash
   dotnet restore
   ```

3. **Build the project**:
   ```bash
   dotnet build
   ```

4. **Run the application**:
   ```bash
   dotnet run --project src/OGRALAB
   ```

## Database Setup

The application automatically creates and initializes the SQLite database on first run:
- Database file: `Data/ogralab.db`
- Automatic migration and seeding
- Default admin user is created automatically

## Default Login Credentials

- **Username**: admin
- **Password**: Admin@123

## Color Scheme

The application uses a medical-themed color palette:
- **Primary Color**: #1B4D3E (Dark Green)
- **Secondary Color**: #2E7D68 (Medium Green)
- **Accent Color**: #4A9F8A (Light Green)
- **Background**: #E8F4F8 (Light Blue)
- **Text**: #333333 (Dark Gray)

## Configuration

Application settings are stored in `appsettings.json`:
- Database connection string
- Logging configuration
- Application-specific settings

## Features in Detail

### Authentication Service
- Secure password verification using BCrypt
- User session management
- Remember Me functionality with expiration
- Recent usernames tracking

### Single Instance
- Prevents multiple application instances
- Brings existing instance to foreground when duplicate launch is attempted

### MVVM Implementation
- Clean separation between UI and business logic
- Command pattern for user interactions
- Data binding for reactive UI updates
- Property change notifications

## Development

### Adding New Features
1. Create models in `Models/` folder
2. Add services in `Services/` folder
3. Implement ViewModels in `ViewModels/` folder
4. Create Views in `Views/` folder
5. Register services in `App.xaml.cs`

### Database Changes
1. Modify models or add new ones
2. Update `OgralabDbContext`
3. The application uses `EnsureCreated()` for development

## Future Phases

- Phase 2: Patient Management
- Phase 3: Test Management
- Phase 4: Laboratory Operations
- Phase 5: Reporting and Analytics
- Phase 6: System Administration

## License

This project is proprietary software. All rights reserved.

## Support

For technical support or questions, please contact the development team.

---

**OGRALAB v1.0.0** - Medical Laboratory Management System
