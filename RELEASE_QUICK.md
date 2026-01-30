# Quick Release Guide

A quick reference for creating releases. For detailed instructions, see [RELEASE.md](RELEASE.md).

## Prerequisites

1. **NuGet API Key**: Store in GitHub Secrets as `NUGET_API_KEY`
   - Get from: https://www.nuget.org/account/apikeys
2. **GitHub Write Access**: Required for creating releases
3. **.NET 10.0 SDK**: Installed locally for manual releases

## Automated Release (Recommended)

1. **Update version in BlazorRTE.csproj**:
   ```xml
   <Version>1.0.1</Version>
   ```

2. **Commit and push**:
   ```bash
   git add BlazorRTE.csproj
   git commit -m "Bump version to 1.0.1"
   git push
   ```

3. **Create and push tag**:
   ```bash
   git tag -a v1.0.1 -m "Release version 1.0.1"
   git push origin v1.0.1
   ```

4. **Done!** GitHub Actions will:
   - Build the project
   - Create NuGet package
   - Publish to NuGet.org
   - Create GitHub Release

## Manual Release

If you need to release manually:

```bash
# 1. Clean and build
dotnet clean
dotnet build --configuration Release

# 2. Create package
dotnet pack --configuration Release --output ./artifacts

# 3. Publish to NuGet
dotnet nuget push ./artifacts/BlazorRTE.1.0.1.nupkg \
  --api-key YOUR_API_KEY \
  --source https://api.nuget.org/v3/index.json

# 4. Create tag
git tag -a v1.0.1 -m "Release version 1.0.1"
git push origin v1.0.1

# 5. Create GitHub Release manually via web UI
```

## Version Numbers

Follow [Semantic Versioning](https://semver.org/):
- **X**.y.z - MAJOR: Breaking changes
- x.**Y**.z - MINOR: New features
- x.y.**Z** - PATCH: Bug fixes

## Post-Release

- [ ] Verify on NuGet.org
- [ ] Test installation: `dotnet add package BlazorRTE --version 1.0.1`
- [ ] Check GitHub Release is published

## Troubleshooting

- **Version mismatch error**: Ensure tag version matches csproj version
- **NuGet push fails**: Check API key and version doesn't already exist
- **Build fails**: Ensure .NET 10.0 SDK is installed

---

For complete documentation, see [RELEASE.md](RELEASE.md).
