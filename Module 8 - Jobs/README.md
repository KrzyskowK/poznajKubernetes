# Zadania wsadowe i cykliczne

## Job

### Job Controller

- JobController zarządza Jobami
- Job tworzy Pod
- Pody nie są usuwane dopóki job nie zostanie usunięty
- upewnia sie ze pody zostana uruchomione i __zakonczone z sukcesem__

przykładowy template:
```
apiVersion: batch/v1
kind: job
metadata:
  name: run-once-job
spec:
  backoffLimit: 2
  activeDeadlineSeconds: 100 
  completions: 5
  parallelism: 3 # ile jobow pracuje na raz (jezeli jeden pod zakon)
  template:
    metadata:
      name: run-once
    spec:
      containers:
      - name: run-once-pod
        image: busybox
        args:
          - "echo"
          - "cześć"
    restartPolicy: OnFailure # lub Never
```

### job - backoffLimit
- ile razy job controller bedzie probowal uruchamiac błednie zakonczone job (domyslnie 6)

### job - activeDeadlineSeconds
- czas po którym zakończy się wykonywanie job (domyslnie 100sec)

### job - completions
- ilość powtórek zakończonych z sukcesem (domyślnie 1)
- tyle pod zostanie uruchomionych sekwencyjnie (o ile nie utawimy paralellism)

### job - parallelism
- ile pod zostanie uruchomionych równolegle
- jeżeli jeden pod zakonczy sie sukcesem job uznawany jest za zakonczony sukcesem
- jezeli jeden pod zakonczy prace pozostale pod zostają wyłaczone
- (domyślnie 1)

### job - completions + parallelism
- jezeli mamy: `completions: 5` i `parallelism: 3` to najpierw uruchomią się 3 pody a po zakończeniu następne 2

tworzenie template
```
kubectl create job <NAME> --image=<IMAGE> --dry-run -o yaml > job.yaml
```
zarządzanie 
```
kubectl describe job/<NAZWA>
kubectl logs job/<NAZWA>
kubectl delete job <NAZWA>
```
zarządzanie gdy mamy więcej niż jeden pod
```
kubectl get pods --selector=job-name=<NAZWA> --output-jsonpath='{.items[*].metadata.name}'

kubectl logs --selector=job-name=<NAME>
```


## CronJob

- do wykonywania zadań cyklicznych
- format rozszerzonego crone `vixie cron`, wspiera `/`
- dodatkow można używać słów kluczowy np `@hourly`

template:
```
apiVersion: batch/v1beta1
kind: CronJob
metadata:
  name: cron
spec:
  schedule: "*/1 * * * *"
  concurrencyPolicy: Allow
  startingDeadlineSeconds: 100
  jobTemplate:
    spec:
      template:
        spec: 
          containers:
          - name: cron
            image: busbox
            args:
            - "echo"
            - "cześć"
          restartPolicy: OnFailure
```

tworzenie cronejob
```
kubectl create cronjob <NAZWA> --image=<OBRAZ> --schedule="*/1 * * * *" -o yaml --dry-run
```

uruchomienie cronejob jako job (do debugu)
```
kubectl create job --from=cronJob/<NAZWA CRON> <NAZWA_JOB>
```

### CronJob - concurrencyPolicy
- jak traktować równolegle uruchomine job
- `Allow` (domyślne)
- `Forbid` - nie uruchamiaj jeżeli stary działa
- `Replace` - zastąp jeżeli stary działa

### CronJon - startingDeadlineSeconds
- liczba sekund jaką mamy pominąc jeżelic chcemy uruchomic x pominietych jobów na raz

### CronJon - successfulJobsHistoryLimit
- liczba trzymanych w historii jobów zakończonych z sukcesem

### CronJon - failedJobsHistoryLimit
- liczba trzymanych w historii jobów zakończonych z failem