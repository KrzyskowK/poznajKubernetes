# Secrets – Ćwiczenia

## Część 1 – stworzenie własnego prywatnego repozytorium kontenerów

done: registry.hub.docker.com/krzyskowk/pk.demoapp:1.0.0

## Część 2 – wykorzystanie obrazu z prywatnego repozytorium

`dockerconfig.json` jest lokalnym plikiem stworzonym na podstawie ~/.docker/config.json zawierającym jedynie niezbędną authentykacje do prywatnego repo na dockerhub

```
> kubectl create secret generic regcred --from-file=.dockerconfigjson=dockerconfig.json --type=kubernetes.io/dockerconfigjson     
secret/regcred created
```

```
apiVersion: v1
kind: Pod
metadata:
  name: pkdemoapp
spec:
  containers:
  - image: krzyskowk/pk.demoapp:1.0.0
    name: pkdemoapp
  imagePullSecrets:
    - name: regcred
status: {}
```

## Część 3 – stwórz secret i wykorzystaj go w Pod
### Mając już obraz z prywatnego repozytorium, stwórz 2 rodzaje secret: --from-literal i --from-file, używając polecenia kubectl create. Gdy będziesz miał już je utworzone, spróbuj wykorzystać je jako pliki i/lub zmienne środowiskowe. Możesz skorzystać z pliku z demo:

```
> kubectl create secret generic literal-secret --from-literal=userA=abc --from-literal=passA=123
secret/literal-secret created

> echo userB=bbbb > creds.txt
> kubectl create secret generic file-secret --from-file=file=creds.txt
secret/file-secret created
```
`pod-secrets.yml`:
```
apiVersion: v1
kind: Pod
metadata:
  name: pkdemoapp
spec:
  volumes:
    - name: secret-volume
      secret:
        secretName: file-secret
  containers:
  - image: krzyskowk/pk.demoapp:1.0.0
    name: pkdemoapp
    envFrom:
    - secretRef:
        name: literal-secret
        optional: false
    volumeMounts:
      - mountPath: /etc/secret
        name: secret-volume
  imagePullSecrets:
    - name: regcred
status: {}
```

```
> kubectl apply -f pod-secrets.yml
pod/pkdemoapp created
```

secrety prawidłowo wpisane do zmiennych środowiskowych
```
> kubectl exec pkdemoapp -- printenv
PATH=/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin
HOSTNAME=pkdemoapp
passA=123
userA=abc
KUBERNETES_PORT_443_TCP_ADDR=10.96.0.1
KUBERNETES_SERVICE_HOST=10.96.0.1
KUBERNETES_SERVICE_PORT=443
KUBERNETES_SERVICE_PORT_HTTPS=443
KUBERNETES_PORT=tcp://10.96.0.1:443
KUBERNETES_PORT_443_TCP=tcp://10.96.0.1:443
KUBERNETES_PORT_443_TCP_PROTO=tcp
KUBERNETES_PORT_443_TCP_PORT=443
ASPNETCORE_URLS=http://+:80
DOTNET_RUNNING_IN_CONTAINER=true
DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true
HOME=/root
```

oraz prawidłowo zamonotowane jako plik
```
> kubectl exec pkdemoapp -- cat /etc/secret/file
creds=abc
```