# Ćwiczenia

## Stwórz CronJob tak aby:

- co 2 minuty tworzył on Job
- stworzony Job powinien tworzyć 2 lub więcej chodzące Pod
- Pody powinny chodzić więcej niż 2 minuty. Możesz na stałe zaszyć 3 minuty 🙂
- używając parametru concurrencyPolicy spróbuj uzyskać efekt, aby nowo utworzone Pod zastępowały stare, czyli nigdy żaden z Pod się nie zakończy

Tworzymy template cronjob
```
> kubectl create cronjob cronjob-demo --image=busybox --schedule="*/2 * * * 
*" --dry-run -o yaml > cronjob-demo.yaml
```

Po drobnych modyfikacjach
```
apiVersion: batch/v1beta1
kind: CronJob
metadata:
  name: cronjob-demo
spec:
  schedule: '*/2 * * * *'
  concurrencyPolicy: Replace
  jobTemplate:
    metadata:
      name: cronjob-demo
    spec:
      completions: 2
      parallelism: 2
      template:
        metadata:
          name: busybox
        spec:
          containers:
          - image: busybox
            name: cronjob-demo
            args: 
            - "sleep"
            - "3000"
          restartPolicy: OnFailure
```

dodajemy nasz `cronjob` do konfiguracji klastra
```
> kubectl apply -f .\cronjob-demo.yaml
cronjob.batch/cronjob-demo created
```

obserwujemy że `cronjob-demo` tworzy job `cronjob-demo-1579027800` który do zakończenia potrzebuje 2 podów
`cronjob-demo-1579027800-s8xpv` i `cronjob-demo-1579027800-42c2g`
```
> kubectl get cronjob -w
NAME           SCHEDULE      SUSPEND   ACTIVE   LAST SCHEDULE   AGE
cronjob-demo   */2 * * * *   False     0        <none>          0s
cronjob-demo   */2 * * * *   False     1        0s              53s

> kubectl get job -w
NAME                      COMPLETIONS   DURATION   AGE
cronjob-demo-1579027800   0/2                      0s
cronjob-demo-1579027800   0/2           0s         0s

> kubectl get pod -w
NAME                            READY   STATUS    RESTARTS   AGE
cronjob-demo-1579027800-s8xpv   0/1     Pending   0          0s
cronjob-demo-1579027800-42c2g   0/1     Pending   0          0s
cronjob-demo-1579027800-s8xpv   0/1     Pending   0          0s
cronjob-demo-1579027800-42c2g   0/1     Pending   0          0s
cronjob-demo-1579027800-s8xpv   0/1     ContainerCreating   0          0s
cronjob-demo-1579027800-42c2g   0/1     ContainerCreating   0          0s
cronjob-demo-1579027800-s8xpv   1/1     Running             0          5s
cronjob-demo-1579027800-42c2g   1/1     Running             0          7s
```

po 2 minutach `cronjob-demo` uruchamia nowy job `cronjob-demo-1579027920`.
Stary job `cronjob-demo-1579027800` nie został zakończony ponieważ pody nie przeszły do stanu `completed`
W wyniku zastosowania `concurrencyPolicy: Replace` starty job oraz pody `cronjob-demo-1579027800-*` zostają usunięte a w ich miejsce zostają stworzone dwa nowe pody `cronjob-demo-1579027920-2vrpd` i `cronjob-demo-1579027920-99f2z`
```
> kubectl get cronjob -w
NAME           SCHEDULE      SUSPEND   ACTIVE   LAST SCHEDULE   AGE
cronjob-demo   */2 * * * *   False     0        <none>          0s
cronjob-demo   */2 * * * *   False     1        0s              53s
cronjob-demo   */2 * * * *   False     1        0s              2m53s

> kubectl get job -w
NAME                      COMPLETIONS   DURATION   AGE
cronjob-demo-1579027800   0/2                      0s
cronjob-demo-1579027800   0/2           0s         0s
cronjob-demo-1579027800   0/2           2m         2m
cronjob-demo-1579027920   0/2                      0s
cronjob-demo-1579027920   0/2           0s         0s

> kubectl get pod -w
NAME                            READY   STATUS    RESTARTS   AGE
cronjob-demo-1579027800-s8xpv   0/1     Pending   0          0s
cronjob-demo-1579027800-42c2g   0/1     Pending   0          0s
cronjob-demo-1579027800-s8xpv   0/1     Pending   0          0s
cronjob-demo-1579027800-42c2g   0/1     Pending   0          0s
cronjob-demo-1579027800-s8xpv   0/1     ContainerCreating   0          0s
cronjob-demo-1579027800-42c2g   0/1     ContainerCreating   0          0s
cronjob-demo-1579027800-s8xpv   1/1     Running             0          5s
cronjob-demo-1579027800-42c2g   1/1     Running             0          7s
cronjob-demo-1579027920-2vrpd   0/1     Pending             0          0s
cronjob-demo-1579027920-2vrpd   0/1     Pending             0          0s
cronjob-demo-1579027920-99f2z   0/1     Pending             0          0s
cronjob-demo-1579027920-99f2z   0/1     Pending             0          0s
cronjob-demo-1579027920-2vrpd   0/1     ContainerCreating   0          1s
cronjob-demo-1579027920-99f2z   0/1     ContainerCreating   0          1s
cronjob-demo-1579027800-s8xpv   1/1     Terminating         0          2m1s
cronjob-demo-1579027800-42c2g   1/1     Terminating         0          2m1s
cronjob-demo-1579027920-2vrpd   1/1     Running             0          5s
cronjob-demo-1579027920-99f2z   1/1     Running             0          7s
```

Sytuacja ta będzie się powtarzać co 2 minuty
