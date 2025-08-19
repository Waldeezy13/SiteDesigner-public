# SiteDesigner AutoCAD Civil 3D Add-in

A .NET 8 AutoCAD Civil 3D 2026.1 add-in for site design and layout automation with intelligent offset detection and automated public repository mirroring.

## Development Setup

### AutoCAD Path Configuration

The project uses a flexible AutoCAD path resolution system to work across different machines and AutoCAD versions.

#### Default Behavior
By default, the project looks for AutoCAD 2026 in the standard installation path:
```
C:\Program Files\Autodesk\AutoCAD 2026\
```

#### Per-Machine Overrides

If you have AutoCAD installed in a different location or version, you have several options:

**Option 1: Local Properties File (Recommended)**
1. Copy `AutoCAD.ManagedDir.props.example` to `AutoCAD.ManagedDir.props` in the solution root
2. Uncomment and modify the `AutoCADManagedDir` property:
```xml
<Project>
  <PropertyGroup>
    <AutoCADManagedDir>C:\Program Files\Autodesk\AutoCAD 2025\</AutoCADManagedDir>
  </PropertyGroup>
</Project>
```

**Option 2: Environment Variable**
Set the `ACAD_MANAGED` environment variable to your AutoCAD installation path:
```cmd
set ACAD_MANAGED=C:\Program Files\Autodesk\AutoCAD 2025
```

**Option 3: MSBuild Command Line**
Pass the path during build:
```cmd
dotnet build -p:AutoCADManagedDir="C:\Custom\AutoCAD\Path\"
```

### Build Process

The project builds to a temporary directory to avoid AutoCAD file locking issues:
```
%TEMP%\SiteDesigner\Build\net8.0-windows\SiteDesigner.Plugin.dll
```

### Loading in AutoCAD

1. Open AutoCAD Civil 3D 2026.1
2. Type `NETLOAD` and browse to the DLL path above
3. Run `SDSTART` to open the Site Designer palette
4. Use `SDSITESETUP` to select a boundary and configure the site

## Repository Structure

This project uses automated mirroring between private and public repositories:

- **Private Repository**: Full development with sensitive configurations
- **Public Repository**: [SiteDesigner-public](https://github.com/Waldeezy13/SiteDesigner-public) - Automatically mirrored via GitHub Actions

### Automated Mirroring

The project includes GitHub Actions workflow (`.github/workflows/mirror.yml`) that automatically:
- Mirrors `main` and `develop` branches to the public repository
- Syncs tags and releases
- Maintains clean separation between private development and public sharing

## Features

- **Resizable Palette**: Modern WPF interface hosted in AutoCAD with proper ElementHost configuration
- **Intelligent Offset Detection**: Automatically determines inward vs outward offsets using area comparison and inside point detection
- **Layer Management**: Creates properly styled layers (C-SITE-SETBACK) with green color and dashed linetype
- **Site Configuration**: Configurable setbacks and parking parameters with real-time UI binding
- **Multiple Commands**: Both palette buttons and command-line access
- **Portable Development**: Cross-machine AutoCAD path configuration
- **Professional Git Setup**: Enhanced .gitignore with crash/telemetry exclusions

## Commands

- `SDSTART` - Open the Site Designer palette
- `SDSITESETUP` - Select site boundary and inside point for offset direction detection
- `SDDRAWSETBACK` - Draw setback polygon on C-SITE-SETBACK layer using minimum setback distance
- `SDFLOATPALETTE` - Float and resize palette (useful for palette resizing issues)
- `SDRESETFLOAT` - Reset palette with new GUID to clear persisted UI state
- `SDDIAG` - Show comprehensive diagnostic information
- `SDALTPALETTE` - Create alternative palette configuration for testing

## Technical Architecture

### .NET 8 Project Structure
- **SiteDesigner.Core** (.NET 8): Configuration models and shared types
- **SiteDesigner.Plugin** (.NET 8 Windows): AutoCAD integration with portable reference paths  
- **SiteDesigner.UI** (.NET 8 Windows WPF): Modern resizable palette interface

### Key Components
- **GeometryUtil**: Intelligent offset direction detection using ray casting and area calculations
- **PaletteHost**: Manages AutoCAD PaletteSet with ElementHost for WPF content
- **SetbackService**: Layer management and setback polygon creation
- **LayoutService**: Test layout placement with keyword prompts for offset direction

### AutoCAD Integration
- **Document Locking**: Proper `using (doc.LockDocument())` for palette-triggered operations
- **Command Registration**: `[CommandMethod]` attributes with `[assembly: CommandClass]`
- **Error Handling**: Comprehensive exception handling with user feedback
- **Reference Management**: Non-private AutoCAD assembly references to avoid deployment issues

## Development Workflow

1. **Private Development**: Work in private repository with full access to configurations
2. **Automated Mirroring**: GitHub Actions automatically syncs to public repository on push
3. **Public Sharing**: Clean public repository without sensitive data or machine-specific configs
4. **Community Engagement**: Public repository enables community contributions and usage

## Mirror check
Public mirror smoke test — commit #1

## License

[Choose appropriate license - MIT, Apache 2.0, etc.]