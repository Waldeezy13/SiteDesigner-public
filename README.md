# SiteDesigner AutoCAD Civil 3D Add-in

A .NET 8 AutoCAD Civil 3D 2026.1 add-in for site design and layout automation.

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

**Option 1: Environment Variable (Recommended)**
Set the `ACAD_MANAGED` environment variable to your AutoCAD installation path:
```cmd
set ACAD_MANAGED=C:\Program Files\Autodesk\AutoCAD 2025
```

**Option 2: Local Properties File**
1. Copy `AutoCAD.ManagedDir.props.example` to `AutoCAD.ManagedDir.props`
2. Uncomment and modify the `AutoCADManagedDir` property:
```xml
<AutoCADManagedDir>C:\Program Files\Autodesk\AutoCAD 2025\</AutoCADManagedDir>
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

## Features

- **Resizable Palette**: Modern WPF interface hosted in AutoCAD
- **Intelligent Offset Detection**: Automatically determines inward vs outward offsets
- **Layer Management**: Creates properly styled layers for different elements
- **Site Configuration**: Configurable setbacks and parking parameters
- **Multiple Commands**: Both palette buttons and command-line access

## Commands

- `SDSTART` - Open the Site Designer palette
- `SDSITESETUP` - Select site boundary and inside point
- `SDDRAWSETBACK` - Draw setback polygon on C-SITE-SETBACK layer
- `SDDIAG` - Show diagnostic information
- `SDFLOATPALETTE` / `SDRESETFLOAT` - Palette management utilities