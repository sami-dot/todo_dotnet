pipeline {
    agent any

    environment {
        GIT_CREDENTIALS = 'github-token'      // Your Jenkins GitHub credentials ID
        NEXUS_CREDENTIALS = 'nexus-creds'     // Your Jenkins Nexus credentials ID
        NEXUS_URL = 'http://192.168.47.158:8081/repository/dotnet-artifacts/'
        PROJECT_NAME = 'TodoApi'
        DOTNET_VERSION = '8.0' 
        CONNECTION_STRING = 'Server=localhost;Database=TodoDb;User Id=sa;Password=@Admin1234!;'

    }

    stages {
        stage('Checkout') {
            steps {
                git credentialsId: "${GIT_CREDENTIALS}", url: 'https://github.com/sami-dot/todo_dotnet.git'
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
                    dotnet tool install --global dotnet-ef || true
                    export PATH="$PATH:/root/.dotnet/tools"
                    dotnet ef database update --connection "$CONNECTION_STRING"
                '''
            }
        }
        stage('Migrate Database') {
            steps {
                // Assuming your project has migrations configured
                sh "dotnet ef database update"
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
    }
}
