# Stageplay

[![Run .NET Tests](https://github.com/Valax321/Stageplay/actions/workflows/dotnet-test.yml/badge.svg)](https://github.com/Valax321/Stageplay/actions/workflows/dotnet-test.yml)
![.NET version 10.0](https://img.shields.io/badge/version-10.0-blue?logo=dotnet)

Stageplay is a pure C# framework for creating interactive narrative games, visual/sound novels etc. It
is designed to be low-allocating, trim and AOT compatible and is built on top of
the [Foster Framework](https://github.com/FosterFramework/Foster/) by Noel Berry.

Stageplay is inspired by other narrative game tools such as Twine, Ren'Py, ye old NScripter etc. Its main goal over
those projects is to be extensible and portable to any system where .NET can run.

The framework is currently under active development. Most things are not yet implemented and the API is subject to wild
changes at my whim.

## Goals

I am currently implementing an MVP feature set suitable for building simple NVL or ADV style games with only limited
visual and audio scripting features (no super fancy animation or interactive music). Something like you might find in an
old NScripter game or a simple Ren'Py project. The scripting language should end up resembling something like BASIC, but
hopefully a bit more pleasant to read.

This project aims to follow best practices for portable .NET code -- minimal reflection is used, file access is limited
to what is provided by the host framework, and the entire project is AOT-ready and properly annotated for code trimming.
It is also designed to allocate as little GC heap memory as possible during runtime, to minimise stuttters on platforms
with crap memory allocators.

## Features

| Feature                     | Implementation Stage |
|-----------------------------|----------------------|
| MVP feature-set             | 🚧                   |
| Steamworks platform support | 🚧                   |

- ✅ Fully Implemented
- 🚧 Under Development
- ❔Under Consideration
- ❌ Unimplemented/out of scope

## Platform Support

Stageplay should be able to be ported to any platform that can run .NET 10+ and can build SDL3. Currently, Foster runs
on desktop platforms and requires SDL_GPU support, but this is not a hard limitation. Stageplay will run anywhere Foster
does.

The possibility does exist for implementing an entirely custom runtime instead of Foster (probably using
my [Radish.Windowing](https://github.com/Valax321/Radish.Windowing.git) library), however this would be a great deal of
work reinventing the wheel. I will explore this possibility at a later date if Foster no longer fits my needs in the future.

## License

Stageplay is licensed under the zlib license. See LICENSE for details.

Stageplay.Platform.Steamworks uses code from
my [Facepunch.Steamworks fork](https://github.com/Valax321/Facepunch.Steamworks), which uses the MIT license. This code
is
entirely contained within a git submodule in `source/thirdparty` separate from this repo.
