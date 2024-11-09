# Estudo Generalizado de Kubernetes

## Docker
Docker é uma plataforma que permite criar, testar e implantar aplicações rapidamente usando contêineres. Esses contêineres são pacotes leves, portáteis e autossuficientes que incluem tudo o que a aplicação precisa para rodar: código, runtime, bibliotecas e dependências.

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

## Build de imagem simples com Docker
### Descrição
Construir uma imagem Docker envolve criar um pacote que contém tudo o que sua aplicação precisa para rodar, incluindo o código, as dependências, o runtime e outras bibliotecas necessárias. Esse processo é feito utilizando um Dockerfile, que é um arquivo de texto com uma série de instruções para montar a imagem.

Passos para Construir uma Imagem Docker
Escrever o Dockerfile: Um arquivo simples que descreve como montar a imagem.

* **FROM**: Define a imagem base.

* **COPY**: Copia arquivos do seu sistema para a imagem.

* **RUN**: Executa comandos para instalar dependências.

* **CMD**: Define o comando padrão que será executado quando um contêiner for iniciado a partir da imagem.


Usaremos o projeto DotNetDefaultWebApi como exemplo desse build, na raiz do projeto teremos do Dockerfile que foi gerado pelo próprio Visual Studio

Build da imagem:

```
# -t especificamos o nome da imagem
# :latest criamos a tag de versão da imagem, por default vamos criar a latest
# . define que o arquivo Dockerfile se encontra na raiz
docker build -t dot-net-default-web-api:latest .
```

Rodar a imagem no container
```
docker run -d -p 8080:8080 dot-net-default-web-api:latest
```

Comandos para verificar detalhes sobre o container
```
# Ver os containers em execução
docker ps

# Logs do container
docker logs <container-id>

# Acessar container
docker exec -it <id-container> /bin/bash

# Ver variaveis do ambiente do container
docker exec <id-container> env 

# Parar container
docker stop <id-container>

# Deletar container
docker rm <id-container>
```

Vamos habilitar a imagem para ser acessada local:
```
# Se não setarmos a variave de ambiente o swagger nao vai ser exposto
docker run -d -p 8080:8080 dot-net-default-web-api:latest

# --name setar nome para o container
# -e setar variavel de ambiente (-e VARIAVEL1=valor1 -e VARIAVEL2=valor2)
docker run -d -p 8080:8080 --name dotnet-default-webapi -e ASPNETCORE_ENVIRONMENT=Development dot-net-default-web-api:latest
```

Se o container executar e conseguir acessar os recursos



## Deploy da Imagem

### K3D
Para fazer o deploy da imagem no K3D, podemos fazer de 2 maneiras:

* Importa essa image para ele, pois assim não precisamos publicar a imagem em nenhum registry
```
k3d image import dot-net-default-web-api:latest
```

* Publicar a imagem em um registry, como o DockerHub por exemplo
```
# Build da imagem seguindo os padroes
docker build -t kakoferrare/dot-net-default-web-api:latest .

# Login no DockerHub
docker login

#Enviar pro DockerHub
docker push kakoferrare/dot-net-default-web-api:latest  
```

### Criar arquivo Deployment.yaml
Precisamos criar um arquivo de deploy com as especificações do nosso Pod, ele geralmente fica na pasta k8s

```
apiVersion: apps/v1
kind: Deployment
metadata:
  name: dotnet-default-web-api
spec:
  replicas: 1
  selector:
    matchLabels:
      app: dotnet-default-web-api
  template:
    metadata:
      labels:
        app: dotnet-default-web-api
    spec:
      containers:
        - name: dotnet-default-web-api
          image: kakoferrare/dot-net-default-web-api:latest
          ports:
            - containerPort: 8080
          resources:
            requests:
              memory: "64Mi"
              cpu: "250m"
            limits:
              memory: "256Mi"
              cpu: "500m"
```

Apenas com esse arquivo já tudo que precisamos para subir o pod com a imagem no cluster, com o comando
```
kubectl apply -f ./k8s/deployment.yaml
```

Conferir se o pod subiu no k3d
```
kubectl get pod
kubectl logs dotnet-default-web-api-845d4cdd84-q2fxc
kubectl get pod -o wide
```

Como ainda não subimos o service, que expoe o pod, caso queira acessa-lo e ver se aplicação responde.
Precisamos do port-forward
```
kubectl port-forward <nome-do-pod> <porta-local>:<porta-do-pod>
```
Com esse comando conseguimos acessar pelo local a aplicação rodando no pod


## Service
Service é um recurso fundamental que permite definir uma maneira lógica de acessar um conjunto de pods como um único serviço. Ele abstrai a complexidade da comunicação entre os pods e proporciona um método estável para acessar as aplicações, mesmo que os pods subjacentes possam mudar ao longo do tempo.

Para isso vamos criar o arquivo service.yaml
```
apiVersion: v1
kind: Service
metadata:
  name: dotnet-default-web-api
spec:
  selector:
    app: dotnet-default-web-api
  ports:
    - protocol: TCP
      port: 80
      targetPort: 8080
  type: LoadBalancer

```

Para verificar o serviço
```
kubectl get service

## Caso quisermos ver mais detalhes, ou até avaliar se tem algum erro no service
kubectl describe svc dotnet-default-web-api
```
E tentar acessar o serviço:
http://localhost:8080/Name