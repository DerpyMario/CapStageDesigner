Complete Stage Designer System Features:
Core Components:

    StageDesignerWindow - Main editor window with full stage editing capabilities
    StageObject - Component for all stage objects with snapping and properties
    StageManager - Runtime manager for loading/saving stages
    ClipBounds - Defines stage clip boundaries
    StageDesignerSettings - Configurable settings asset

Editor Tools:

    PrefabPalette - Drag-and-drop prefab placement tool
    StageBatchOperations - Bulk operations on multiple objects
    StageValidator - Validation and optimization tools
    StageDesignerSceneTool - Custom scene view tool
    Custom Inspectors - Enhanced property editing

Advanced Features:

    Snap System - Grid, vertex, edge, and face snapping
    JSON Import/Export - Full compatibility with your stage format
    MapObjEvent Support - Special handling for map events
    Visual Helpers - Grid display, bounds visualization
    Undo/Redo Support - Full Unity undo integration

Usage:

    Open Tools → Stage Designer → Stage Designer Window
    Create a new stage or load an existing JSON file
    Use GameObject → Stage Designer → Create Stage Root to set up the basic structure
    Add clips with GameObject → Stage Designer → Create Clip
    Use the Prefab Palette (Tools → Stage Designer → Prefab Palette) to place objects
    Configure snapping and grid settings in the main Stage Designer window
    Use Batch Operations for bulk editing multiple objects
    Run Stage Validator to check for issues before export
    Export your finished stage using Assets → Stage Designer → Export Stage JSON

Key Shortcuts (when Stage Designer Scene Tool is active):

    G - Grab/Move mode
    R - Rotate selected object
    S - Scale selected object
    X - Delete selected object
    Ctrl+D - Duplicate selected object
