pipeline {
    agent any
    
    environment {
        SCANNER_HOME = tool 'sonar-scanner'
        IMAGE_TAG = "v${BUILD_NUMBER}"
        TRIVY_CACHE_DIR = "/var/lib/jenkins/trivy-cache"
        TRIVY_DB_REPOSITORY = "ghcr.io/aquasecurity/trivy-db"
        NUGET_PACKAGES = "/var/lib/jenkins/.nuget/packages"
        NUGET_HTTP_TIMEOUT = "900"
        SONAR_SCANNER_OPTS = "-Dsonar.scanner.skipJreProvisioning=true"
        DOCKER_BUILDKIT = '1'
    
    }
    stages {
        stage('Git Checkout') {
            steps {
                git branch: 'main', credentialsId: 'git-cred', url: 'https://github.com/Nillz24/capstone-dotnet-mongo-jenkin-ci.git'
            }
        }
        stage('Gitleaks Scan') {
            steps {
             sh 'gitleaks detect --report-format=json --report-path=gitleaks-report.json --exit-code=1'
            }
        }
        stage('Compile') {
            steps {
                sh 'dotnet build'
            }
        }
        stage('trivy FS Scan') {
            steps {
                
              sh '''trivy fs --cache-dir $TRIVY_CACHE_DIR --format table -o trivy-fs-report.html --timeout 130m .'''
            }
        }
        stage('Restore') {
            steps {
                retry(3) {
                    sh 'dotnet restore Tests/NoteApp.Tests.csproj --disable-parallel'
                }
            }
        }

        stage('Test') {
            steps {
                sh 'dotnet test Tests/NoteApp.Tests.csproj --no-restore'
            }
        }
        stage('SonarQube Analysis') {
            steps {
                withSonarQubeEnv('sonar') {
                    sh ''' ${SCANNER_HOME}/bin/sonar-scanner \
                            -Dsonar.projectKey=NoteApp \
                            -Dsonar.projectName=NoteApp \
                            -Dsonar.host.url=http://localhost:9000 \
                            -Dsonar.token=${SONAR_AUTH_TOKEN} \
                            -Dsonar.scanner.skipJreProvisioning=true \
                            -Dsonar.scanner.socketTimeout=3000 '''
                }
            }
        }
        
        stage('Quality Gate Check') {
            steps {
                timeout(time: 1, unit: 'HOURS') {
                     waitForQualityGate abortPipeline: false, credentialsId: 'sonar-token'
                    }
            }
        }
        
        stage('Build Image & Tag Image') {
            steps {
                script {
                        def CURRENT_TAG = "v${env.BUILD_NUMBER}"
                        def PREV_TAG = env.BUILD_NUMBER.toInteger() > 1 ? "v${env.BUILD_NUMBER.toInteger() - 2}" : ""
                    withDockerRegistry(credentialsId: 'docker-token') {
                        sh """
                            echo "Current tag: ${CURRENT_TAG}"
                            echo "Previous tag: ${PREV_TAG}"

                            # Try pulling previous image for cache
                            docker pull nillz26/noteapp:${PREV_TAG} || true

                            # Build with cache
                            docker build \
                            --cache-from=nillz26/noteapp:${PREV_TAG} \
                            -t nillz26/noteapp:${CURRENT_TAG} \
                            . """

                    }
                }
            }
        }
        
        stage('trivy Image Scan') {
            steps {
              sh 'trivy image --cache-dir $TRIVY_CACHE_DIR--format table -o trivy-image-report.html --timeout 130m nillz26/noteapp:$IMAGE_TAG'
            }
        }
        
        stage('Push Image') {
            steps {
                script {
                    withDockerRegistry(credentialsId: 'docker-token') {
                        sh "docker push nillz26/noteapp:$IMAGE_TAG"
                    }
                }
            }
        }
        /*
        stage('Update Manifest File CD Repo') {
            steps {
                script {
                    cleanWs()
                    withCredentials([usernamePassword(credentialsId: 'git-cred', passwordVariable: 'GIT_PASSWORD', usernameVariable: 'GIT_USERNAME')]) {
                        sh '''
                            # Clone the CD Repo
                            git clone https://${GIT_USERNAME}:${GIT_PASSWORD}@github.com/Nillz24/capstone-dotnet-mongo-jenkin-ci.git
                            
                            # Update the tag in manifest
                            cd Capstone-DotNET-Mongo-CD
                            sed -i "s|nillz26/noteapp:.*|nillz26/noteapp:${IMAGE_TAG}|" Manifest/manifest.yaml
                            
                            # Confirm Changes
                            echo "Updated manifest file contents:"
                            cat Manifest/manifest.yaml
                            
                            # Commit and push the changes
                            git config user.name "Jenkins"
                            git config user.email "jenkins@example.com"
                            git add Manifest/manifest.yaml
                            git commit -m "Update image tag to ${IMAGE_TAG}"
                            git push origin main
                        '''
                    }
                    
                }
            }
        } */
    }
    post {
    always {
        script {
            def jobName = env.JOB_NAME
            def buildNumber = env.BUILD_NUMBER
            def pipelineStatus = currentBuild.result ?: 'UNKNOWN'
            def bannerColor = pipelineStatus.toUpperCase() == 'SUCCESS' ? 'green' : 'red'

            def body = """
                <html>
                <body>
                <div style="border: 4px solid ${bannerColor}; padding: 10px;">
                <h2>${jobName} - Build ${buildNumber}</h2>
                <div style="background-color: ${bannerColor}; padding: 10px;">
                <h3 style="color: white;">Pipeline Status: ${pipelineStatus.toUpperCase()}</h3>
                </div>
                <p>Check the <a href="${BUILD_URL}">console output</a>.</p>
                </div>
                </body>
                </html>
            """

            emailext (
                subject: "${jobName} - Build ${buildNumber} - ${pipelineStatus.toUpperCase()}",
                body: body,
                to: 'nills009@gmail.com',
                from: 'nills009@gmail.com',
                replyTo: 'jenkins@gmail.com',
                mimeType: 'text/html',
               
            )
        }
    }
}
}
