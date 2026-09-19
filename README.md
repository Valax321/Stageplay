# Stageplay

[![Run .NET Tests](https://github.com/Valax321/Stageplay/actions/workflows/dotnet-test.yml/badge.svg)](https://github.com/Valax321/Stageplay/actions/workflows/dotnet-test.yml)
![.NET version 10.0](https://img.shields.io/badge/version-10.0-blue?logo=dotnet)

Stageplay is an engine-agnostic pure C# framework for creating interactive narrative games, visual/sound novels etc. It
is designed to be low-allocating, trim and AOT compatible and pluggable into just about any existing .NET based game
engine or framework.

Stageplay is inspired by other narrative game tools such as Twine, Ren'Py, ye old NScripter etc. Its main goal over those projects is to be highly portable and not tied to any specific kind of device or OS. If it can run .NET, it should be able to run Stageplay.

The framework is currently under active development. Most things are not yet implemented and the API is subject to wild changes at my whim.

## Goals

I am currently implementing an MVP feature set suitable for building simple NVL or ADV style games with only limited visual and audio scripting features (no super fancy animation or interactive music). Something like you might find in an old NScripter game or a simple Ren'Py project. The scripting language should end up resembling something like BASIC, but hopefully a bit more pleasant to read.

This project aims to follow best practices for portable .NET code -- minimal reflection is used, file access is limited to what is provided by the host game engine, and the entire project is AOT-ready and properly annotated for code trimming. It is also designed to allocate as little GC heap memory as possible during runtime, to minimise stuttters on platforms with crap memory allocators.

## Features

| Feature          | Implementation Stage |
|------------------|----------------------|
| MVP feature-set  | 🚧                   |
| MonoGame runtime | 🚧                   |
| Foster runtime   | ❔                   |

- ✅ Fully Implemented
- 🚧 Under Development
- ❔Under Consideration
- ❌ Unimplemented/out of scope

## Engine Support

Stageplay should be able to be ported to any .NET-based game framework that supports .NET 10, such as MonoGame, FNA,
Foster, MoonWorks, Godot 4 and Unity 7+ (once released).

A reference implementation for a runtime built on MonoGame is included, which was chosen since the framework is stable,
battle-tested and simple enough to make demonstrating the necessary integrations clear.

I also plan to natively support a Foster runtime once that project reaches a stable release.

## License

Stageplay is licensed under the BSD 3-clause license. See LICENSE for details.

Stageplay.Extension.Steamworks includes code from Facepunch.Steamworks, which uses the MIT license. This code is entirely contained within a git submodule in `source/thirdparty` separate from this repo.
