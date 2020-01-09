# Używanie certyfikatów w Ingress – Ćwiczenia

### Korzystając z Ingress utworzonych przy okazji „Używanie reguł Ingress – Ćwiczenia” zmodyfikuje je o wykorzystanie certyfikatów.

### W zależności od możliwości użyj self-sign lub Let’s Encrypt.

generujemy lokalny certyfikat
```
> openssl req -newkey rsa:2048 -nodes -keyout key.pem -x509 -days 365 -out certificate.pem

Generating a 2048 bit RSA private key
................+++++
......................................+++++
writing new private key to 'key.pem'
-----
You are about to be asked to enter information that will be incorporated
into your certificate request.
What you are about to enter is what is called a Distinguished Name or a DN.
There are quite a few fields but you can leave some blank
For some fields there will be a default value,
If you enter '.', the field will be left blank.
-----
Country Name (2 letter code) [AU]:PL
State or Province Name (full name) [Some-State]:
Locality Name (eg, city) []:POZNAN
Organization Name (eg, company) [Internet Widgits Pty Ltd]:krzyskowk
Organizational Unit Name (eg, section) []:
Common Name (e.g. server FQDN or YOUR name) []:mix.127.0.0.1.nip.io
Email Address []:krzysztof.krzyskow@gmail.com
```

dodajemy certyfikat do `secrets` jako `tls-localhost`
```
> kubectl create secret tls tls-localhost --key key.pem --cert certificate.pem
secret/tls-localhost created

> kubectl get secret tls-localhost -o wide
NAME            TYPE                DATA   AGE
tls-localhost   kubernetes.io/tls   2      50s
```

następnie modyfikujemy `ingress` o nazwie `mixed-routing` stworzony w ćwiczeniu. Konfigurujemy TLS tylko dla jednego z hostów ustawionego w routingu

`ingress-mixed-routing-tls.yaml`:
```
apiVersion: networking.k8s.io/v1beta1
kind: Ingress
metadata:
  name: mixed-routing
  annotations:
    nginx.ingress.kubernetes.io/rewrite-target: /$1
spec:
  tls:
  - hosts:
    - mix.127.0.0.1.nip.io
    secretName: tls-localhost
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

widzimy że po skonfigurowaniu TLS w ingress zaczyna korzystać z domyślnego portu dla https (443) - widoczne w sekcji `PORTS`.
Odpowiedni wpis o TLS jest widoczny również po wykonaniu `describe`
```
> kubectl apply -f .\ingress-mixed-routing-tls.yaml
ingress.networking.k8s.io/mixed-routing configured

> kubectl get ing
NAME            HOSTS                                                          ADDRESS     PORTS     AGE
mixed-routing   v1.127.0.0.1.nip.io,v2.127.0.0.1.nip.io,mix.127.0.0.1.nip.io   localhost   80, 443   45h

> kubectl describe ing mixed-routing
Name:             mixed-routing
Namespace:        default
Address:          localhost
Default backend:  default-http-backend:80 (<none>)
TLS:
  tls-localhost terminates mix.127.0.0.1.nip.io
Rules:
  Host                  Path              Backends
  ----                  ----              --------
  v1.127.0.0.1.nip.io                     hello-app-v1:8080 (10.1.1.44:8080,10.1.1.45:8080)
  v2.127.0.0.1.nip.io                     hello-app-v2:8080 (10.1.1.46:8080,10.1.1.47:8080)
  mix.127.0.0.1.nip.io
                        /echo/?(.*)       echo-app:8080 (10.1.1.42:8080,10.1.1.43:8080)
                        /dumpster/?(.*)   dumpster-app:8080 (10.1.1.40:8080,10.1.1.41:8080)
Annotations:
  kubectl.kubernetes.io/last-applied-configuration:  {"apiVersion":"networking.k8s.io/v1beta1","kind":"Ingress","metadata":{"annotations":{"nginx.ingress.kubernetes.io/rewrite-target":"/$1"},"name":"mixed-routing","namespace":"default"},"spec":{"rules":[{"host":"v1.127.0.0.1.nip.io","http":{"paths":[{"backend":{"serviceName":"hello-app-v1","servicePort":8080}}]}},{"host":"v2.127.0.0.1.nip.io","http":{"paths":[{"backend":{"serviceName":"hello-app-v2","servicePort":8080}}]}},{"host":"mix.127.0.0.1.nip.io","http":{"paths":[{"backend":{"serviceName":"echo-app","servicePort":8080},"path":"/echo/?(.*)"},{"backend":{"serviceName":"dumpster-app","servicePort":8080},"path":"/dumpster/?(.*)"}]}}],"tls":[{"hosts":["mix.127.0.0.1.nip.io"],"secretName":"tls-localhost"}]}}

  nginx.ingress.kubernetes.io/rewrite-target:  /$1
Events:
  Type    Reason  Age                  From                      Message
  ----    ------  ----                 ----                      -------
  Normal  UPDATE  4m56s (x2 over 45h)  nginx-ingress-controller  Ingress default/mixed-routing
```

weryfikujemy, że certyfikat został faktycznie podpięty pod `mix.127.0.0.1.nip.io`
```
> curl --insecure -v https://mix.127.0.0.1.nip.io/ 2>&1 | awk 'BEGIN { cert=0 } /^\* SSL connection/ { cert=1 } /^\*/ { if (cert) print }'

* SSL connection using TLSv1.2 / ECDHE-RSA-AES256-GCM-SHA384
* ALPN, server accepted to use h2
* Server certificate:
*  subject: C=PL; ST=Some-State; L=POZNAN; O=krzyskowk; CN=mix.127.0.0.1.nip.io; emailAddress=krzysztof.krzyskow@gmail.com
*  start date: Jan  9 19:00:16 2020 GMT
*  expire date: Jan  8 19:00:16 2021 GMT
*  issuer: C=PL; ST=Some-State; L=POZNAN; O=krzyskowk; CN=mix.127.0.0.1.nip.io; emailAddress=krzysztof.krzyskow@gmail.com
*  SSL certificate verify result: self signed certificate (18), continuing anyway.
* Using HTTP2, server supports multi-use
* Connection state changed (HTTP/2 confirmed)
* Copying HTTP/2 data in stream buffer to connection buffer after upgrade: len=0
* Using Stream ID: 1 (easy handle 0x7fffe1e8f7e0)
* Connection state changed (MAX_CONCURRENT_STREAMS updated)!
* Connection #0 to host mix.127.0.0.1.nip.io left intact
```
weryfikuemy również ze np. `v1.127.0.0.1.nip.io` nie otrzymal stworzonego przez nas certyfikatu
```
>curl --insecure -v https://v1.127.0.0.1.nip.io/ 2>&1 | awk 'BEGIN { cert=0 } /^\* SSL connection/ { cert=1 } /^\*/ { if (cert) print }'

* SSL connection using TLSv1.2 / ECDHE-RSA-AES256-GCM-SHA384
* ALPN, server accepted to use h2
* Server certificate:
*  subject: O=Acme Co; CN=Kubernetes Ingress Controller Fake Certificate
*  start date: Jan  7 18:20:35 2020 GMT
*  expire date: Jan  6 18:20:35 2021 GMT
*  issuer: O=Acme Co; CN=Kubernetes Ingress Controller Fake Certificate
*  SSL certificate verify result: unable to get local issuer certificate (20), continuing anyway.     * Using HTTP2, server supports multi-use
* Connection state changed (HTTP/2 confirmed)
* Copying HTTP/2 data in stream buffer to connection buffer after upgrade: len=0
* Using Stream ID: 1 (easy handle 0x7fffe824a7e0)
* Connection state changed (MAX_CONCURRENT_STREAMS updated)!
* Connection #0 to host v1.127.0.0.1.nip.io left intact
```