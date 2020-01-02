# Service Discovery – Ćwiczenia

## Przetestuj działanie Service Discovery korzystając ze swojej aplikacji albo helloapp.

### Utwórz dwa namespaces

```
> kubectl create namespace frontend
namespace/frontend created

> kubectl create namespace backend
namespace/backend created
```

### W każdym namespaces umieć pod i serwis

`pod-in-different-namespaces:`
```
apiVersion: v1
kind: Service
metadata:
  name: hello-api
  namespace: backend
spec:
  type: ClusterIP
  selector:
    app: hello-api
  ports:
  - name: http
    port: 80
    protocol: TCP
    targetPort: http
---
apiVersion: v1
kind: Pod
metadata:
  labels:
    app: hello-api
  name: hello-api
  namespace: backend
spec:
  containers:
  - image: poznajkubernetes/helloapp:svc
    name: hello
    ports:
      - containerPort: 8080
        name: http
        protocol: TCP
---
apiVersion: v1
kind: Service
metadata:
  name: hello-page
  namespace: frontend
spec:
  type: ClusterIP
  selector:
    app: hello-page
  ports:
  - name: http
    port: 80
    protocol: TCP
    targetPort: http
---
apiVersion: v1
kind: Pod
metadata:
  labels:
    app: hello-page
  name: hello-page
  namespace: frontend
spec:
  containers:
  - image: poznajkubernetes/helloapp:svc
    name: hello
    ports:
      - containerPort: 8080
        name: http
        protocol: TCP

```

```
> kubectl apply -f pod-in-different-namespaces.yml
> kubectl get all -n frontend
NAME             READY   STATUS    RESTARTS   AGE
pod/hello-page   1/1     Running   0          5m46s

NAME                 TYPE        CLUSTER-IP    EXTERNAL-IP   PORT(S)   AGE       
service/hello-page   ClusterIP   10.103.92.6   <none>        80/TCP    5m46s 

> kubectl get all -n backend 
NAME            READY   STATUS    RESTARTS   AGE
pod/hello-api   1/1     Running   0          4m32s

NAME                TYPE        CLUSTER-IP     EXTERNAL-IP   PORT(S)   AGE       
service/hello-api   ClusterIP   10.102.94.36   <none>        80/TCP    4m32s 
```

### Przetestuj działanie Service Discovery z wykorzystaniem curl i nslookup. Jeśli używasz swojej aplikacji wywołaj endpointy pomiędzy aplikacjami.

```
kubectl run -it --rm tools --generator=run-pod/v1 --image=giantswarm/tiny-tools
```

nslookup:

jeżeli odpytujemy o serwis z osobnego `namespace` i nie podamy `namespace` w adresie serwisu, nie będziemy w stanie dostać adresu ip

```
/ # nslookup hello-page
Server:         10.96.0.10
Address:        10.96.0.10#53

** server can't find hello-page: NXDOMAIN
```

po podaniu namespace jesteśmy w stanie odnaleźć adres ip

```
/ # nslookup hello-page.frontend
Server:         10.96.0.10
Address:        10.96.0.10#53

Name:   hello-page.frontend.svc.cluster.local
Address: 10.103.92.6
```

jesteśmy również w stanie wywoływać serwisy pomiędzy namespacami za pomocna konwencji `<service_name>.<namespace>`
```
/ # curl hello-api
curl: (6) Could not resolve host: hello-api
/ # curl hello-api.backend
Cześć, 🚢 =>  hello-api
/ # curl hello-page.frontend
Cześć, 🚢 =>  hello-page
/ #
```