# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What This Is

ProtoAttributor is a dual-platform developer tool with two independent extension projects that share a common purpose: automatically managing ProtoBuf (`[ProtoContract]`/`[ProtoMember]`/`[ProtoIgnore]`) and DataContract (`[DataContract]`/`[DataMember]`/`[IgnoreDataMember]`) serialization attributes on C# classes. Both extensions support Add, Reorder, and Remove operations.

## VS Code Extension (`vscode/`)

### Commands

```bash
npm run compile          # build (tsc)
npm run watch            # watch mode
npm run lint             # ESLint
npm run lint-fix         # ESLint with auto-fix
npm run pretest          # compile + lint
npm run test-jest        # unit tests (Jest)
npm run test-jest-watch  # unit tests in watch mode
npm run test-jest-coverage  # unit tests with coverage
npm test                 # VS Code integration tests
npm run vscode:package   # produce .vsix
```

Run a single Jest test file:
```bash
npx jest src/path/to/file.test.ts
```

Jest config: `vscode/jest.config.js`. Test files match `**/src/**/*.test.+(ts|js)`, excluding `/src/test/` (fixture files) and `/Sample/`.

### Architecture

The extension activates on `onLanguage:csharp` and registers three commands (`protoattributor.add`, `.reorder`, `.remove`). Each command prompts the user to choose between Proto or DataContract attribute families via QuickPick.

**Processing pipeline:**

1. `extension.ts` - Command registration; calls `getAllPublicMembers()` then dispatches per-member to handlers.
2. `src/utils/csharp-util.ts` - Regex-based C# text parsing. `getAllPublicMembers()` classifies lines as `Class`, `Enum`, `Method`, `LambaProperty`, or `FullProperty` (`SignatureType` enum). Also handles `using` directives and line-ending detection.
3. `src/proto-attributor-csharp.ts` - Core attribute manipulation: add/remove/reorder attributes, compute next index, write back via VS Code `WorkspaceEdit`.
4. `src/utils/constants.ts` - `Proto` and `Data` static classes with all attribute/using name strings.
5. `src/interfaces/window.interface.ts` - `IWindow` interface for testability.
6. `src/utils/workspace-util.ts` - Guards for workspace/editor state.

**Important:** The VS Code extension uses **regex-based text manipulation**, not an AST. This differs fundamentally from the Visual Studio extension.

## Visual Studio Extension (`visual-studio/`)

### Commands

```bash
# Restore
nuget restore visual-studio\ProtoAttributor.sln

# Build
msbuild visual-studio\ProtoAttributor.sln /t:Rebuild /p:configuration="Release" /p:DeployExtension=false

# Test
dotnet test -c="Release" --verbosity=normal visual-studio\ProtoAttributor.sln

# Run a single test class
dotnet test visual-studio\ProtoAttributor.sln --filter "FullyQualifiedName~ClassName"
```

See `visual-studio/BuildNotes.md` for manual VS build notes and instructions for adding new VS version targets.

### Architecture

VSIX AsyncPackage targeting VS 2022 (min 17.0), .NET Framework 4.8 assembly. Uses **Roslyn (Microsoft.CodeAnalysis) AST rewriting** - a proper syntax tree approach.

**Layers:**

- **Package** (`ProtoAttributorPackage.cs`) - Registers two async services and initializes 12 commands (6 Context menu + 6 Tools menu, split between Proto and DataAnno variants).
- **Commands** (`Commands/Context/`, `Commands/Menu/`) - Three command pairs per attribute family (Add, Renumber, Remove). Context commands operate on Solution Explorer selection; Menu commands operate on the open file.
- **Services** (`Services/`) - `ProtoAttributeService` and `DataAnnoAttributeService` via `IAttributeService`. Each wraps three parsers (Adder, Remover, Rewriter) and parses file content into a Roslyn `CSharpSyntaxTree`.
- **Parsers** - Core Roslyn `CSharpSyntaxRewriter` subclasses:
  - `Parsers/ProtoContracts/BaseProtoRewriter.cs` - Abstract base; handles class/enum declarations and `using` insertion, tracks `StartIndex`.
  - `ProtoAttributeAdder.cs` - Visits property/enum-member declarations to add missing attributes.
  - `ProtoAttributeRewriter.cs` - Renumbers existing attributes sequentially.
  - `ProtoAttributeRemover.cs` - Removes all Proto* attributes and usings.
  - Mirror classes under `Parsers/DataContracts/` for DataMember support.
  - `Parsers/NodeHelper.cs` - Static helpers for Roslyn attribute matching and using-directive insertion.
  - `Parsers/TriviaMaintainer.cs` - Preserves leading/trailing whitespace trivia during node mutation.
- **Executors** (`Executors/`) - `AttributeExecutor` iterates `SelectedItems` in Solution Explorer (supports recursive folder traversal) with a progress dialog; `TextSelectionExecutor` applies changes to a `TextSelection`.

### Test Project

`visual-studio/ProtoAttributor.Tests/` - xUnit targeting .NET 8, assertions via Shouldly, coverage via coverlet. Uses protobuf-net in test fixtures.

## Key Symmetry

Both extensions define the same attribute/using name constants in their respective `Constants.cs` / `constants.ts` files. When adding support for a new attribute family, update both.
