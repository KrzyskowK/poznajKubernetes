
# Pod - Ćwiczenia

## Ćwiczenia nr 1

### 1. Utwórz definicje (plik YAML) pod z obrazem busybox za pomocą polecenia kubectl run z opcjami lub używając edytora tekstowego. Pamiętaj o opcjach --dry-run oraz -o yaml

```
> kubectl run busybox --image=busyboPod>x --restart=Never --dry-run -o yaml > busybox.yml
```

```
apiVersion: v1
kind: Pod
metadata:
  creationTimestamp: null
  labels:
    run: busybox
  name: busybox
spec:
  containers:
  - image: busybox
    name: busybox
    resources: {}
  dnsPolicy: ClusterFirst
  restartPolicy: Never
status: {}
```

### 2. Używając polecenia kubectl create stwórz Pod na podstawie definicji z punktu 1.

```
> kubectl create -f busybox.yml
```

### 3. Sprawdź status tworzenia Pod za pomocą get. Możesz też ustawić w osobnym oknie sprawdzanie „ciągłe” z opcją -w czyli --watch i zostawić je do końca ćwiczeń.

```
> kubectl get pods -w

NAME        READY   STATUS    RESTARTS   AGE
busybox     0/1     ContainerCreating   0          0s
busybox     1/1     Running             0          6s
busybox     0/1     Completed           0          11s       
```

### 4. Zastanów się dlaczego Pod przeszedł w stan Completed.

ponieważ kontener wykonał swoją pracę i zakończył działanie

## Ćwiczenia nr 2

### 1. Korzystając ze stanu klastra z ćwiczenia 1 oraz pliku YAML, przystąp do ćwiczenia.

```
kubectl get pods

NAME       READY   STATUS        RESTARTS   AGE
busybox    1/1     Completed       0          3m26s    
```

### 2. Popraw plik YAML dodając command tak by Pod nie kończył od razu swojej pracy (pamiętaj dwóch sposobach notacji command). Następnie dodaj lub zmień tag w obrazie. Poprawną wartość tag dla busybox znajdziesz na stronie: https://hub.docker.com/_/busybox


```
apiVersion: v1
kind: Pod
metadata:
  creationTimestamp: null
  labels:
    run: busybox
  name: busybox
spec:
  containers:
  - image: busybox:1.31.1
    name: busybox
    resources: {}
    command:
      - sleep
      - "1000"
  dnsPolicy: ClusterFirst
  restartPolicy: Never
status: {}
```

### 3. Użyj polecenia kubectl create by wgrać nową definicję.

```
> kubectl create -f busybox.yml
Error from server (AlreadyExists): error when creating "busybox.yml": pods "busybox" already exists
```

### 4. Zastanów się dlaczego nie działa.

nie można ponownie stworzyć poda który już istnieje

### 5. Usuń starą definicję za pomocą kubectl delete.

```
> kubectl delete pod busybox
pod "busybox" deleted

> kubectl get pods
No resources found.
```

## Ćwiczenia nr 3

### 1. Utwórz w YAML definicję Pod, który nie kończy od razu swojej pracy. Możesz skorzystać z pliku stworzonego YAML w ćwiczeniu 2.
### 2 .Wgraj go na klaster używając polecenia create.

```
> kubectl create -f busybox.yml --save-config=true
pod/busybox created
```

### 3. Wykonaj modyfikację pliku np: zmień czas dla komendy sleep

```
...
    command:
      - sleep
      - "300"
...
```

### 4. Spróbuj wgrać modyfikacje na klaster używając create lub apply.

`create` nie zadziała z tego samego powodu co porzpednio
```
> kubectl create -f busybox.yml
Error from server (AlreadyExists): error when creating "busybox.yml": pods "busybox" already exists
```

`apply` nie pozwoli nam zmodyfikować poda po zmianie parametrów w command
```
> kubectl apply -f busybox.yml
The Pod "busybox" is invalid: spec: Forbidden: pod updates may not change fields other than `spec.containers[*].image`, `spec.initContainers[*].image`, `spec.activeDeadlineSeconds` or `spec.tolerations` (only additions to existing tolerations)
...
```

### 5. Zastanów się i opisz na grupie kiedy warto używać create, a kiedy apply.

`create` - to podejscie imperatywne, `apply` - deklaratywne.
Używałbym `apply` ze względu na to że jest bardziej uniwersalne.

## Ćwiczenie 4

### 1. Posprzątaj swój klaster tak by nie było na nim stworzonych przez Ciebie Pod.

```
> kubectl delete pod busybox
pod "busybox" deleted

> kubectl get pods
No resources found.
```

### 2. Czy masz pomysł jak to zautomatyzować?

dokumentacji https://kubernetes.io/docs/reference/generated/kubectl/kubectl-commands#delete podpowiada że można się posłużyć:
```
kubectl delete pod --field-selector=status.phase==Succeeded  
```



```
> kubectl get pods
NAME       READY   STATUS      RESTARTS   AGE
busybox    1/1     Running     0          4m34s
busybox1   1/1     Running     0          44s
busybox2   0/1     Completed   0          27s
busybox3   0/1     Completed   0          16s

> kubectl delete pod --field-selector=status.phase==Succeeded    
pod "busybox2" deleted
pod "busybox3" deleted

> kubectl get pods                                               
NAME       READY   STATUS    RESTARTS   AGE
busybox    1/1     Running   0          4m45s
busybox1   1/1     Running   0          55s
```