# ⚠️ Still a work in progress

> Features and the layout may still move around. Robocopy will not. It has been itself since Windows NT, and it is not taking notes from us.

# Robocopy GUI

A Windows desktop app for people who want Robocopy’s power without assembling a command that looks like a keyboard fell asleep on the switch list.

It is a **C# / .NET 8 / Windows Forms** front end for `robocopy.exe`. You pick the folders, tick the options, read the command it is about to run, and then either run it or think better of it. There is a progress bar, because staring at a black log and guessing is a hobby, not a plan.

> [!WARNING]
> `/MIR`, `/PURGE`, `/MOV`, and `/MOVE` can delete files. Robocopy does this calmly, with no dramatic music. Read the generated command, and use **List only (`/L`)** before you let a mirror anywhere near a folder you like.

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

- Browse for a source folder and a destination folder
- Filter with file patterns, `/XF`, and `/XD`
- Turn Robocopy’s switches on from the tabs, instead of remembering which letter means “please delete the extras”
- Preview the exact `robocopy.exe` command, then copy it if you want to run it somewhere else
- Run Robocopy, watch the log, and cancel it if the plan goes sideways
- Show overall copy progress, plus the current file and that file’s own percentage
- Write a log with `/LOG`, `/LOG+`, `/UNILOG`, or `/UNILOG+`
- Pass any switch the tabs do not cover through **Other switches**
- Translate Robocopy’s exit codes, which do not mean what exit codes mean anywhere else

### Options on the tabs

| Tab | What you can set |
|---|---|
| Copy | Subfolders, empty folders, mirror, purge, move, restartable and backup modes, unbuffered I/O, sparse files, links, junctions, compression, offload, cloning, folder depth |
| Selection | Archive-only copies, include and exclude rules, size and age limits, FAT times, DST, symbolic links and junctions |
| Attributes | `/COPY` and `/DCOPY` flags, `/COPYALL`, `/SEC`, `/SECFIX`, `/TIMFIX`, and adding or removing attributes |
| Performance | Threads (`/MT`), packet gap, retries, low-free-space pause, run hours, monitoring, and I/O throttling |
| Logging | List only, verbose output, timestamps, full paths, byte sizes, what to hide, ETA, console-and-log output, and the log file |
| Advanced | Job files (`/JOB`, `/SAVE`), quit after reading the command, and a box for any other switch |

`/SECFIX` fixes security on files that are skipped. It does not mean “copy everything including the owner’s diary.” `/COPYALL` is the one that copies all the file information.

The progress bar follows the size of the files being copied. Under it, the status line names the current file and how far through that file Robocopy is, which is the difference between “something is happening” and “a 40 GB file is happening.” A job that finishes with no failures moves the bar to 100%.

---

## Requirements

- **Windows**, with `robocopy.exe` on the path. A normal Windows install already has it. If yours does not, that is a bigger conversation than this README.
- **.NET 8 SDK** if you are building from source.
- **64-bit Windows** for the `win-x64` publish command below.

---

## Getting started

### Clone the repository

```bash
git clone https://github.com/abhidotnet/RoboCopyGui.git
cd RoboCopyGui
```

### Build and run

```bash
dotnet build
dotnet run
```

`dotnet run` opens the window. It does not copy anything until you click **Run Robocopy**, which is the correct order of events.

### Publish an executable

```bash
dotnet publish -c Release
```

The project targets `win-x64`. The executable lands under `bin\Release\net8.0-windows\win-x64\publish\`. It still needs the .NET 8 desktop runtime on the machine that runs it, unless you publish self-contained:

```bash
dotnet publish -c Release --self-contained true
```

That build is larger. It also does not need a separate runtime install, which is the whole point of carrying the runtime around in your pocket.

---

## How to use

1. Choose a **source** and a **destination**. Both are required, and the source has to exist. Robocopy is brave, not clairvoyant.
2. Leave **Files** as `*.*` to copy everything, or name the patterns you actually want, such as `*.docx *.xlsx`.
3. Exclude files or folders if there is something in the source you would rather not meet at the destination.
4. Set options on the tabs. The **Generated command** box updates as you go. If the command looks wrong, the copy will too.
5. Click **Run Robocopy**. Mirror, purge, and move ask first, unless **List only (`/L`)** is on, because a list cannot delete your holiday photos.
6. Watch the bar and the log. **Cancel** stops the Robocopy process.
7. Read the exit code at the end. A number other than 0 can still be a happy ending. See below, and try not to take it personally.

**Copy command** puts the generated command on the clipboard, for the moments when you trust the switches and still want to run them yourself.

---

## Examples

Copy a folder, including subfolders, and leave empty ones behind:

- Tick **Copy subdirectories (`/S`)**.
- Leave the dramatic red options alone.

See what a mirror would do, without it actually doing it:

- Tick **Mirror source to destination (`/MIR`)**.
- Tick **List only (`/L`)** on the Logging tab.
- Run it. Read the log. Then, and only then, untick `/L` if the list was what you hoped for.

Copy large files with a few threads and unbuffered I/O:

- Tick **Unbuffered I/O (`/J`)** and **Copy with multiple threads (`/MT`)**.
- The progress text is a bit less precise while several files copy at once. Robocopy is busy. The bar is doing its best.

---

## Safety notes

- `/MIR` makes the destination match the source, including the part where extra destination files go away.
- `/PURGE` deletes destination files and folders that are no longer in the source.
- `/MOV` and `/MOVE` delete from the source after the copy. “Move” is not a cute word here.
- The app asks before running those, unless you are in list-only mode.
- `/MT` cannot be combined with a packet gap (`/IPG`), EFS raw mode (`/EFSRAW`), or low-free-space mode (`/LFSM`). The app refuses that combination rather than letting Robocopy refuse it for you after you have already made tea.
- `/REG` saves your retry settings as the machine defaults. It is labelled in red for a reason.

---

## Robocopy exit codes

Robocopy treats exit codes as a set of flags, not as a simple pass or fail. Zero is peaceful. Eight and above means something actually failed.

| Code | Meaning |
|---|---|
| 0 | Nothing was copied, and nothing failed. A perfectly quiet success. |
| 1 | Files were copied successfully. |
| 2 | Extra files or directories were found at the destination. |
| 3 | Files were copied, and extras were found. |
| 4 | Mismatched files or directories were found. |
| 5 | Files were copied, and mismatches were found. |
| 6 | Extras and mismatches were found. |
| 7 | Files were copied, and both extras and mismatches were found. |
| 8 or more | At least one copy failure. This is the one to worry about. |

Codes 1 through 7 can be combined. 3 is 1 plus 2, and Robocopy is not going to apologise for the arithmetic.

---

## Advanced switches

The tabs cover the switches from `robocopy /?`. If a future Windows build invents another one, or if you simply prefer to type `/IF` yourself, put it in **Other switches** on the Advanced tab. That text is passed through as written, so a typo there is your typo. The command preview is the place to notice it.

---

## Known limitations

- The progress percentage is estimated from the source tree. Filters such as file age are only partly reflected in that estimate, so the bar may sit just short of full until the job ends cleanly, and then it goes to 100%.
- **Hide Robocopy’s own percentage (`/NP`)** removes the live ticks, so the bar advances once per file instead.
- **Multi-threaded copy (`/MT`)** interleaves progress. The percentage is less exact, and a little more hopeful.
- Hiding file or directory names (`/NFL`, `/NDL`) also hides information the bar uses to measure the job.
- This app does not reimplement Robocopy. If `robocopy.exe` cannot do it, a checkbox will not invent it.

---

## Licence

No licence file is in the repository yet. Until there is one, treat the code as all rights reserved by the author.
