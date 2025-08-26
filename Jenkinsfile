pipeline {
    agent any

    environment {
        AWS_REGION    = 'me-south-1'                   
        ECR_REPO      = '223700470790.dkr.ecr.me-south-1.amazonaws.com/qa/whatsappbridge'
        VERSION       = "1.0.0-${env.GIT_COMMIT[0..6]}"  
    }

    stages {
        stage('Checkout') {
            steps {
                git branch: 'Devops-Branch', url: 'https://github.com/TahaRamkda/bridge.git'
            }
        }

        stage('Docker Build') {
            steps {
                sh """
                  echo Building Docker image: $ECR_REPO:$VERSION
                  docker build -t $ECR_REPO:$VERSION -f WhatsAppBridge/WhatsAppBridge/Dockerfile .
                """
            }
        }

        stage('Docker Push to ECR') {
            steps {
                withCredentials([aws(credentialsId: 'iam_user_cred', region: "${AWS_REGION}")]) {
                    sh """
                      aws ecr get-login-password --region $AWS_REGION \
                        | docker login --username AWS --password-stdin $ECR_REPO
                      docker push $ECR_REPO:$VERSION
                    """
                }
            }
        }

        stage('Update Kubernetes Deployment') {
            steps {
                withCredentials([[$class: 'AmazonWebServicesCredentialsBinding', credentialsId: 'iam_user_cred']]) {
                    sh """
                    export AWS_REGION=${AWS_REGION}

                    aws eks update-kubeconfig \
                        --name bct-cluster \
                        --region $AWS_REGION

                    kubectl set image deployment/bridge-deploy \
                        bridge=$ECR_REPO:$VERSION \
                        --namespace=qa

                    kubectl rollout status deployment/bridge-deploy --namespace=qa
                    """
                }
            }
        }
    }
}