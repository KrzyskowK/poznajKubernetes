# Grupowanie i Wybieranie

## Labels

dodanie etykiety
```
kubectl label pods <NAME> labelName=labelValue labelName1=labelValue1
```

aktualizacja etykiety
```
kubectl label pods <NAME> --overwrite labelName=labelValue
```

usunięcie etykiety
```
kubectl label pods <NAME> labelName-
```

dodanie etykiety do wszystkich pod
```
kubectl label pods --all labelName=labelValue
```

pokaż wszystkie etykiety zdefiniowane w zasobie
```
kubectl get <RESOURCE> --show-labels
```

pokaż etykietę labelName jako nową kolumnę w liście Podów
```
# zamiast -L można stosować --label-columns
kubectl get pods -L <labelName>
kubectl get pods -L <labelName>,<labelName1>
```

### selectors

pokaż tylko i wyłącznie Pody które mają etykietę labelName
```
# zamiast --selector można stosowoać -l (małe l)
kubectl get pods --selector <labelName>
kubectl get pods --selector '!<labelName>'
kubectl get pods --selector <labelName>=<val1>
kubectl get pods --selector '<labelName>=<val1>, <labelName1>=<val2>'
kubectl get pods --selector '<labelName> in (<val1>, <val2>)'
kubectl get pods --selector '<labelName> notin (<val1>, <val2>)'
```

### field-selector

do przeszukiwania yaml

```
kubectl get pods --field-selector=status.phase=Running
kubectl get pods --field-selector=status.phase!=Running
```

### jsonpath selector

```
kubectl get pods -o=jsonpath='{.items[0].metadata.name}'
kubectl get pods -o=jsonpath='{.items[*].metadata.name}'
kubectl get pods -o=jsonpath='{range .items[*]}{.metadata.name}{"\n"}{end}'
kubectl get pods -o=jsonpath='{range .items[*]}{.metadata.name}{"\t"}{.kind}{"\n"}{end}'
kubectl get pods -o=jsonpath='{.items[?(@.spec.terminationGracePeriodSeconds==30)].metadata.name}'
```

### sort by

```
kubectl get pods --sort-by=.status.phase
kubectl get pods --sort-by=.metadata.name
kubectl get pods --sort-by=.metadata.creationTimestamp
```

### rekomendowane etykiety
```
apiVersion: apps/v1
kind: StatefulSet
metadata:
  labels:
    app.kubernetes.io/name: mysql
    app.kubernetes.io/instance: wordpress-abcxzy
    app.kubernetes.io/version: "5.7.21"
    app.kubernetes.io/component: database
    app.kubernetes.io/part-of: wordpress
    app.kubernetes.io/managed-by: helm
```

## Annotations

- dodatkowy sposób opisu zasobów
- można po nich filtrowac/szukać jedynie za pomoca jsonpath
- widac je jedynie w describe

przykład:

```
apiVersion: v1
kind: Pod
metadata:
  name: pkad
  annotations:
    contoso.com/author: "Jakub Wędrowycz"
    contoso.com/repository: "http://github.com/contoso/pkad"
    contoso.com/support: "support@contoso.com"
    contoso.com/description: "Simple app showing k8s functionality"
...
```