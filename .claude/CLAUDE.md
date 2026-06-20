# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What This Is

ProtoAttributor is a dual-platform developer tool with two independent extension projects that share a common purpose: automatically managing ProtoBuf (`[ProtoContract]`/`[ProtoMember]`/`[ProtoIgnore]`) and DataContract (`[DataContract]`/`[DataMember]`/`[IgnoreDataMember]`) serialization attributes on C# classes. Both extensions support Add, Reorder, and Remove operations.

## VS Code Extension (`vscode/`)

### Commands

```bash
npm run compile             # build (tsc)
npm run watch               # watch mode
npm run lint                # ESLint
npm run lint-fix            # ESLint with auto-fix
npm run pretest             # compile + lint
npm run test-jest           # unit tests (Jest)
npm run test-jest-watch     # unit tests in watch mode
npm run test-jest-coverage  # unit tests with coverage (enforces 70% threshold)
npm test                    # VS Code integration tests
npm run vscode:package      # produce .vsix
```

Run a single Jest test file:
```bash
npx jest src/path/to/file.test.ts
```

Jest config: `vscode/jest.config.js`. Test files match `**/src/**/*.test.+(ts|js)`, excluding `/src/test/` (fixture files) and `/Sample/`. Coverage threshold is 70% across all metrics; `extension.ts` is excluded from coverage.

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

# Build (requires MSBuild / Visual Studio installed — dotnet CLI cannot build the VSIX project)
msbuild visual-studio\ProtoAttributor.sln /t:Rebuild /p:configuration="Release" /p:DeployExtension=false

# Test (no MSBuild needed — Core project is SDK-style)
dotnet test -c="Release" --verbosity=normal visual-studio\ProtoAttributor.Tests\ProtoAttributor.Tests.csproj

# Run a single test class
dotnet test visual-studio\ProtoAttributor.Tests\ProtoAttributor.Tests.csproj --filter "FullyQualifiedName~ClassName"
```

See `visual-studio/BuildNotes.md` for manual VS build notes and instructions for adding new VS version targets.

### Project Structure

The solution has three projects:

| Project | Type | Purpose |
|---------|------|---------|
| `ProtoAttributor` | Legacy non-SDK VSIX (ToolsVersion="15.0") | VS extension package — Commands, Services, Executors, Package |
| `ProtoAttributor.Core` | SDK-style netstandard2.0 | Parsers + Constants only; no VS SDK deps; referenced by tests |
| `ProtoAttributor.Tests` | SDK-style net8.0 xUnit | References Core, not the VSIX project |

**Why Core exists:** The VSIX project uses VS SDK APIs (`EnvDTE`, `Microsoft.VisualStudio.*`) that `dotnet CLI` cannot build. `ProtoAttributor.Core` compiles the pure-Roslyn parser/constant files via relative `<Compile Include>` paths so tests can build and run without MSBuild. The VSIX project compiles those same files independently — no files were moved.

### Architecture

VSIX AsyncPackage targeting VS 2022 (min 17.0), .NET Framework 4.8 assembly. Uses **Roslyn (Microsoft.CodeAnalysis) AST rewriting**.

**Layers:**

- **Package** (`ProtoAttributorPackage.cs`) - Registers two async services and initializes 12 commands (6 Context menu + 6 Tools menu, split between Proto and DataAnno variants).
- **Commands** (`Commands/Context/`, `Commands/Menu/`) - Three command pairs per attribute family (Add, Renumber, Remove). Context commands operate on Solution Explorer selection; Menu commands operate on the open file.
- **Services** (`Services/`) - `ProtoAttributeService` and `DataAnnoAttributeService` via `IAttributeService`. Each wraps three parsers (Adder, Remover, Rewriter) and parses file content into a Roslyn `CSharpSyntaxTree`. Services hold a `Microsoft.VisualStudio.OLE.Interop.IServiceProvider` — this is why they stay in the VSIX project and not Core.
- **Parsers** (in Core) - Core Roslyn `CSharpSyntaxRewriter` subclasses:
  - `Parsers/ProtoContracts/BaseProtoRewriter.cs` - Abstract base; handles class/enum declarations and `using` insertion, tracks `StartIndex`. Saves/restores `StartIndex` around nested class visits so inner class counters do not bleed into outer class.
  - `ProtoAttributeAdder.cs` - Visits property/enum-member declarations to add missing attributes. Skips `static` and expression-bodied (`=>`) properties.
  - `ProtoAttributeReader.cs` - Walks tree to find the highest existing index. Uses `_classDepth` guard to avoid counting properties inside nested classes.
  - `ProtoAttributeRewriter.cs` - Renumbers existing attributes sequentially.
  - `ProtoAttributeRemover.cs` - Removes all Proto* attributes and usings.
  - Mirror classes under `Parsers/DataContracts/` for DataMember support.
  - `Parsers/NodeHelper.cs` - Static helpers for Roslyn attribute matching and using-directive insertion.
  - `Parsers/TriviaMaintainer.cs` - Preserves leading/trailing whitespace trivia during node mutation.
- **Executors** (`Executors/`) - `AttributeExecutor` iterates `SelectedItems` in Solution Explorer; `TextSelectionExecutor` applies changes to a `TextSelection`.

### Test Project

`visual-studio/ProtoAttributor.Tests/` - xUnit targeting .NET 8, assertions via Shouldly. `TestFixure` (note: intentional typo in existing code) provides `LoadTestFile(relativePath)` and `AssertOutputContainsCount(string[] source, string searchTerm, int numOfTimes)`.

Bug-proving tests live in:
- `ProtoContracts/ProtoAttributeAdderBugTests.cs`
- `DataContracts/DataAttributeReaderBugTests.cs`

## CI / CD (`/.github/workflows/`)

**`node.js.yml`** — VS Code extension (ubuntu-latest):
- Runs Jest coverage (enforces 70% threshold)
- On push to main: sets version to `1.0.${{ github.run_number }}` via `npm version`, then publishes via `vsce` using `VSCE_PAT` secret

**`dotnet.yml`** — VS extension (windows-latest):
- Sets VSIX manifest version to `1.2.${{ github.run_number }}` via `VsixVersionAction`
- MSBuild builds the VSIX; `dotnet test` runs tests (Core builds standalone — no `--no-build` needed)
- On push to main: publishes `.vsix` via `VsixPublisher.exe` (located via `vswhere`) using `VS_MARKETPLACE_PAT` secret
- Publish manifest: `visual-studio/publishManifest.json`

## Key Symmetry

Both extensions define the same attribute/using name constants in their respective `Constants.cs` / `constants.ts` files. When adding support for a new attribute family, update both.
