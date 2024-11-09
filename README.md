# Estudo Generalizado de Kubernetes

## K3D

k3d é uma ferramenta leve que permite rodar clusters Kubernetes (especificamente a distribuição minimalista k3s da Rancher Lab) em Docker. Isso facilita a criação de clusters de um único nó ou multi-nós diretamente no seu ambiente local, ideal para desenvolvimento e teste

```
k3d cluster create mycluster

kubectl get nodes
```

## Pods do kube-system
Quando você cria um cluster Kubernetes, alguns pods padrão são implantados automaticamente para garantir que o cluster funcione corretamente. Esses pods geralmente incluem componentes essenciais do plano de controle e outros serviços auxiliares

* CoreDNS: Um serviço de DNS para resolver nomes de serviço em seu cluster. Ele é crucial para permitir que os pods encontrem e se comuniquem uns com os outros.

* Metrics Server: Coleta métricas de uso de recursos dos nodes e pods. Isso é importante para a auto-escala e monitoramento

* Traefik: Um ingress controller que gerencia o acesso externo aos serviços no cluster. Ele roteará o tráfego para os serviços apropriados

* Local Path Provisioner: Gerencia volumes persistentes locais para os pods

```
kubectl get pods -n kube-system
```