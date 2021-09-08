# Post a release to Slack

This action will create a slack post for a release.

## Assumptions

* Changelog is in the format `## x.y.z-maybe.a (*some-date*)`.
* There should be only one section in the changelog (i.e. should be equivalent to what is getting posted in the Github release)
* Github release should be tagged as the version - i.e. `x.y.z-maybe.a`

## Usage

```yaml
- name: Post to slack
  id: release-to-slack
  uses: realm/ci-actions/release-to-slack@v3
  with:
    changelog: ${{ github.workspace }}/RELEASE-NOTES.md
    sdk: .NET
    webhook-url: ${{ secrets.SLACK_WEBHOOK_URL }}
    version: '10.2.3'
```

The action takes the following parameters:

1. *(Required)* `changelog`: the path to the top section of CHANGELOG.md.
1. *(Required)* `sdk`: friendly name for the SDK.
1. *(Required)* `webhook-url`: url for the slack integration.
1. *(Required)* `version`: the version that is being released

[![GitHub release badge](https://badgen.net/github/release/realm/ci-actions/run-ios-simulator)](https://github.com/realm/ci-actions/releases/latest)
