# Konfiguracja

## Environment Variables

- we can pass them as a part of .yml in `container` section as `env`
- we can pass it as a variable to `args`


## ConfigMap

- podstawowy zasób kubernetes
- key:value
- może być wykorzystane jako `envrionment variables` lub `config files`

### Podejscie imperatywne 

tworzenie ConfigMap
```
kubectl create configmap <MAP_NAME> --from-literal=yourKey=yourValue --from-file=yourkey=yourfile.txt
```
pobranie ConfigMap
```
kubectl get configmap
```
opis ConfigMapy
```
kubectl describe configmap <MAP_NAME>
```
pobranie ConfigMapy jako yaml
```
kubectl get configmap <MAP_NAME> -o yaml
```
edycja ConfigMapy - poz apisaniu zmiany zostana automatycznie wdrozone
```
kubectl edit configmap <MAP_NAME>
```

### Podejście deklaratywne

tworzenie ConfigMapy
```
kubectl apply -f configmap.yaml
```

### ConfigMap jako env variable w POD

- `envFrom` w konfiguracji kontenera pozwala wciągnąć wszystkie zmienne z podanej `configMapRef`
```
envFrom:
    - configMapRef:          
        name: <MAP_NAME>
        optional: false
```
- `configMapKeyRef` poznawala wciągnąć pojedynczy klucz
```
env:
    - name: 'NAME'
      valueFrom:
        configMapKeyRef:
          key: <KEY>
          name: <MAP_NAME>
```

wylistowanie zmiennych środowkiskowych w pod
```
kubectl exec <POD_NAME> -- printenv
```

### ConfigMap jako volume

Możemy urzyć configMap do swtorzenia pliku konfiguracyjnego na volumenie

```
spec:
    volumes:
    - name: <VOL_NAME>
      configMap:
        name: <MAP_NAME>
        items:
          - key: <FILE_KEY>
            path: config.json
    containers:
      - name: <CONTAINER_NAME>
        volumeMounts:
          - name: <VOL_NAME>
            mountPath: /etc/config # here we will get the file /etc/config/config.json
```

Możemy też możliwośc wciagniecia każdego wpisu jako osobny plik

```
spec:
    volumes:
    - name: <VOL_NAME>
      configMap:
        name: <MAP_NAME>
    containers:
      - name: <CONTAINER_NAME>
        volumeMounts:
          - name: <VOL_NAME>
            mountPath: /etc/config # here we will get the files /etc/config/...
```

## Secret

- działa tak samo jak configMap
- przechowywany base64
- można skonfigurować szyfrowanie w bazie etcd
- aplikacje otrzymują wartości plain-text
- można ustawić blokadę dostępu za pomocą RBAC

- typy Secretow
  - *docker-registry* - dane do uwierzytelnianai w prywatnym repo dockerowym
  - generic/opaque - dowolne dane
  - tls - sekrety będące certyfikatami

  ### imperatywne

  tworzenie
  ```
  kubectl create secret <TYPE> <NAME> --from-literal=a=b --from-file=user.txt 
  ```


  pobieranie
  ```
  kubectl get secret <NAME>
  ```

### Secret jako env variable w POD

- `envFrom` w konfiguracji kontenera pozwala wciągnąć wszystkie zmienne z podanej `secretRef`
```
envFrom:
    - secretRef:          
        name: <MAP_NAME>
        optional: false
```
- `secretKeyRef` poznawala wciągnąć pojedynczy klucz
```
env:
    - name: 'NAME'
      valueFrom:
        secretKeyRef:
          key: <KEY>
          name: <MAP_NAME>
```

### Secret jako volume

Możemy urzyć secret do swtorzenia pliku konfiguracyjnego na volumenie

```
spec:
    volumes:
    - name: <VOL_NAME>
      secret:
        secretName: <SECRET_NAME>
        items:
          - key: <FILE_KEY>
            path: config.json
    containers:
      - name: <CONTAINER_NAME>
        volumeMounts:
          - name: <VOL_NAME>
            mountPath: /etc/config # here we will get the file /etc/config/config.json
```

Możemy też możliwośc wciagniecia każdego wpisu jako osobny plik

```
spec:
    volumes:
    - name: <VOL_NAME>
      secret:
        secretName: <SECRET_NAME>
    containers:
      - name: <CONTAINER_NAME>
        volumeMounts:
          - name: <VOL_NAME>
            mountPath: /etc/config # here we will get the files /etc/config/...
```

### Secret dla dostepu do dockera

tworzenie secretu z dostepem do prywatnego repo
```
docker login
cat ~/.docker/config.json

kubectl create secret generic <SECRET_NAME> --from-file=.dockerconfigjson=~/.docker/config.json --type=kubernetes.io/dockerconfigjson
```

następnie w spec POD dodajemy

```
imagePullSecrets:
  - name: <SECRET_NAME>
```

### Secret dla certyfikatow

towrzenie
```
kubectl create secret tls <NAME> --key="private.key" --cert="public.crt"
```