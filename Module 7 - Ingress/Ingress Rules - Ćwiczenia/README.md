# Używanie reguł Ingress – Ćwiczenia

definicję serwisów i deploymentów użyte w ćwiczeniu:

[dumpster-app](./dumpster-app.yaml)

[echo-app](./echo-app.yaml)

[hello-app-v1](./hello-app-v1.yaml)

[hello-app-v2](./hello-app-v2.yaml)

### 1. Stwórz Ingress (i pomocnicze zasoby) jako default backend

`ingress-capture-all.yaml`
```
apiVersion: networking.k8s.io/v1beta1
kind: Ingress
metadata:
  name: capture-all
spec:
  backend:
    serviceName: hello-app-v1
    servicePort: 8080
```
Po wgraniu konfiguracji ingresa widzimy, że jest on dostępny spoza klastra na adresie `localhost:80`.
Widzimy również, że reguły `host: *` oraz `path: *` wskazują na dwie instancje `hello-app-v1` zgodnie z konfiguracją.
```
> kubectl get ing
NAME          HOSTS   ADDRESS       PORTS   AGE
capture-all   *       localhost     80      16s

> kubectl describe ing capture-all
Name:             capture-all
Namespace:        default
Address:          localhost
Default backend:  hello-app-v1:8080 (10.1.1.44:8080,10.1.1.45:8080)
Rules:
  Host  Path  Backends
  ----  ----  --------
  *     *     hello-app-v1:8080 (10.1.1.44:8080,10.1.1.45:8080)
Annotations:
  kubectl.kubernetes.io/last-applied-configuration:  {"apiVersion":"networking.k8s.io/v1beta1","kind":"Ingress","metadata":{"annotations":{},"name":"capture-all","namespace":"default"},"spec":{"backend":{"serviceName":"hello-app-v1","servicePort":8080}}}

Events:
  Type    Reason  Age   From                      Message
  ----    ------  ----  ----                      -------
  Normal  CREATE  71s   nginx-ingress-controller  Ingress default/capture-all
  Normal  UPDATE  20s   nginx-ingress-controller  Ingress default/capture-all
```

jeżeli odpytamy teraz `localhost` z różnymi `path` i `queries` zawsze jesteśmy przekierowywani na ten sam adress `hello-app-v1:8080` (widzmy, że poprawnie trafiamy na rózne instancje serwisu)
```
> curl localhost
Cześć, 🚢 =>  hello-app-v1-78fdd8d6b5-cgfsz
> curl localhost/test
Cześć, 🚢 =>  hello-app-v1-78fdd8d6b5-kv9ml
> localhost?blablabla
Cześć, 🚢 =>  hello-app-v1-78fdd8d6b5-cgfsz
```

### 2. Stwórz Ingress (i pomocnicze zasoby) i ustaw fanout routing

`ingress-fanout.yaml`:
```
apiVersion: networking.k8s.io/v1beta1
kind: Ingress
metadata:
  name: fanout
  annotations:
    nginx.ingress.kubernetes.io/rewrite-target: /$1
spec:
  rules:  
  - http:
      paths:
      - path: /v1/?(.*)
        backend: 
          serviceName: hello-app-v1
          servicePort: 8080
      - path: /v2/?(.*)
        backend: 
          serviceName: hello-app-v2
          servicePort: 8080
      - path: /echo/?(.*)
        backend: 
          serviceName: echo-app
          servicePort: 8080
```
Po wgraniu konfiguracji ingresa widzimy, że jest on dostępny spoza klastra na adresie `localhost:80`.
Widzimy również, że wildcard nadal ustawiony jest dla `host` jednak `path` wskazują już zgodnie z konfiguracją na odpowiednie serwisy
```
> kubectl get ing
NAME     HOSTS   ADDRESS     PORTS   AGE
fanout   *       localhost   80      80s

> kubectl describe ing fanout
Name:             fanout
Namespace:        default
Address:          localhost
Default backend:  default-http-backend:80 (<none>)
Rules:
  Host  Path  Backends
  ----  ----  --------
  *
        /v1/?(.*)     hello-app-v1:8080 (10.1.1.44:8080,10.1.1.45:8080)
        /v2/?(.*)     hello-app-v2:8080 (10.1.1.46:8080,10.1.1.47:8080)
        /echo/?(.*)   echo-app:8080 (10.1.1.42:8080,10.1.1.43:8080)
Annotations:
  kubectl.kubernetes.io/last-applied-configuration:  {"apiVersion":"networking.k8s.io/v1beta1","kind":"Ingress","metadata":{"annotations":{"nginx.ingress.kubernetes.io/rewrite-target":"/$1"},"name":"fanout","namespace":"default"},"spec":{"rules":[{"http":{"paths":[{"backend":{"serviceName":"hello-app-v1","servicePort":8080},"path":"/v1/?(.*)"},{"backend":{"serviceName":"hello-app-v2","servicePort":8080},"path":"/v2/?(.*)"},{"backend":{"serviceName":"echo-app","servicePort":8080},"path":"/echo/?(.*)"}]}}]}}

  nginx.ingress.kubernetes.io/rewrite-target:  /$1
Events:
  Type    Reason  Age                From                      Message
  ----    ------  ----               ----                      -------
  Normal  CREATE  89s                nginx-ingress-controller  Ingress default/fanout
  Normal  UPDATE  49s (x2 over 66s)  nginx-ingress-controller  Ingress default/fanout
```
jeżeli teraz odpytamy `localhost` na który nie mamy ustawionego przekierowania - otrzymamy 404
```
> curl localhost

<html>
<head><title>404 Not Found</title></head>
<body>
<center><h1>404 Not Found</h1></center>
<hr><center>openresty/1.15.8.2</center>
</body>
</html>
```
widzimy, że `localhost/v1/*` przekierowany zostanie na `hello-app-v1:8080`
```
> curl localhost/v1/abc
Cześć, 🚢 =>  hello-app-v1-78fdd8d6b5-kv9ml
```
a `localhost/v2/*` odpowiednio na `hello-app-v2:8080`
```
> curl localhost/v2/abc
Cześć, 🚢 =>  hello-app-v2-7767747b4c-2r26x
```
dla potwierdzenia, że `path` i `query` są poprawnie przekazywane do applikacji możemy odpytać `echo-app` 
```
> curl localhost/echo/xyz?abc=123
CLIENT VALUES:
client_address=10.1.1.39
command=GET
real path=/xyz?abc=123
query=abc=123
request_version=1.1
request_uri=http://localhost:8080/xyz?abc=123

SERVER VALUES:
server_version=nginx: 1.10.0 - lua: 10001

HEADERS RECEIVED:
accept=*/*
host=localhost
user-agent=curl/7.58.0
x-forwarded-for=192.168.65.3
x-forwarded-host=localhost
x-forwarded-port=80
x-forwarded-proto=http
x-real-ip=192.168.65.3
x-request-id=25bd42049141bf62027a0b4859af681d
x-scheme=http
BODY:
-no body in request-
```

### 3. Stwórz Ingress (i pomocnicze zasoby) i ustaw host routing

`ingress-host-routing.yaml`:
```
apiVersion: networking.k8s.io/v1beta1
kind: Ingress
metadata:
  name: host-routing
spec:
  rules:
  - host: v1.127.0.0.1.nip.io
    http:
      paths:
      - backend:
          serviceName: hello-app-v1
          servicePort: 8080
  - host: v2.127.0.0.1.nip.io
    http:
      paths:
      - backend:
          serviceName: hello-app-v2
          servicePort: 8080
  - host: echo.127.0.0.1.nip.io
    http:
      paths:
      - backend:
          serviceName: echo-app
          servicePort: 8080
```
Po wgraniu konfiguracji ingresa widzimy, że jest on dostępny spoza klastra na adresie `localhost:80`.
Widzimy również od razu listę zkonfigurowanych hostów jak i przypisane do nich docelowe serwisy
```
> kubectl get ing
NAME           HOSTS                                                           ADDRESS     PORTS   AGE
host-routing   v1.127.0.0.1.nip.io,v2.127.0.0.1.nip.io,echo.127.0.0.1.nip.io   localhost   80      2m19s

> kubectl describe ing host-routing
Name:             host-routing
Namespace:        default
Address:          localhost
Default backend:  default-http-backend:80 (<none>)
Rules:
  Host                   Path  Backends
  ----                   ----  --------
  v1.127.0.0.1.nip.io          hello-app-v1:8080 (10.1.1.44:8080,10.1.1.45:8080)
  v2.127.0.0.1.nip.io          hello-app-v2:8080 (10.1.1.46:8080,10.1.1.47:8080)
  echo.127.0.0.1.nip.io        echo-app:8080 (10.1.1.42:8080,10.1.1.43:8080)
Annotations:
  kubectl.kubernetes.io/last-applied-configuration:  {"apiVersion":"networking.k8s.io/v1beta1","kind":"Ingress","metadata":{"annotations":{},"name":"host-routing","namespace":"default"},"spec":{"rules":[{"host":"v1.127.0.0.1.nip.io","http":{"paths":[{"backend":{"serviceName":"hello-app-v1","servicePort":8080}}]}},{"host":"v2.127.0.0.1.nip.io","http":{"paths":[{"backend":{"serviceName":"hello-app-v2","servicePort":8080}}]}},{"host":"echo.127.0.0.1.nip.io","http":{"paths":[{"backend":{"serviceName":"echo-app","servicePort":8080}}]}}]}}

Events:
  Type    Reason  Age    From                      Message
  ----    ------  ----   ----                      -------
  Normal  CREATE  2m37s  nginx-ingress-controller  Ingress default/host-routing
  Normal  UPDATE  2m4s   nginx-ingress-controller  Ingress default/host-routing
```
widzimy również poprawne odpowiedzi gdy odpytamy poszczególne hosty
```
> curl v1.127.0.0.1.nip.io
Cześć, 🚢 =>  hello-app-v1-78fdd8d6b5-kv9ml

> curl v2.127.0.0.1.nip.io
Cześć, 🚢 =>  hello-app-v2-7767747b4c-cpgnk

> curl echo.127.0.0.1.nip.io/xyz?abc=123
CLIENT VALUES:
client_address=10.1.1.39
command=GET
real path=/xyz?abc=123
query=abc=123
request_version=1.1
request_uri=http://echo.127.0.0.1.nip.io:8080/xyz?abc=123

SERVER VALUES:
server_version=nginx: 1.10.0 - lua: 10001

HEADERS RECEIVED:
accept=*/*
host=echo.127.0.0.1.nip.io
user-agent=curl/7.58.0
x-forwarded-for=192.168.65.3
x-forwarded-host=echo.127.0.0.1.nip.io
x-forwarded-port=80
x-forwarded-proto=http
x-real-ip=192.168.65.3
x-request-id=0432f6275a20278f282a50227bed464e
x-scheme=http
BODY:
-no body in request-
```

### 4. Stwórz Ingress (i pomocnicze zasoby) i wymieszaj dowolnie fanout i host routing

`ingress-mixed-routing.yaml`:
```
apiVersion: networking.k8s.io/v1beta1
kind: Ingress
metadata:
  name: mixed-routing
  annotations:
    nginx.ingress.kubernetes.io/rewrite-target: /$1
spec:
  rules:
  - host: v1.127.0.0.1.nip.io
    http:
      paths:
      - backend:
          serviceName: hello-app-v1
          servicePort: 8080
  - host: v2.127.0.0.1.nip.io
    http:
      paths:
      - backend:
          serviceName: hello-app-v2
          servicePort: 8080
  - host: mix.127.0.0.1.nip.io
    http:
      paths:
      - path: /echo/?(.*)
        backend: 
          serviceName: echo-app
          servicePort: 8080
      - path: /dumpster/?(.*)
        backend: 
          serviceName: dumpster-app
          servicePort: 8080
```

```
> kubectl describe ing mixed-routing
Name:             mixed-routing
Namespace:        default
Address:
Default backend:  default-http-backend:80 (<none>)
Rules:
  Host                  Path              Backends
  ----                  ----              --------
  v1.127.0.0.1.nip.io                     hello-app-v1:8080 (10.1.1.44:8080,10.1.1.45:8080)
  v2.127.0.0.1.nip.io                     hello-app-v2:8080 (10.1.1.46:8080,10.1.1.47:8080)
  mix.127.0.0.1.nip.io
                        /echo/?(.*)       echo-app:8080 (10.1.1.42:8080,10.1.1.43:8080)
                        /dumpster/?(.*)   dumpster-app:8080 (10.1.1.40:8080,10.1.1.41:8080)
Annotations:
  kubectl.kubernetes.io/last-applied-configuration:  {"apiVersion":"networking.k8s.io/v1beta1","kind":"Ingress","metadata":{"annotations":{"nginx.ingress.kubernetes.io/rewrite-target":"/$1"},"name":"mixed-routing","namespace":"default"},"spec":{"rules":[{"host":"v1.127.0.0.1.nip.io","http":{"paths":[{"backend":{"serviceName":"hello-app-v1","servicePort":8080}}]}},{"host":"v2.127.0.0.1.nip.io","http":{"paths":[{"backend":{"serviceName":"hello-app-v2","servicePort":8080}}]}},{"host":"mix.127.0.0.1.nip.io","http":{"paths":[{"backend":{"serviceName":"echo-app","servicePort":8080},"path":"/echo/?(.*)"},{"backend":{"serviceName":"dumpster-app","servicePort":8080},"path":"/dumpster/?(.*)"}]}}]}}

  nginx.ingress.kubernetes.io/rewrite-target:  /$1
Events:
  Type    Reason  Age   From                      Message
  ----    ------  ----  ----                      -------
  Normal  CREATE  10s   nginx-ingress-controller  Ingress default/mixed-routing
```

```
> curl v1.127.0.0.1.nip.io
Cześć, 🚢 =>  hello-app-v1-78fdd8d6b5-cgfsz
```
```
> curl v2.127.0.0.1.nip.io
Cześć, 🚢 =>  hello-app-v2-7767747b4c-2r26x
```
```
> curl mix.127.0.0.1.nip.io
<html>
<head><title>404 Not Found</title></head>
<body>
<center><h1>404 Not Found</h1></center>
<hr><center>openresty/1.15.8.2</center>
</body>
</html>
```
```
> curl mix.127.0.0.1.nip.io/any
<html>
<head><title>404 Not Found</title></head>
<body>
<center><h1>404 Not Found</h1></center>
<hr><center>openresty/1.15.8.2</center>
</body>
</html>
```
```
> curl mix.127.0.0.1.nip.io/dumpster/xyz?abc=123
v1 running on dumpster-app-594995f987-m7f86
```
```
> curl mix.127.0.0.1.nip.io/echo/xyz?abc=123
CLIENT VALUES:
client_address=10.1.1.39
command=GET
real path=/xyz?abc=123
query=abc=123
request_version=1.1
request_uri=http://mix.127.0.0.1.nip.io:8080/xyz?abc=123

SERVER VALUES:
server_version=nginx: 1.10.0 - lua: 10001

HEADERS RECEIVED:
accept=*/*
host=mix.127.0.0.1.nip.io
user-agent=curl/7.58.0
x-forwarded-for=192.168.65.3
x-forwarded-host=mix.127.0.0.1.nip.io
x-forwarded-port=80
x-forwarded-proto=http
x-real-ip=192.168.65.3
x-request-id=9d8928595359d4d1638a21f0ec9858ae
x-scheme=http
BODY:
-no body in request-krzyskowk@DESKTOP-TAJ12LR:/mnt/d/work/Poznaj Kubernetes/Module 3 - Configuration/Scurl mix.127.0.0.1.nip.io/dumpster/xyz?abc=123
v1 running on dumpster-app-594995f987-m7f86
```
