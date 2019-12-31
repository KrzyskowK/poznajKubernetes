# Etykiety – Grupowanie zasobów – Ćwiczenia

## 1. Wykonaj podstawowe operacje na etykietach imperatywnie. Takie operacje będą przydatne w późniejszych częściach szkolenia jak na przykład trzeba będzie przeanalizować niedziałający Pod albo przekierować ruch na inne Pody.

```
kubectl run bb1 --restart=Never --image=busybox 
kubectl run bb2 --restart=Never --image=busybox 
kubectl run bb3 --restart=Never --image=busybox 
```

### Dodaj etykietę

```
>kubectl label pod bb2 env=lab
pod/bb2 labeled

> kubectl get pods --show-labels
NAME   READY   STATUS      RESTARTS   AGE   LABELS
bb1    0/1     Completed   0          73s   run=bb1
bb2    0/1     Completed   0          69s   env=lab,run=bb2
bb3    0/1     Completed   0          65s   run=bb3
```

### Dodaj etykietę do wszystkich zasobów na raz

```
> kubectl label pod --all test=abc
pod/bb1 labeled
pod/bb2 labeled
pod/bb3 labeled

> kubectl get pods --show-labels  
NAME   READY   STATUS      RESTARTS   AGE    LABELS
bb1    0/1     Completed   0          2m4s   run=bb1,test=abc
bb2    0/1     Completed   0          2m     env=lab,run=bb2,test=abc
bb3    0/1     Completed   0          116s   run=bb3,test=abc
```

### Zaktualizuj etykietę

```
> kubectl label pod bb1 --overwrite test=xyz       
pod/bb1 labeled

> kubectl get pods --show-labels                   
NAME   READY   STATUS      RESTARTS   AGE     LABELS
bb1    0/1     Completed   0          2m50s   run=bb1,test=xyz
bb2    0/1     Completed   0          2m46s   env=lab,run=bb2,test=abc
bb3    0/1     Completed   0          2m42s   run=bb3,test=abc
```

### Usuń etykietę
```
> kubectl label pod bb3 run-                       
pod/bb3 labeled

> kubectl get pods --show-labels
NAME   READY   STATUS      RESTARTS   AGE     LABELS
bb1    0/1     Completed   0          3m34s   run=bb1,test=xyz
bb2    0/1     Completed   0          3m30s   env=lab,run=bb2,test=abc
bb3    0/1     Completed   0          3m26s   test=abc
```

## 2. Stwórz trzy Pody z czego dwa posiadające po dwie etykiety: app=ui i env=test oraz app=ui i env=stg, trzeci bez etykiet


```
> kubectl run bb1 --restart=Never --image=busybox 
> kubectl run bb2 --restart=Never --image=busybox 
> kubectl run bb3 --restart=Never --image=busybox 
> kubectl label pod bb1 app=ui env=test
> kubectl label pod bb2 app=ui env=stg
> kubectl label pod --all run-

> kubectl get pod --show-labels                    
NAME   READY   STATUS      RESTARTS   AGE   LABELS
bb1    0/1     Completed   0          75s   app=ui,env=test
bb2    0/1     Completed   0          71s   app=ui,env=stg
bb3    0/1     Completed   0          68s   <none>
```

### Wybierz wszystkie Pody które mają etykietę env zdefiniowaną

```
> kubectl get pod --selector env
NAME   READY   STATUS      RESTARTS   AGE
bb1    0/1     Completed   0          15m
bb2    0/1     Completed   0          15m
```

### Wybierz wszystkie Pody które nie mają etykiety env zdefiniowanej

```
> kubectl get pod --selector '!env'
NAME   READY   STATUS      RESTARTS   AGE
bb3    0/1     Completed   0          16m
```

### Wybierz Pody które mają app=ui ale nie znajdują się w env=stg

```
> kubectl get pod --selector 'app=ui, env!=stg'

NAME   READY   STATUS      RESTARTS   AGE
bb1    0/1     Completed   0          19m
```

### Wybierz Pody których env znajduje się w przedziale stg i demo

```
> kubectl get pod --selector 'env in (stg, demo)'  

NAME   READY   STATUS      RESTARTS   AGE
bb2    0/1     Completed   0          20m
```

## 3. Z wcześniej stworzonych Podów:

### Wybierz i wyświetl tylko nazwy Poda

```
> kubectl get pods -o jsonpath='{range .items[*]}{.metadata.name}{''\n''}{end}'
bb1
bb2
bb3
```

### Posortuj widok po dacie ostatniej aktualizacji Poda

zakładam że chodzi o creation timestamp
```
> kubectl get pods --sort-by=.metadata.creationTimestamp

NAME   READY   STATUS      RESTARTS   AGE
bb1    0/1     Completed   0          30m
bb2    0/1     Completed   0          30m
bb3    0/1     Completed   0          30m
```

### Wybierz tylko i wyłączenie te Pody które nie są w fazie Running

```
> kubectl get pods --field-selector=status.phase!=Running

NAME   READY   STATUS      RESTARTS   AGE
bb1    0/1     Completed   0          31m
bb2    0/1     Completed   0          30m
bb3    0/1     Completed   0          30m
```