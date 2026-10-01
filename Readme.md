# ⚠️ WORK IN PROGRESS

> This project is under active development. Features, interface behaviour, and supported options may change.

# Robocopy GUI

A Windows desktop GUI for running **Robocopy** (`robocopy.exe`) without manually composing command-line commands.

Built with **C#**, **.NET 8**, and **Windows Forms**, the application provides folder selection, common Robocopy options, command preview, real-time output, logging, and process cancellation.

> [!WARNING]
> Robocopy can delete files and folders when certain options are used. Always check the generated command and use `/L` (list-only mode) before running an unfamiliar or destructive operation.

---

## Contents

- [Features](#features)
- [Requirements](#requirements)
- [Getting started](#getting-started)
- [Build and run](#build-and-run)
- [Publish an executable](#publish-an-executable)
- [How to use](#how-to-use)
- [Examples](#examples)
- [Safety notes](#safety-notes)
- [Robocopy exit codes](#robocopy-exit-codes)
- [Advanced switches](#advanced-switches)
- [Known limitations](#known-limitations)
- [Licence](#licence)

---

## Features

- Browse for **source** and **destination** folders
- Define file patterns, such as `*.docx *.xlsx`
- Exclude files using `/XF`
- Exclude directories using `/XD`
- Configure common Robocopy options through the interface
- Preview the generated `robocopy.exe` command
- Copy the generated command to the clipboard
- Run Robocopy directly from the application
- Stream Robocopy output in real time
- Cancel an active Robocopy process
- Save output using `/LOG`
- Add any additional supported Robocopy options through an **Advanced switches** field
- Interpret Robocopy exit codes, including its non-standard success behaviour

### Currently exposed options

| Category | Options |
|---|---|
| Copy | `/S`, `/E`, `/MIR`, `/MOV`, `/MOVE` |
| Copy metadata | `/COPYALL`, `/SEC`, `/SECFIX` |
| Resilience | `/Z`, `/B`, `/ZB`, `/R:n`, `/W:n` |
| Output | `/V`, `/L`, `/ETA`, `/NP`, `/TEE`, `/LOG:file` |
| Filters | `/XF`, `/XD`, file patterns |

---

## Requirements

- **Windows**
  - Robocopy must be available as `robocopy.exe`.
  - Robocopy is included with modern Windows versions.
- **.NET 8 SDK** to build from source.
- **Windows x64** when using the example `win-x64` publish command.

---

## Getting Started

### Clone the repository

```bash
git clone https://github.com/abhidotnet/RoboCopyGui.git
cd RoboCopyGui