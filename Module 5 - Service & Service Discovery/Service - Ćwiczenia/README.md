# Obiekt serwisu – Ćwiczenia

## Ćwiczenie 1

### container-to-container w Pod. Wykorzystaj do tego nginx.

`container-to-container.yaml:`
```
apiVersion: v1
kind: Pod
metadata:
  name: nginx
spec:
  containers:
  - image: nginx
    name: nginx
    ports:
      - containerPort: 80
        name: http
        protocol: TCP
  - image: busybox
    name: busybox
    command: ['sleep', '100']
```

```
> kubectl apply -f container-to-container.yaml
> kubectl exec nginx -c busybox -- wget -qO- localhost:80
<!DOCTYPE html>
<html>
<head>
<title>Welcome to nginx!</title>
<style>
...
```

### Komunikacja pomiędzy Podami – Pod-to-Pod. Wykorzystaj do tego nginx.

`pod-to-pod.yaml:`
```
apiVersion: v1
kind: Pod
metadata:
  name: nginx
spec:
  containers:
  - image: nginx
    name: nginx
    ports:
      - containerPort: 80
        name: http
        protocol: TCP
---
apiVersion: v1
kind: Pod
metadata:
  name: busybox
spec:
  containers:
  - image: busybox
    name: busybox
    command: ['sleep', '100']
```
```
> kubectl apply -f pod-to-pod.yaml
> kubectl get pods -o wide
NAME      READY   STATUS    RESTARTS   AGE   IP           NODE             NOMINATED NODE   READINESS GATES
busybox   1/1     Running   0          16s   10.1.0.135   docker-desktop   <none>           <none>
nginx     1/1     Running   0          16s   10.1.0.134   docker-desktop   <none>           <none>

> kubectl exec busybox -- wget -qO- 10.1.0.134
<!DOCTYPE html>
<html>
<head>
<title>Welcome to nginx!</title>
<style>
...
```

### Wykorzystaj nginx i wystaw go za pomocą serwisu ClusterIP w środku klastra.

`clusterip-service.yaml:`
```
apiVersion: v1
kind: Service
metadata:
  name: clusterip-service
spec:
  type: ClusterIP
  selector:
    app: nginx
  ports:
  - name: http
    port: 8080
    protocol: TCP
    targetPort: http
---
apiVersion: v1
kind: Pod
metadata:
  name: nginx1
  labels:
    app: nginx
spec:
  containers:
  - image: nginx
    name: nginx
    ports:
      - containerPort: 80
        name: http
        protocol: TCP
---
apiVersion: v1
kind: Pod
metadata:
  name: nginx2
  labels:
    app: nginx
spec:
  containers:
  - image: nginx
    name: nginx
    ports:
      - containerPort: 80
        name: http
        protocol: TCP
```

```
> kubectl apply -f .\clusterip-service.yaml
service/clusterip-service created
pod/nginx1 created
pod/nginx2 created
```

Widzimy, że IP podów `nginx1` i `nginx2` są przypisane w endpointach `clusterip-service` service
```
> kubectl get pods -o wide
NAME     READY   STATUS    RESTARTS   AGE   IP           NODE             NOMINATED NODE   READINESS GATES
nginx1   1/1     Running   0          10m   10.1.0.136   docker-desktop   <none>           <none>
nginx2   1/1     Running   0          10m   10.1.0.137   docker-desktop   <none>           <none>

> kubectl get endpoints
NAME                ENDPOINTS                     AGE
clusterip-service   10.1.0.136:80,10.1.0.137:80   13m
kubernetes          192.168.65.3:6443             23d

> kubectl describe service clusterip-service
Name:              clusterip-service
Namespace:         default
Labels:            <none>
Annotations:       kubectl.kubernetes.io/last-applied-configuration:
                     {"apiVersion":"v1","kind":"Service","metadata":{"annotations":{},"name":"clusterip-service","namespace":"default"},"spec":{"ports":[{"name...
Selector:          app=nginx
Type:              ClusterIP
IP:                10.101.162.234
Port:              http  8080/TCP
TargetPort:        http/TCP
Endpoints:         10.1.0.136:80,10.1.0.137:80
Session Affinity:  None
Events:            <none>
```


sprawdzamy czy nginx jest dostępny pod adresem IP servisu
```
> kubectl get svc 
NAME                TYPE        CLUSTER-IP       EXTERNAL-IP   PORT(S)    AGE
clusterip-service   ClusterIP   10.101.162.234   <none>        8080/TCP   63s
kubernetes          ClusterIP   10.96.0.1        <none>        443/TCP    23d

> kubectl run -it --rm tools --generator=run-pod/v1 --image=giantswarm/tiny-tools
# curl 10.101.162.234:8080
<!DOCTYPE html>
<html>
<head>
<title>Welcome to nginx!</title>
...
```

sprawdzamy również czy możemy dostać się do nginx przez nazwę naszego serwisu
```
> kubectl run -it --rm tools --generator=run-pod/v1 --image=giantswarm/tiny-tools
# curl clusterip-service:8080
<!DOCTYPE html>
<html>
<head>
<title>Welcome to nginx!</title>
...
```

### Wykorzystaj nginx i wystaw go na świat za pomocą serwisu NodePort w dwóch opcjach: bez wskazywania portu dla NodePort i ze wskazaniem.



### Wykorzystaj nginx i wystaw go na świat za pomocą serwisu typu LoadBalancer,