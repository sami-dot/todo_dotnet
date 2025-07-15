pipeline {
    agent any

    environment {
        GIT_CREDENTIALS = 'github-token'      // Your Jenkins GitHub credentials ID
        NEXUS_CREDENTIALS = 'nexus-creds'     // Your Jenkins Nexus credentials ID
        NEXUS_URL = 'http://192.168.47.158:8081/repository/dotnet-artifacts/'
        PROJECT_NAME = 'TodoApi'
        DOTNET_VERSION = '8.0' 
        CONNECTION_STRING = 'Server=192.168.47.76;Database=TodoDb;User Id=sa;Password=@Admin1234!;Encrypt=False;TrustServerCertificate=True;'
        DEPLOY_DIR = '/var/lib/jenkins/todoapi'  // Deployment folder on your VM

    }

    stages {
        stage('Checkout') {
            steps {
                git branch: 'main', credentialsId: "${GIT_CREDENTIALS}", url: 'https://github.com/sami-dot/todo_dotnet.git'
            }
        }
        stage('Restore') {
            steps {
                sh "dotnet restore"
            }
        }

        stage('Build') {
            steps {
                sh "dotnet build --configuration Release"
            }
        }
        
       stage('Apply EF Core Migrations') {
            steps {
                sh '''
                    echo 🧱 Applying EF Core migrations...

                    # Define tools path
                    DOTNET_TOOLS="$HOME/.dotnet/tools"
                    export PATH="$PATH:$DOTNET_TOOLS"

                    # Install dotnet-ef if not present
                    dotnet tool install --global dotnet-ef || true

                    # Run migration using full path to dotnet-ef
                    "$DOTNET_TOOLS/dotnet-ef" database update --connection "Server=192.168.47.76;Database=TodoDb;User Id=sa;Password=@Admin1234!;Encrypt=False;TrustServerCertificate=True;"
                '''
            }
        }

        stage('Migrate Database') {
            steps {
                sh '''#!/bin/bash
      DOTNET_TOOLS=/var/lib/jenkins/.dotnet/tools
      export PATH=$PATH:$DOTNET_TOOLS
      dotnet-ef database update --connection "${CONNECTION_STRING}"
        '''
            }
        }
        stage('Publish') {
            steps {
                sh "dotnet publish -c Release -o publish"
            }
        }

        stage('Package') {
            steps {
                sh "zip -r ${PROJECT_NAME}.zip publish/"
            }
        }

        stage('Upload to Nexus') {
            steps {
                script {
                    withCredentials([usernamePassword(credentialsId: "${NEXUS_CREDENTIALS}", usernameVariable: 'NEXUS_USER', passwordVariable: 'NEXUS_PASS')]) {
                        sh """
                        curl -v -u $NEXUS_USER:$NEXUS_PASS --upload-file ${PROJECT_NAME}.zip ${NEXUS_URL}${PROJECT_NAME}-${env.BUILD_NUMBER}.zip
                        """
                    }
                }
            }
        }
        stage('Deploy on VM') {
            steps {
                script {
                    // Compose Nexus artifact URL with build number
                    def artifactUrl = "${NEXUS_URL}${PROJECT_NAME}-${env.BUILD_NUMBER}.zip"
                    def deployDir = "${DEPLOY_DIR}"

                    sh """
                    echo "Cleaning deployment directory..."
                    rm -rf ${deployDir}
                    mkdir -p ${deployDir}

                    echo "Downloading artifact from Nexus..."
                    wget -O ${deployDir}/${PROJECT_NAME}.zip "${artifactUrl}"

                    echo "Unzipping artifact..."
                    unzip -o ${deployDir}/${PROJECT_NAME}.zip -d ${deployDir}

                    echo "Stopping any running instances of the app..."
                    pkill -f ${PROJECT_NAME}.dll || true

                    echo "Starting the app in background..."
                    nohup dotnet ${deployDir}/${PROJECT_NAME}.dll > ${deployDir}/app.log 2>&1 &
                    """
                }
            }
        }
    }
}
