pipeline {
    agent {
        kubernetes {
            yaml """
apiVersion: v1
kind: Pod
metadata:
  labels:
    app: jenkins-kaniko
spec:
  serviceAccountName: jenkins-sa
  containers:
  - name: kaniko
    image: gcr.io/kaniko-project/executor:latest
    command:
    - sleep
    args:
    - infinity
    volumeMounts:
    - name: workspace-volume
      mountPath: /workspace
  volumes:
  - name: workspace-volume
    emptyDir: {}
"""
        }
    }

    environment {
        AWS_REGION = 'me-south-1'
        ECR_REPO   = '223700470790.dkr.ecr.me-south-1.amazonaws.com/qa/whatsappbridge'
        VERSION    = "1.0.0-${env.GIT_COMMIT[0..6]}"
    }

    stages {
        stage('Checkout') {
            steps {
                git branch: 'Devops-Branch', url: 'https://github.com/TahaRamkda/bridge.git'
            }
        }

        stage('Build & Push with Kaniko') {
            steps {
                container('kaniko') {
                    sh """
                      /kaniko/executor \
                        --context=${WORKSPACE}/WhatsAppBridge/WhatsAppBridge \
                        --dockerfile=${WORKSPACE}/WhatsAppBridge/WhatsAppBridge/Dockerfile \
                        --destination=$ECR_REPO:$VERSION \
                        --single-snapshot \
                        --verbosity=info
                    """
                }
            }
        }

        stage('Update Kubernetes Deployment') {
            steps {
                sh """
                  kubectl set image deployment/bridge-deploy \
                      bridge=$ECR_REPO:$VERSION \
                      --namespace=qa

                  kubectl rollout status deployment/bridge-deploy --namespace=qa
                """
            }
        }
    }
}
