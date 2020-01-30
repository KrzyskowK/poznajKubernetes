# Obiekt StatefulSets – Ćwiczenia

### Na podstawie lekcji przetestuj:

- Tworzenie i aktualizację StatefulSets
- Działanie serwisu typu headless
- Skalowanie StatefulSets

Połącz wiedzę na temat kontenera typu init oraz wolumenów z StatefulSets. Za pomocą kontenera typu init pobierz stronę z repozytorium git. Pobrane dane muszą trafić na trwały wolumen i zostać wykorzystane do serwowania treści w głównym kontenerze (wykorzystaj nginx). Skorzystaj z materiałów po ćwiczeniach o kontenera typu init.

---

### Tworzenie i aktualizację StatefulSets

Manifest dna naszego statefulset
```
apiversion: apps/v1
kind: StatefulSet
metadata:
  name: nginx
spec:
  replicas: 3
  selector:
    matchlabels:
      app: nginx
  template:
    metadata:
      labels:
        app: nginx
    spec:
      initContainers:
      - image: ubuntu:latest
        name: setup
        volumeMounts:
          - name: workdir
            mountPath: /repo
        command: ["/bin/sh", "-c"]
        args:
          - apt-get update;
            apt-get install -y git;
            git clone "https://github.com/KrzyskowK/poznajKubernetes.git" /repo;
    containers:
      - image: nginx:1.17.6
        name: nginx
        ports:
          - containerPort: 80
        volumeMounts:
          - name: workdir
            mountPath: /usr/share/nginx/html
    volumes:
      - name: workdir
        emptyDir: {}
```

Wgrywamy konfigurację na klaster
```
> kubectl apply -f .\statefulset.yaml
statefulset.apps/nginx created
```
Obserwujemy jak kolejno tworzone są instancje podów `nginx-0`, `nginx-1`, `nginx-2`

```
> kubectl get statefulset -w
NAME    READY   AGE
nginx   0/3     0s
nginx   0/3     0s
nginx   1/3     40s
nginx   2/3     81s
nginx   3/3     118s

> kubectl get pod -w
NAME      READY   STATUS    RESTARTS   AGE
nginx-0   0/1     Pending   0          0s
nginx-0   0/1     Pending   0          0s
nginx-0   0/1     Init:0/1   0          0s
nginx-0   0/1     Init:0/1   0          4s
nginx-0   0/1     PodInitializing   0          38s
nginx-0   1/1     Running           0          40s
nginx-1   0/1     Pending           0          0s
nginx-1   0/1     Pending           0          0s
nginx-1   0/1     Init:0/1          0          0s
nginx-1   0/1     Init:0/1          0          4s
nginx-1   0/1     PodInitializing   0          40s
nginx-1   1/1     Running           0          41s
nginx-2   0/1     Pending           0          0s
nginx-2   0/1     Pending           0          0s
nginx-2   0/1     Init:0/1          0          0s
nginx-2   0/1     Init:0/1          0          4s
nginx-2   0/1     PodInitializing   0          36s
nginx-2   1/1     Running           0          37s
```
Sprawdźmy co się stanie jeżeli teraz spróbujemy usunąć instancję `nginx-1`
```
> kubectl delete pod nginx-1
pod "nginx-1" deleted

> kubectl get pod -w
NAME      READY   STATUS    RESTARTS   AGE
nginx-1   1/1     Terminating       0          10m
nginx-1   0/1     Terminating       0          10m
nginx-1   0/1     Terminating       0          10m
nginx-1   0/1     Terminating       0          10m
nginx-1   0/1     Pending           0          0s
nginx-1   0/1     Pending           0          0s
nginx-1   0/1     Init:0/1          0          0s
nginx-1   0/1     Init:0/1          0          5s
nginx-1   0/1     PodInitializing   0          44s
nginx-1   1/1     Running           0          45s
```
`nginx-1` zostaje odtworzony przez statefulset

Spróbujmy teraz zamienić `volumen` z `emptyDir` na `PersistenVolumeClaim`.
Dodajemy do spec sekcję `volueClaimTemplates` która utworzyc PVC które powinno skorzystać z defaultowego PeristentVolumeProvisioner do stowrzenia odpowiedniego PersistanVolume.
```
  volumeClaimTemplates:
    - metadata:
        name: workdir
      spec:
        accessModes: [ "ReadWriteOnce"]
        resources:
          requests:
            storage: 10Mi
```
⚠️ dodatkowo z templatu poda całkowicie usuwamy sekcję volumes!

Próbujemy teraz zaktualizować naszą konfigurację - co niestety nie powiedzie się 
```
> kubectl apply -f .\statefulset-pvc.yaml
The StatefulSet "nginx" is invalid: spec: Forbidden: updates to statefulset spec for fields other than 'replicas', 'template', and 'updateStrategy' are forbidden
```
Spróbujmy zatem usunąć statefulset bez usuwania podów
```
> kubectl delete statefulset nginx --cascade=false
statefulset.apps "nginx" deleted

> kubectl get pod
NAME      READY   STATUS    RESTARTS   AGE
nginx-0   1/1     Running   0          28m
nginx-1   1/1     Running   0          16m
nginx-2   1/1     Running   0          26m

> kubectl get statefulset
No resources found.
```
Dodajemy teraz konfigurację ponownie
```
> kubectl apply -f .\statefulset-pvc.yaml
statefulset.apps/nginx created
```

Widzimy, że PV i PVC zotaly utworzone zgodnie z templatem, jednak tylko dla pierwszego pod `nginx-0`
```
> kubectl get pv
NAME                                       CAPACITY   ACCESS MODES   RECLAIM POLICY   STATUS   CLAIM                  STORAGECLASS   REASON   AGE
pvc-39fe1bb9-43a5-11ea-b471-00155d006a01   10Mi       RWO            Delete           Bound    default/workdir-nginx-0   hostpath                65s

> kubectl get pvc
NAME           STATUS   VOLUME                                     CAPACITY   ACCESS MODES   STORAGECLASS   AGE
workdir-nginx-0   Bound    pvc-39fe1bb9-43a5-11ea-b471-00155d006a01   10Mi       RWO            hostpath       68s
```
Statefulset nie był w stanie zaktualizować podów automatycznie 
Powodem tego możemy znaleźć w eventach statefulset - podmiana volumeMount nie jest obslugiwana przez automatyczny rollout.
```
> kubectl get statefulset
NAME    READY   AGE
nginx   0/3     38s

> kubectl rollout history statefulset nginx
statefulset.apps/nginx 
REVISION
0
1

> kubectl describe statefulset nginx
...
Events:
  Type     Reason            Age                From                    Message
  ----     ------            ----               ----                    -------
  Normal   SuccessfulCreate  23s                statefulset-controller  create Claim workdir-nginx-0 Pod nginx-0 in StatefulSet nginx success
  Warning  FailedUpdate      3s (x14 over 23s)  statefulset-controller  update Pod nginx-0 in StatefulSet nginx failed error: Pod "nginx-0" is invalid: spec: Forbidden: pod updates may not change fields other than `spec.containers[*].image`, `spec.initContainers[*].image`, `spec.activeDeadlineSeconds` or `spec.tolerations` (only additions to existing tolerations)
{"Volumes":[{"Name":"workdir","HostPath":null,"EmptyDir": ....
```
spróbujmy usunąć ręcznie ostatni pod i sprawdzić czy statefulset oddtworzy `ngnix-2` w nowej wersji
```
> kubectl delete pod nginx-2
pod "nginx-2" deleted

> kubectl get pod
NAME      READY   STATUS    RESTARTS   AGE
nginx-0   1/1     Running   0          65m
nginx-1   1/1     Running   0          54m

> kubectl get statefulset
NAME    READY   AGE
nginx   0/3     19m
```
⚠️ Widzimy, że pomimo ręcznego usunięcia pod nie został odtworzony. 

💡 Przypomnijmy sobie jednak, że statefulset w obecnej konfiguracji zawsze rozpoczyna "świeży" deployment od pierwszego indeksu, sprawdźmy zatem czy usunięcie `nginx-0` spowoduje deployment nowej wersji

```
> kubectl delete pod nginx-0
pod "nginx-0" deleted

> kubectl get pod -w
NAME      READY   STATUS    RESTARTS   AGE
nginx-0   0/1     Terminating   0          8m19s
nginx-0   0/1     Pending       0          0s
nginx-0   0/1     Pending       0          0s
nginx-0   0/1     Init:0/1      0          0s
nginx-0   0/1     Init:0/1      0          4s
nginx-0   0/1     PodInitializing   0          43s
nginx-0   1/1     Running           0          44s

> kubectl get statefulset
NAME    READY   AGE
nginx   1/3     7m14s
```

Usuńmy zatem kolejny pod `nginx-1` i pozwólmyna dokończenie rolloutu nowej wersji.

Widzimy, że pozostałe pody zostają poprawnie utworzone. PV i PVC są teraz również dostępne dla wszystkich podów.
```
> kubectl delete pod nginx-1
pod "nginx-1" deleted

> kubectl get pod
NAME      READY   STATUS    RESTARTS   AGE
nginx-0   1/1     Running   0          11m
nginx-1   1/1     Running   0          3m
nginx-2   1/1     Running   0          2m20s

> kubectl get statefulset
NAME    READY   AGE
nginx   3/3     14m

> kubectl get pvc
NAME              STATUS   VOLUME                                     CAPACITY   ACCESS MODES   STORAGECLASS   AGE
workdir-nginx-0   Bound    pvc-39fe1bb9-43a5-11ea-b471-00155d006a01   10Mi       RWX            hostpath       14m
workdir-nginx-1   Bound    pvc-d80c4202-43a5-11ea-b471-00155d006a01   10Mi       RWX            hostpath       10m
workdir-nginx-2   Bound    pvc-f3ceb5ba-43a6-11ea-b471-00155d006a01   10Mi       RWX            hostpath       2m31s

> kubectl get pv
NAME                                       CAPACITY   ACCESS MODES   RECLAIM POLICY   STATUS   CLAIM                     STORAGECLASS   REASON   AGE
pvc-39fe1bb9-43a5-11ea-b471-00155d006a01   10Mi       RWX            Delete           Bound    default/workdir-nginx-0   hostpath                15m
pvc-d80c4202-43a5-11ea-b471-00155d006a01   10Mi       RWX            Delete           Bound    default/workdir-nginx-1   hostpath                10m
pvc-f3ceb5ba-43a6-11ea-b471-00155d006a01   10Mi       RWX            Delete           Bound    default/workdir-nginx-2   hostpath                2m42s
```

Sprawdźmy teraz czy przy aktualizacji wersji obrazu (`nginx:1.17.6 -> 1.17`) startegia rollingUpdate zadziała zgodnie z oczekiwaniami.

```
> kubectl apply -f .\statefulset-pvc.yaml
statefulset.apps/nginx configured

> kubectl get pod -w
NAME      READY   STATUS     RESTARTS   AGE
nginx-0   1/1     Running    0          18m
nginx-1   1/1     Running    0          10m
nginx-2   0/1     Init:0/1   0          4s
nginx-2   0/1     Init:0/1   0          5s
nginx-2   0/1     Init:Error   0          42s
nginx-2   0/1     Init:0/1     1          45s
```

Rollout rozpoczyna się od ostatniego poda. Widzimy jednak, że pod `nginx-02` ma problem z inicjalizacją. Sprawdźmy zatem logi.

```
> kubectl attach pod nginx-2 -c setup

...
Setting up git (1:2.17.1-1ubuntu0.5) ...
Processing triggers for libc-bin (2.27-3ubuntu1) ...
Processing triggers for ca-certificates (20180409) ...
Updating certificates in /etc/ssl/certs...
0 added, 0 removed; done.
Running hooks in /etc/ca-certificates/update.d...
done.
fatal: destination path '/repo' already exists and is not an empty directory.
```
W logach widać, że `initContainer` nie potrafi pobrać repozytorium do folderu /repo ponieważ folder już istnieje. 

⚠️ Cała sytuacja jest oczywista, korzystając z PV zatem nasze /repo będzie usuwane przy podmianie podów (np przy deploymencie) w związku z tym musimy obsłużyć ten przypadek żeby InitContainer mógł zakończyć się sukcesem
```
  args:
    - apt-get update;
    apt-get install -y git;
    if cd repo; 
    then git pull;
    else git clone "https://github.com/KrzyskowK/poznajKubernetes.git" /repo;
    fi
```

Widzimy, że tym razem wszystkie pod zostału poprawnie zaktualizowane w kolejności od największego indeksu do najmniejszego
```
> kubectl apply -f .\statefuset-pvc-release-fixed.yaml
statefulset.apps/nginx configured

> kubectl get pod
NAME      READY   STATUS    RESTARTS   AGE
nginx-0   1/1     Running   0          103s
nginx-1   1/1     Running   0          2m33s
nginx-2   1/1     Running   0          3m23s

> kubectl get pod nginx-0 -o jsonpath="{..containers[:1].image}"
nginx:1.17
> kubectl get pod nginx-1 -o jsonpath="{..containers[:1].image}"
nginx:1.17
> kubectl get pod nginx-2 -o jsonpath="{..containers[:1].image}"
nginx:1.17

```

### Działanie serwisu typu headless