# GitHub Copilot Agent Instructions

## Overview
This document provides instructions for GitHub Copilot when working on the MonoGameVoxelTestWindows project. These guidelines ensure code quality, maintainability, and consistency across the codebase.

---

## Branch Protection & Workflow

### Always Use Feature Branches
- **NEVER edit code directly on the `main` branch**
- Always create a new branch for any code changes, following these naming conventions:
  - `feature/` - for new features (e.g., `feature/player-inventory`)
  - `bugfix/` - for bug fixes (e.g., `bugfix/chunk-loading-crash`)
  - `refactor/` - for code refactoring (e.g., `refactor/simplify-raycast`)
- Create the branch before making any edits: `git checkout -b <branch-name>`

### Workflow Example
```bash
# Create a new branch
git checkout -b feature/new-block-types

# Make changes, then commit
git add .
git commit -m "Add new block types with textures"
```

---

## Testing Requirements

### Test-Driven Development
- **All new code must be testable** where appropriate
- Write unit tests for:
  - Business logic and algorithms (e.g., raycasting, pathfinding)
  - Data structures (e.g., chunks, inventories, world generators)
  - System components (e.g., block destruction, NPC behavior)
  - Utility classes and helper methods

### Testing Framework: xUnit
- Use `[Fact]` attribute for test methods
- Follow **Arrange-Act-Assert (AAA)** pattern:
  ```csharp
  [Fact]
  public void MethodName_Scenario_ExpectedResult()
  {
      // Arrange
      var testObject = new MyClass();
      
      // Act
      var result = testObject.DoSomething();
      
      // Assert
      Assert.Equal(expectedValue, result);
  }
  ```
- Use descriptive test names: `MethodName_Scenario_ExpectedResult`
- Create test helper classes as needed (e.g., `MockRandom` for deterministic testing)
- Keep tests self-contained - no shared state between tests

### Test Execution Mandate
- **BEFORE marking any task as complete, run all tests**:
  ```bash
  dotnet test
  ```
- Ensure all tests pass before committing changes
- If tests fail, fix the issues or update tests as needed
- Do not skip test execution - this is a mandatory step

### Testability Guidelines
- Use interfaces for dependencies (e.g., `IBlockAccessor`, `IWorldGenerator`, `IRandom`)
- Design classes to be testable through dependency injection
- Avoid static state and singletons where possible
- Keep methods focused and single-purpose

---

## Code Review Quality Checklist

Before completing any task, verify the following:

### Architecture & Design
- [ ] **Interface-based dependencies**: Use interfaces (`IBlockAccessor`, `IServiceContainer`, etc.) instead of concrete types
- [ ] **Dependency injection**: Register services properly in `ServiceContainer`
- [ ] **Single Responsibility**: Each class has one clear purpose
- [ ] **Sealed classes**: Use `sealed` keyword for classes not intended for inheritance

### Code Quality
- [ ] **Null checks**: Validate parameters and handle null cases appropriately
- [ ] **Error handling**: Use appropriate exception types and provide clear error messages
- [ ] **Resource cleanup**: Implement `IDisposable` for classes holding unmanaged resources
- [ ] **Immutability**: Use `readonly` fields and immutable data structures where appropriate
- [ ] **Value types**: Use structs/records for small data containers (e.g., coordinates)

### Code Style
- [ ] **File-scoped namespaces**: Use file-scoped namespace declarations
- [ ] **Descriptive naming**: Use clear, intention-revealing names for variables, methods, and classes
- [ ] **Consistent formatting**: Follow existing code style in the project
- [ ] **Remove unused code**: Delete commented-out code and unused imports

### Testing
- [ ] **Test coverage**: New/modified code has corresponding unit tests
- [ ] **Tests pass**: All tests execute successfully (`dotnet test`)
- [ ] **Test quality**: Tests are clear, maintainable, and test the right things

---

## Documentation Standards

### XML Documentation Comments
- **Required for ALL public members** (classes, methods, properties, interfaces)
- Use XML doc comments (`///`) to describe:
  - What the member does
  - Parameters and their purpose
  - Return values
  - Exceptions that may be thrown
  - Example usage for complex APIs

#### Example
```csharp
/// <summary>
/// Performs a raycast from the camera to find the first intersected block.
/// </summary>
/// <param name="origin">The starting position of the ray in world space.</param>
/// <param name="direction">The normalized direction vector of the ray.</param>
/// <param name="maxDistance">The maximum distance to check for intersections.</param>
/// <returns>A <see cref="RaycastHit"/> containing the hit information, or null if no block was hit.</returns>
/// <exception cref="ArgumentException">Thrown when direction is not normalized.</exception>
public RaycastHit? Raycast(Vector3 origin, Vector3 direction, float maxDistance)
{
    // Implementation...
}
```

### Inline Comments
- Use inline comments for complex logic that isn't immediately obvious:
  - Raycasting algorithms (DDA, step calculations)
  - Chunk coordinate conversions
  - Performance optimizations
  - Workarounds for known issues
- **Don't comment obvious code** - let the code speak for itself
- Explain **why**, not **what** (the code shows what, comments explain reasoning)

### Interface Documentation
- Document interface contracts clearly
- Specify expected behavior and constraints
- Document side effects and state changes

---

## MonoGame-Specific Best Practices

### Content Pipeline
- Use the **Content Pipeline** for loading assets (textures, models, fonts, effects)
- Assets should be added to `Content/Content.mgcb` and built through the MGCB Editor
- Load content in `LoadContent()` method, never in `Update()` or `Draw()`
- Reference content using content-relative paths: `Content.Load<Texture2D>("textureName")`

### Dispose Patterns
- **Always dispose of graphics resources** when no longer needed
- Implement `IDisposable` for classes holding:
  - `Texture2D`, `Effect`, `Model`, `RenderTarget2D`
  - `VertexBuffer`, `IndexBuffer`
  - Any `GraphicsResource` derived types
- Dispose resources in proper order (user resources before built-in resources)
- Use `using` statements or `try-finally` blocks to ensure disposal

#### Example
```csharp
public sealed class TextureAtlas : IDisposable
{
    private Texture2D _texture;
    
    public void Dispose()
    {
        _texture?.Dispose();
    }
}
```

### Game Loop Threading
- **Update() and Draw() run on the same thread** - no multi-threading by default
- Keep `Update()` and `Draw()` methods fast (< 16ms each for 60 FPS)
- `Update()` is for game logic only - **never render in Update()**
- `Draw()` is for rendering only - **never change game state in Draw()**
- Use `IsRunningSlowly` to detect performance issues

### Graphics State Management
- Reset graphics state after custom rendering (blend states, depth stencil, rasterizer state)
- Use `GraphicsDevice.Clear()` at the start of `Draw()`
- Set appropriate `SamplerState` for texture filtering (e.g., `PointClamp` for pixel art)

### Common Patterns
- Use `BasicEffect` or custom `Effect` for rendering
- Create vertex buffers for dynamic geometry (chunk meshes)
- Batch draw calls to minimize state changes
- Use `SpriteBatch` for 2D rendering (UI, debug text)

---

## Project-Specific Patterns

### Chunk-Based Architecture
- World is divided into 4x4x4 chunks for efficient memory and rendering
- Use `ChunkManager` to coordinate chunk operations
- Mark chunks as dirty when blocks change (requires mesh rebuild)
- Implement proper chunk boundary handling

### Block Access Patterns
- Use `IBlockAccessor` interface for all world access
- `ArrayWorld` - base world layer (terrain generation)
- `DestructibleBlockLayer` - overlay for modified blocks
- `CompositeBlockAccessor` - combines both layers transparently
- Coordinate conversions: world → chunk → local block coordinates

### Dependency Injection
- Use `ServiceContainer` (implements `IServiceContainer`) for service registration
- Register services at startup in `Game1` constructor or `Initialize()`
- Access services through interface types, not concrete implementations
- Avoid service locator anti-pattern - prefer constructor injection

### Interface Segregation
- Keep interfaces small and focused
- Examples: `ICameraInput`, `IWorldGenerator`, `IRandom`, `IBlockAccessor`
- Mock interfaces in tests for isolated unit testing

---

## Common Tasks Checklist

### Adding a New Feature
1. [ ] Create a feature branch (`feature/...`)
2. [ ] Design the feature with interfaces if it involves dependencies
3. [ ] Implement the feature with proper error handling
4. [ ] Write XML documentation for all public members
5. [ ] Write unit tests covering normal and edge cases
6. [ ] Run `dotnet test` and ensure all tests pass
7. [ ] Verify code quality checklist items
8. [ ] Commit with descriptive message

### Fixing a Bug
1. [ ] Create a bugfix branch (`bugfix/...`)
2. [ ] Write a failing test that reproduces the bug (if possible)
3. [ ] Fix the bug with minimal changes
4. [ ] Ensure the test now passes
5. [ ] Run `dotnet test` to verify no regressions
6. [ ] Document any non-obvious changes
7. [ ] Commit with reference to the issue

### Refactoring Code
1. [ ] Create a refactor branch (`refactor/...`)
2. [ ] Ensure existing tests cover the code being refactored
3. [ ] Make incremental refactoring changes
4. [ ] Run `dotnet test` after each significant change
5. [ ] Update documentation if public APIs changed
6. [ ] Verify code still follows project patterns
7. [ ] Commit with clear explanation of changes

---

## Summary

**Remember the three pillars:**
1. 🌿 **Branch Protection**: Always work on feature branches, never directly on `main`
2. ✅ **Testing**: Write tests, run `dotnet test`, ensure all tests pass before completing
3. 📝 **Documentation**: Document all public APIs with XML comments

**When in doubt:**
- Make code testable through interfaces and dependency injection
- Follow existing patterns in the codebase
- Keep MonoGame best practices in mind (dispose resources, separate Update/Draw)
- Run tests before marking any task complete

---

*Last Updated: January 14, 2026*
