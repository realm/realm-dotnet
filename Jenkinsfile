#!groovy

@Library('realm-ci') _

configuration = 'Release'

def AndroidABIs = [ 'armeabi-v7a', 'arm64-v8a', 'x86', 'x86_64' ]
def WindowsPlatforms = [ 'Win32', 'x64' ]
def WindowsUniversalPlatforms = [ 'Win32', 'x64', 'ARM' ]

String versionSuffix = ''

stage('Checkout') {
  nodeWithCleanup('docker-cph-01') {
    def test_runner_image = docker.image('mcr.microsoft.com/dotnet/core/sdk:2.1')
    test_runner_image.pull()

    withRealmCloud("test_server-0ed2349a36352666402d0fb2e8763ac67731768c-race") { rc ->
      test_runner_image.inside("--link ${rc.id}:rc") {
        sh 'echo $RC_PORT_9080_TCP_ADDR:$RC_PORT_9080_TCP_PORT'
      }
    }
  }
}

def nodeWithCleanup(String label, Closure steps) {
  node(label) {
    echo "Running job on ${env.NODE_NAME}"

    // compute a shorter workspace name by removing the UUID at the end
    def terminus = env.WORKSPACE.lastIndexOf('-')
    def at = env.WORKSPACE.lastIndexOf('@')
    def workspace = env.WORKSPACE.substring(0, terminus)
    if (at > 0)
      workspace += env.WORKSPACE.drop(at)

    ws(workspace) {
      try {
        steps()
      } finally {
        deleteDir()
      }
    }
  }
}

def withRealmCloud(String version, block = { it }) {
  docker.withRegistry("https://${env.DOCKER_REGISTRY}", "ecr:eu-west-1:aws-ci-user") {
    // run image, get IP
    docker.image("${env.DOCKER_REGISTRY}/ci/mongodb-realm-images:${version}")
      .withRun("--name rc") { obj ->
        block(obj)
    }
  }
}