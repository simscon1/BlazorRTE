# Release Guide for BlazorRTE

This guide explains how to create a new release of BlazorRTE, including publishing to NuGet and creating GitHub releases.

## Table of Contents
- [Prerequisites](#prerequisites)
- [Release Process Overview](#release-process-overview)
- [Manual Release Process](#manual-release-process)
- [Automated Release Process](#automated-release-process)
- [Version Numbering](#version-numbering)
- [Post-Release Checklist](#post-release-checklist)

## Prerequisites

Before creating a release, ensure you have:

1. **NuGet API Key**: Required for publishing to NuGet.org
   - Get your API key from https://www.nuget.org/account/apikeys
   - Store it as a GitHub secret named `NUGET_API_KEY`

2. **GitHub Permissions**: Write access to the repository for creating releases

3. **Development Environment**: 
   - .NET 10.0 SDK installed
   - Git configured

## Release Process Overview

A complete release involves:
1. Updating the version number
2. Building and testing the package
3. Creating a Git tag
4. Publishing to NuGet.org
5. Creating a GitHub Release with release notes

## Manual Release Process

### Step 1: Update Version Number

1. Edit `BlazorRTE.csproj` and update the `<Version>` tag:
   ```xml
   <Version>1.0.1</Version>
   ```

2. Commit the version change:
   ```bash
   git add BlazorRTE.csproj
   git commit -m "Bump version to 1.0.1"
   git push
   ```

### Step 2: Build and Test

1. Clean previous builds:
   ```bash
   dotnet clean
   ```

2. Build the project:
   ```bash
   dotnet build --configuration Release
   ```

3. Create the NuGet package:
   ```bash
   dotnet pack --configuration Release --output ./artifacts
   ```

4. Verify the package contents:
   ```bash
   # On Windows
   nuget verify -All ./artifacts/BlazorRTE.1.0.1.nupkg
   
   # On Linux/Mac
   unzip -l ./artifacts/BlazorRTE.1.0.1.nupkg
   ```

### Step 3: Create Git Tag

Create and push a Git tag matching the version:

```bash
git tag -a v1.0.1 -m "Release version 1.0.1"
git push origin v1.0.1
```

### Step 4: Publish to NuGet

Push the package to NuGet.org:

```bash
dotnet nuget push ./artifacts/BlazorRTE.1.0.1.nupkg \
  --api-key YOUR_NUGET_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

**Note:** Replace `YOUR_NUGET_API_KEY` with your actual API key.

### Step 5: Create GitHub Release

1. Go to https://github.com/simscon1/BlazorRTE/releases/new
2. Select the tag you created (v1.0.1)
3. Set the release title (e.g., "BlazorRTE v1.0.1")
4. Write release notes describing changes
5. Optionally attach the `.nupkg` file as a release asset
6. Click "Publish release"

## Automated Release Process

The repository includes GitHub Actions workflows to automate releases.

### Using the Automated Workflow

1. **Update Version**: Edit `BlazorRTE.csproj` to update the version number

2. **Create and Push a Tag**:
   ```bash
   git tag -a v1.0.1 -m "Release version 1.0.1"
   git push origin v1.0.1
   ```

3. **Workflow Runs Automatically**: The GitHub Actions workflow will:
   - Build the project
   - Run tests (if any)
   - Create the NuGet package
   - Publish to NuGet.org
   - Create a GitHub Release

4. **Monitor Progress**: Check the Actions tab to monitor the release workflow

### Workflow Configuration

The release workflow is located at `.github/workflows/release.yml` and triggers on:
- Push of tags matching `v*.*.*` (e.g., v1.0.0, v1.2.3)

## Version Numbering

BlazorRTE follows [Semantic Versioning](https://semver.org/) (SemVer):

- **MAJOR.MINOR.PATCH** (e.g., 1.0.0)
  - **MAJOR**: Breaking changes or major new features
  - **MINOR**: New features, backward compatible
  - **PATCH**: Bug fixes, backward compatible

### Examples:
- `1.0.0` → `1.0.1`: Bug fix release
- `1.0.1` → `1.1.0`: New feature added
- `1.1.0` → `2.0.0`: Breaking change introduced

## Post-Release Checklist

After creating a release:

- [ ] Verify the package appears on NuGet.org: https://www.nuget.org/packages/BlazorRTE
- [ ] Test installation in a sample project:
  ```bash
  dotnet add package BlazorRTE --version 1.0.1
  ```
- [ ] Verify the GitHub release is published with correct notes
- [ ] Announce the release (if applicable):
  - Update project website
  - Post on social media
  - Notify users/customers
- [ ] Update the main branch to the next development version if needed

## Troubleshooting

### NuGet Push Fails

**Error**: `The API key 'xxx' is invalid`
- **Solution**: Verify your API key is correct and has not expired. Generate a new one if needed.

**Error**: `Package already exists`
- **Solution**: You cannot overwrite a package version. Increment the version number.

### Tag Already Exists

**Error**: `tag 'v1.0.0' already exists`
- **Solution**: Delete the tag if you need to recreate it:
  ```bash
  git tag -d v1.0.0
  git push origin :refs/tags/v1.0.0
  ```

### Build Fails

- Check that you have .NET 10.0 SDK installed
- Ensure all dependencies are restored: `dotnet restore`
- Review build errors and fix any compilation issues

## Support

For questions or issues with releases:
- **GitHub Issues**: https://github.com/simscon1/BlazorRTE/issues
- **Email**: licensing@loneworx.com

---

**Last Updated**: January 2026
