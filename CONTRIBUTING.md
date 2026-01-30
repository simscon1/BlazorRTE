# Contributing to BlazorRTE

Thank you for your interest in contributing to BlazorRTE! This document provides guidelines and instructions for contributing.

## Table of Contents
- [Code of Conduct](#code-of-conduct)
- [How to Contribute](#how-to-contribute)
- [Development Setup](#development-setup)
- [Making Changes](#making-changes)
- [Pull Request Process](#pull-request-process)
- [Release Process](#release-process)

## Code of Conduct

By participating in this project, you agree to maintain a respectful and collaborative environment.

## How to Contribute

There are many ways to contribute:

- **Report Bugs**: Open an issue describing the bug and how to reproduce it
- **Suggest Features**: Open an issue describing your feature request
- **Submit Code**: Fix bugs or implement features via pull requests
- **Improve Documentation**: Help improve README, docs, or code comments
- **Test**: Test the component in different scenarios and report issues

## Development Setup

### Prerequisites
- .NET 10.0 SDK or later
- Git
- A code editor (Visual Studio, VS Code, or Rider)

### Clone the Repository

```bash
git clone https://github.com/simscon1/BlazorRTE.git
cd BlazorRTE
```

### Build the Project

```bash
dotnet restore
dotnet build
```

### Test Local Changes

To test your changes in a Blazor application:

1. Build the package locally:
   ```bash
   dotnet pack --configuration Debug --output ./local-packages
   ```

2. Reference the local package in your test project:
   ```xml
   <ItemGroup>
     <PackageReference Include="BlazorRTE" Version="1.0.0" />
   </ItemGroup>
   ```

3. Add a local package source:
   ```bash
   dotnet nuget add source /path/to/BlazorRTE/local-packages
   ```

## Making Changes

### Branch Naming

Use descriptive branch names:
- `feature/description` - For new features
- `bugfix/description` - For bug fixes
- `docs/description` - For documentation updates

### Code Style

- Follow existing code style and conventions
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Keep methods focused and concise

### Commit Messages

Write clear commit messages:
- Use present tense ("Add feature" not "Added feature")
- Use imperative mood ("Move cursor to..." not "Moves cursor to...")
- First line should be 50 characters or less
- Reference issues when applicable (#123)

Example:
```
Add keyboard shortcut for strikethrough

- Add Ctrl+Shift+X shortcut
- Update documentation
- Fixes #123
```

## Pull Request Process

1. **Create a Branch**: Create a branch for your changes
   ```bash
   git checkout -b feature/my-new-feature
   ```

2. **Make Changes**: Make your changes and commit them
   ```bash
   git add .
   git commit -m "Add my new feature"
   ```

3. **Push Changes**: Push your branch to GitHub
   ```bash
   git push origin feature/my-new-feature
   ```

4. **Open Pull Request**: 
   - Go to the repository on GitHub
   - Click "Pull requests" > "New pull request"
   - Select your branch
   - Fill in the PR template
   - Submit the PR

5. **Address Feedback**: 
   - Respond to review comments
   - Make requested changes
   - Push updates to your branch

6. **Merge**: Once approved, a maintainer will merge your PR

## Release Process

For maintainers creating releases, see the detailed [RELEASE.md](RELEASE.md) guide.

### Quick Release Steps

1. Update version in `BlazorRTE.csproj`
2. Commit and push the version change
3. Create and push a version tag:
   ```bash
   git tag -a v1.0.1 -m "Release version 1.0.1"
   git push origin v1.0.1
   ```
4. GitHub Actions will automatically build and publish the release

### Version Numbering

We follow [Semantic Versioning](https://semver.org/):
- **MAJOR**: Breaking changes
- **MINOR**: New features (backward compatible)
- **PATCH**: Bug fixes (backward compatible)

## License

By contributing to BlazorRTE, you agree that your contributions will be licensed under the GPL v3 License.

For commercial licensing questions, contact licensing@loneworx.com.

## Questions?

If you have questions about contributing:
- Open an issue for discussion
- Contact the maintainers via GitHub
- Email: licensing@loneworx.com

---

Thank you for contributing to BlazorRTE! 🎉
