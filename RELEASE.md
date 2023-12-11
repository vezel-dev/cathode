# Release

Every major/minor release should come with its own `release/vX.Y` branch. To
create this branch, run `dotnet nbgv prepare-release` from the `master` branch.
Pass `--versionIncrement major` if the version being developed after this
release is going to involve a major version bump.

Patch-level releases should be done out of the relevant major/minor branch. For
example, both `1.0.1` and `1.0.5` should come out of `release/v1.0`. So, there
is no need to run `dotnet nbgv prepare-release` in this case.

Before tagging a release, run `./cake` in the release branch locally on all
platforms that you have access to, and test the [sample programs](src/samples)
with all terminal emulators that you have access to. Verify that nothing has
regressed. Also, ensure that the release branch builds and tests successfully on
[CI](https://github.com/vezel-dev/cathode/actions).

Next, run `dotnet nbgv tag` from the release branch to create a release tag,
followed by `git tag <tag> <tag> -f -m <tag> -s` to sign it, and then push it
with `git push origin <tag>`. Again, wait for CI to build and test the tag. If
something goes wrong on CI, you can run `git tag -d <tag>` and
`git push origin :<tag>` to delete the tag until you resolve the issue(s), and
then repeat this step.

Finally, to actually publish the release, go to the
[releases page](https://github.com/vezel-dev/cathode/releases) to create a
release from the tag you pushed, ideally with some well-written release notes.
Once the release is published, a workflow will build and publish NuGet packages
from the tag.
