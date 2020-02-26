#!groovy

@Library('realm-ci') _

configuration = 'Release'

def AndroidABIs = [ 'armeabi-v7a', 'arm64-v8a', 'x86', 'x86_64' ]
def WindowsPlatforms = [ 'Win32', 'x64' ]
def WindowsUniversalPlatforms = [ 'Win32', 'x64', 'ARM' ]

String versionSuffix = ''

stage('Checkout') {
  nodeWithCleanup('docker-cph-03') {
    checkout([
      $class: 'GitSCM',
      branches: scm.branches,
      gitTool: 'native git',
      extensions: scm.extensions + [
        [$class: 'CloneOption', depth: 0, shallow: true],
        [$class: 'SubmoduleOption', recursiveSubmodules: true]
      ],
      userRemoteConfigs: scm.userRemoteConfigs
    ])

    withRealmCloud("test_server-0ed2349a36352666402d0fb2e8763ac67731768c-race") { rc ->
      def test_runner_image = docker.image('mcr.microsoft.com/dotnet/core/sdk:2.1')
      test_runner_image.pull()
      test_runner_image.inside("--link ${rc.id}:rc") {
        sh 'echo $RC_PORT_9090_TCP_ADDR:$RC_PORT_9090_TCP_PORT'
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
      .withRun { rc ->
        docker.image("${env.DOCKER_REGISTRY}/ci/stitch-cli:190").inside("-v $WORKSPACE/.config/stitch:/root/.config/stitch --link ${rc.id}:rc") {
          sh 'echo $RC_PORT_9090_TCP_ADDR:$RC_PORT_9090_TCP_PORT'
          def access_token = sh(
            script: 'curl --request POST --header "Content-Type: application/json" --data \'{ "username":"unique_user@domain.com", "password":"password" }\' http://$RC_PORT_9090_TCP_ADDR:$RC_PORT_9090_TCP_PORT/api/admin/v3.0/auth/providers/local-userpass/login -s | jq ".access_token" -r',
            returnStdout: true
          ).trim()

          echo "token: $access_token"

          def group_id = sh(
            script: "curl --header 'Authorization: Bearer $access_token' http://\$RC_PORT_9090_TCP_ADDR:\$RC_PORT_9090_TCP_PORT/api/admin/v3.0/auth/profile -s | jq '.roles[0].group_id' -r",
            returnStdout: true
          ).trim()

          echo "group id: $group_id"

          sh """
            /usr/bin/stitch-cli login --base-url=http://\$RC_PORT_9090_TCP_ADDR:\$RC_PORT_9090_TCP_PORT --auth-provider=local-userpass --username=unique_user@domain.com --password=password
            /usr/bin/stitch-cli import --app-name some-app --app-id some-app-yxmfy --project-id $group_id --base-url=http://\$RC_PORT_9090_TCP_ADDR:\$RC_PORT_9090_TCP_PORT -y --strategy replace
          """
        }
        block(rc)
    }
  }
}