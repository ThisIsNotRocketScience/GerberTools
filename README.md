# GerberTools

This repository contains a suite of C# tools for loading, editing, creating, panelizing, and pre-rendering sets of Gerber files. These tools are designed to facilitate PCB production workflows, particularly for panelization and format conversion.

## Main Applications

### GerberPanelizer
**Purpose:** The primary GUI application for creating PCB panels. It allows you to import multiple board designs, arrange them (manually or automatically), add break-tabs (mousebites), and export the merged Gerber files.

**Usage:** Launch `GerberPanelizer.exe`. No command line arguments are required.

### AutoPanelBuilder
**Purpose:** A command-line version of the panelizer for automated workflows.

**Usage:**
```
AutoPanelBuilder.exe [--settings {file}] [--files {filewithfolders}] [--dumpsample] output_directory
```

### GerberViewer
**Purpose:** A simple GUI application for viewing Gerber files.

**Usage:**
```
GerberViewer.exe [file1] [file2] ...
```

## Command Line Utilities

### File Conversion

#### GerberToImage
**Purpose:** Renders Gerber files to high-resolution PNG images.

**Usage:**
```
GerberToImage <files> [--dpi N] [--noxray] [--nopcb] [--silk color] [--trace color] [--copper color] [--mask color]
```

#### GerberToDxf
**Purpose:** Converts Gerber files to DXF format (AutoCAD).

**Usage:**
```
GerberToDxf <infile> <outfile>
```

#### GerberToOutline
**Purpose:** Extracts the outline from a Gerber file and exports it (typically to SVG).

**Usage:**
```
GerberToOutline.exe <infile> <outfile>
```

### File Manipulation

#### GerberCombiner
**Purpose:** Combines multiple Gerber or Excellon files into a single file.

**Usage:**
```
GerberCombiner <outputfile> <inputfile1> <inputfile2> ...
```

#### GerberMover
**Purpose:** Applies translation (move) and rotation to a Gerber file.

**Usage:**
```
GerberMover <inputfile> <outputfile> <X> <Y> <CX> <CY> <Angle>
```

#### GerberClipper
**Purpose:** Clips a subject Gerber file using a polygon defined in an outline Gerber file.

**Usage:**
```
GerberClipper.exe <outlinegerber> <subject> <outputfile>
```

#### GerberSubtract
**Purpose:** Subtracts one Gerber layer from another. **Experimental.**

**Usage:**
```
GerberSubtract <sourcefile> <subtractfile> <outputfile>
```

#### GerberSplitter
**Purpose:** Splits a Gerber file into slices based on a "slice file".

**Usage:**
```
GerberSplitter <slicefile> <gerberfile1> <gerberfile2> ...
```

#### GerberSanitize
**Purpose:** Reads Gerber files and writes out a "sanitized" version, fixing common formatting issues.

**Usage:**
```
GerberSanitize <file1> <file2> ...
```

### Analysis & Verification

#### GerberAnalyse
**Purpose:** Analyzes Gerber files or Zip archives to report board dimensions, drill counts, and layer types.

**Usage:**
```
GerberAnalyse <inputfile_or_folder>
```

## Building
**Visual Studio:** Open `GerberProjects/GerberProjects.sln` and build.

**Dependencies:** Ensure NuGet packages are restored.

**Linux/Mono:** The tools are standard C# and generally compatible with Mono. Run `./build.sh`. Dependencies should be automatically fetched.

## License
See the LICENSE file for details.
