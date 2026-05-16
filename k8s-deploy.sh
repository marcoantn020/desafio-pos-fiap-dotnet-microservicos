#!/usr/bin/env bash
# Deploy FCG no cluster Kubernetes local (Kind)
# Pré-requisito: kind e kubectl instalados
#   curl -Lo ./kind https://kind.sigs.k8s.io/dl/v0.23.0/kind-linux-amd64 && chmod +x ./kind && sudo mv ./kind /usr/local/bin/kind
#   curl -LO "https://dl.k8s.io/release/$(curl -sL https://dl.k8s.io/release/stable.txt)/bin/linux/amd64/kubectl" && chmod +x kubectl && sudo mv kubectl /usr/local/bin/

set -e

CLUSTER_NAME="fcg-cluster"

echo "==> Criando cluster Kind (se não existir)..."
kind get clusters | grep -q "$CLUSTER_NAME" || kind create cluster --name "$CLUSTER_NAME"

echo "==> Carregando imagens Docker no cluster..."
kind load docker-image fcg-users-api:latest        --name "$CLUSTER_NAME"
kind load docker-image fcg-catalog-api:latest      --name "$CLUSTER_NAME"
kind load docker-image fcg-payments-api:latest     --name "$CLUSTER_NAME"
kind load docker-image fcg-notifications-api:latest --name "$CLUSTER_NAME"

echo "==> Aplicando manifests de infraestrutura (Postgres + RabbitMQ)..."
kubectl apply -f infra/k8s/

echo "==> Aguardando Postgres ficar pronto..."
kubectl wait --for=condition=ready pod -l app=postgres --timeout=90s

echo "==> Aguardando RabbitMQ ficar pronto..."
kubectl wait --for=condition=ready pod -l app=rabbitmq --timeout=90s

echo "==> Aplicando manifests dos microsserviços..."
kubectl apply -f users-api/k8s/
kubectl apply -f catalog-api/k8s/
kubectl apply -f payments-api/k8s/
kubectl apply -f notications-api/k8s/

echo "==> Aguardando pods dos serviços ficarem prontos..."
kubectl wait --for=condition=ready pod -l app=users-api         --timeout=120s
kubectl wait --for=condition=ready pod -l app=catalog-api       --timeout=120s
kubectl wait --for=condition=ready pod -l app=payments-api      --timeout=120s
kubectl wait --for=condition=ready pod -l app=notifications-api --timeout=120s

echo ""
echo "==> Deploy concluído! Pods em execução:"
kubectl get pods

echo ""
echo "==> Serviços expostos:"
kubectl get services

echo ""
echo "Acesse os serviços via NodePort (use 'kubectl cluster-info' para o IP do cluster):"
echo "  UsersAPI:         http://<node-ip>:30001"
echo "  CatalogAPI:       http://<node-ip>:30002"
echo "  PaymentsAPI:      http://<node-ip>:30003"
echo "  NotificationsAPI: http://<node-ip>:30004"
echo "  RabbitMQ Mgmt:    http://<node-ip>:30015"
echo ""
echo "Com Kind, use port-forward para acesso local:"
echo "  kubectl port-forward svc/users-api 5001:80"
