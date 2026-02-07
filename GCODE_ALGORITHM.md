# G-Code Generation Algorithm

## Overview

This document describes how the application converts DXF geometries into CNC G-code commands.

## Supported Entities

- **Lines**: Converted to linear moves (G1 commands)
- **Circles**: Drilling operations at center point
- **Arcs**: Arc movements (G2/G3 commands)
- **Polylines**: Sequential linear/arc moves

## G-Code Generation Process

### 1. Header Generation

```gcode
; Generated G-Code from DXF
; Feed Rate: 100 mm/min
; Spindle Speed: 1000 RPM
; Tool Diameter: 3 mm

G21             ; Metric units
G90             ; Absolute positioning
S1000           ; Spindle speed
M3              ; Spindle on
F100            ; Feed rate
G0 Z10          ; Safe height
```

### 2. Entity Processing

For each DXF entity:

#### Lines
- First point: Rapid positioning (G0) + Z-axis move to cutting depth
- Subsequent points: Linear interpolation (G1) moves

#### Circles
- Move to center position
- Execute drilling at center coordinates

#### Arcs
- Move to start point
- Interpolate arc path with appropriate G2/G3 command

### 3. Footer Generation

```gcode
G0 Z10          ; Return to safe height
M5              ; Spindle off
M30             ; Program end
```

## Example Conversion

### Input DXF
```
Line from (10,10) to (20,10)
Line from (20,10) to (20,20)
```

### Output G-Code
```
G21
G90
S1000
M3
F100
G0 Z10
G0 X10 Y10 ; Rapid move to start
G0 Z-5 ; Move to depth
G1 X20 Y10 Z-5 ; Linear move
G1 X20 Y20 Z-5 ; Linear move
G0 Z10
M5
M30
```

## Customization

### Feed Rate
- Adjustable in UI (default: 100 mm/min)
- Affects cutting speed and material quality

### Spindle Speed
- Adjustable in UI (default: 1000 RPM)
- Depends on tool type and material

### Tool Diameter
- Used for potential offset calculations
- Default: 3mm

## Advanced Features (Future)

- Tool path simulation and visualization
- Collision detection
- Z-axis optimization for rapid movements
- Multi-pass strategies for deep cuts
- Surface finishing passes

## G-Code Standards

This implementation follows ISO 6983 (ISO/DIS 1056) G-code standards:

- **G0**: Rapid positioning (no cutting)
- **G1**: Linear interpolation (cutting)
- **G2/G3**: Circular interpolation (not yet implemented)
- **M3**: Spindle clockwise
- **M5**: Spindle stop
- **M30**: Program end
- **S**: Spindle speed (RPM)
- **F**: Feed rate (mm/min)

---

For more information, see the main [README.md](README.md)
