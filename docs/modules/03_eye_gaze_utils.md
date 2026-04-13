# EyeGazeUtils

## Responsibility

`EyeGazeUtils` contains reusable helper functions shared by the runtime modules.

It is not a processing module by itself.

## Typical Uses

The utility layer is used for tasks such as:

- output directory resolution
- export file name generation
- common renderer lookup
- validation of renderers for highlight operations
- support code shared by export-oriented modules

## Why It Exists

Without a shared utility class, multiple modules would duplicate the same helper code for file handling and renderer access.

This module improves:

- readability
- maintainability
- consistency across modules
