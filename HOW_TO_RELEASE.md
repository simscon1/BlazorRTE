# How to Create a New Release - Summary

This document summarizes the release process documentation and automation that has been set up for BlazorRTE.

## What Was Created

### Documentation Files
1. **RELEASE.md** - Comprehensive release guide with detailed instructions
2. **RELEASE_QUICK.md** - Quick reference for common release tasks
3. **CONTRIBUTING.md** - Contributing guidelines including release process
4. **Updated README.md** - Added links to release and contributing documentation

### GitHub Actions Workflows
1. **.github/workflows/release.yml** - Automated release and NuGet publishing
2. **.github/workflows/ci.yml** - Continuous integration builds

## How to Create a Release (Quick Answer)

### Automated Method (Recommended)

1. **Update the version** in `BlazorRTE.csproj`:
   ```xml
   <Version>1.0.1</Version>
   ```

2. **Commit and push** the version change:
   ```bash
   git add BlazorRTE.csproj
   git commit -m "Bump version to 1.0.1"
   git push
   ```

3. **Create and push a tag**:
   ```bash
   git tag -a v1.0.1 -m "Release version 1.0.1"
   git push origin v1.0.1
   ```

4. **Done!** The GitHub Actions workflow will automatically:
   - Build the project
   - Create the NuGet package
   - Publish to NuGet.org (requires `NUGET_API_KEY` secret)
   - Create a GitHub Release

## Prerequisites

### One-Time Setup

1. **Get NuGet API Key**:
   - Go to https://www.nuget.org/account/apikeys
   - Create a new API key with push permissions
   - Copy the key

2. **Add GitHub Secret**:
   - Go to your repository Settings > Secrets and variables > Actions
   - Click "New repository secret"
   - Name: `NUGET_API_KEY`
   - Value: Paste your NuGet API key
   - Click "Add secret"

## Documentation Reference

- **Full Details**: See [RELEASE.md](RELEASE.md)
- **Quick Reference**: See [RELEASE_QUICK.md](RELEASE_QUICK.md)
- **Contributing**: See [CONTRIBUTING.md](CONTRIBUTING.md)

## Workflow Details

### Release Workflow
- **Trigger**: Push of tags matching `v*.*.*` (e.g., v1.0.0, v1.2.3)
- **Actions**:
  1. Checkout code
  2. Setup .NET 10.0
  3. Extract version from tag
  4. Verify version matches csproj
  5. Restore dependencies
  6. Build project
  7. Pack NuGet package
  8. Publish to NuGet.org
  9. Create GitHub Release

### CI Workflow
- **Trigger**: Push to `main` or `develop` branches, or pull requests
- **Actions**: Build and pack (validation only, no publishing)

## Troubleshooting

### Common Issues

1. **"Version mismatch" error**
   - Ensure the tag version (e.g., v1.0.1) matches the version in BlazorRTE.csproj (1.0.1)

2. **NuGet publish fails**
   - Check that `NUGET_API_KEY` secret is set correctly
   - Verify the API key hasn't expired
   - Ensure the version doesn't already exist on NuGet.org

3. **Build fails**
   - Check the Actions tab for detailed error logs
   - Ensure the code builds locally before creating the tag

## Manual Release (If Needed)

If you prefer to release manually or if automation fails:

```bash
# 1. Build and create package
dotnet clean
dotnet build --configuration Release
dotnet pack --configuration Release --output ./artifacts

# 2. Publish to NuGet
dotnet nuget push ./artifacts/BlazorRTE.1.0.1.nupkg \
  --api-key YOUR_API_KEY \
  --source https://api.nuget.org/v3/index.json

# 3. Create tag and GitHub Release manually
git tag -a v1.0.1 -m "Release version 1.0.1"
git push origin v1.0.1
# Then create GitHub Release via web UI
```

## Next Steps

1. **Set up NuGet API Key** secret in GitHub (see Prerequisites above)
2. **Review** the documentation files created
3. **Test** the release process when ready to publish your next version

---

For questions or issues, refer to the full documentation in [RELEASE.md](RELEASE.md).
