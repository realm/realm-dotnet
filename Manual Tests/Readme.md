# Running manual/integration tests

The NuGetReleaseTests project is a clone of the regular test projects that references the NuGet packages rather than the local projects.

## Setting up ROS

1. Download the latest [public ROS](https://realm.io/products/realm-mobile-platform/).
1. Run `start-object-server.command` and create an admin user `a@a` with password `a`.
1. Edit `configuration.yml`:
  1. In the `https` section:
    1. `enable: true`
    1. `certificate_path: 'keys/127_0_0_1-chain.crt.pem'`
    1. `private_key_path: 'keys/127_0_0_1-server.key.pem'`
1. Copy `Tests/ROS/keys` to `path-to-ros/realm-object-server/object-server`

## Setting up Android

You need Android 5.0+ device to run all unit tests.
1. Execute `adb reverse tcp:9080 tcp:9080`
1. Execute `adb reverse tcp:9443 tcp:9443`

## Setting up iOS

If you're running a simulator, no additional setup is required.
If you're running tests on a device, you need to update `Constants.ServerUrl` with the url of your server (make sure it's reachable from the device).