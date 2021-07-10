# Prepare a new release

This action will prepare a new release and open a draft PR for it.

## Picking a version

It will try to automatically pick the correct version based on the content of the changelog. If there are any breaking changes, it will pick a major version bump. If there are enhancements, it'll bump the minor version, and if there are only bug fixes, it'll be a patch version bump.

## Modified files

1. Changelog.md: the latest release will be assigned the correct version and today's date.
2. Realm/AssemblyInfo.props: the version will be updated
3. Realm/Realm.Unity/package.json: the version will be updated