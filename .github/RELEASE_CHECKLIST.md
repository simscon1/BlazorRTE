# Release Checklist

Use this checklist when preparing and executing a release.

## Pre-Release Preparation

- [ ] Review and merge all PRs intended for this release
- [ ] Update version number in `BlazorRTE.csproj`
- [ ] Review and update documentation if needed
- [ ] Test the package locally:
  ```bash
  dotnet clean
  dotnet build --configuration Release
  dotnet pack --configuration Release --output ./artifacts
  ```
- [ ] Review CHANGELOG or prepare release notes
- [ ] Ensure all CI builds are passing on main branch

## One-Time Setup (First Release Only)

- [ ] Create NuGet API key at https://www.nuget.org/account/apikeys
- [ ] Add `NUGET_API_KEY` as a GitHub repository secret:
  - Go to Settings > Secrets and variables > Actions
  - Create new secret named `NUGET_API_KEY`
  - Paste your NuGet API key

## Release Execution

### Automated Release (Recommended)

- [ ] Commit version change:
  ```bash
  git add BlazorRTE.csproj
  git commit -m "Bump version to X.Y.Z"
  git push
  ```

- [ ] Create and push tag:
  ```bash
  git tag -a vX.Y.Z -m "Release version X.Y.Z"
  git push origin vX.Y.Z
  ```

- [ ] Monitor GitHub Actions:
  - Go to Actions tab in GitHub
  - Watch for "Release and Publish to NuGet" workflow
  - Verify all steps complete successfully

- [ ] Verify automated GitHub Release was created

### Manual Release (If Needed)

- [ ] Build and pack:
  ```bash
  dotnet clean
  dotnet build --configuration Release
  dotnet pack --configuration Release --output ./artifacts
  ```

- [ ] Push to NuGet:
  ```bash
  dotnet nuget push ./artifacts/BlazorRTE.X.Y.Z.nupkg \
    --api-key YOUR_API_KEY \
    --source https://api.nuget.org/v3/index.json
  ```

- [ ] Create Git tag:
  ```bash
  git tag -a vX.Y.Z -m "Release version X.Y.Z"
  git push origin vX.Y.Z
  ```

- [ ] Create GitHub Release manually:
  - Go to Releases > Draft a new release
  - Select the tag
  - Use template from `.github/RELEASE_TEMPLATE.md`
  - Attach `.nupkg` file if desired
  - Publish release

## Post-Release Verification

- [ ] Verify package appears on NuGet.org:
  https://www.nuget.org/packages/BlazorRTE/X.Y.Z

- [ ] Test installation in a sample project:
  ```bash
  dotnet new blazor -n TestApp
  cd TestApp
  dotnet add package BlazorRTE --version X.Y.Z
  dotnet build
  ```

- [ ] Verify GitHub Release is published with correct notes:
  https://github.com/simscon1/BlazorRTE/releases

- [ ] Check that release assets are attached (if applicable)

## Post-Release Communication (Optional)

- [ ] Update project website (if applicable)
- [ ] Announce on social media (Twitter, LinkedIn, etc.)
- [ ] Post in relevant communities (Reddit, Discord, etc.)
- [ ] Send email to interested users/customers
- [ ] Update any external documentation or tutorials

## Troubleshooting

If something goes wrong:

1. **Check GitHub Actions logs** for detailed error messages
2. **Review RELEASE.md** troubleshooting section
3. **Common issues**:
   - Version mismatch: Tag version must match csproj version
   - NuGet API key: Check it's correctly set in GitHub secrets
   - Package exists: Cannot overwrite - increment version
   - Build fails: Ensure code compiles locally first

## Notes

- Version format: `vMAJOR.MINOR.PATCH` (e.g., v1.0.1)
- Follow Semantic Versioning: https://semver.org/
- Cannot delete or overwrite NuGet packages once published
- GitHub releases can be edited after creation if needed

---

**After completing the release, delete or archive this checklist for the next release.**
