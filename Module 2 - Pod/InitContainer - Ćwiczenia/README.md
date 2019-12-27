# Ćwiczenie

Celem ćwiczenia jest pobranie strony www z repozytorium git za pomocą kontenera typu init. Pobrane dane muszą trafić na wolumen typu emptyDir i zostać wykorzystane do serwowania treści w głównym kontenerze.

- Do kontenera init z git zbuduj obraz na bazie ubuntu. Należy doinstalować git.
- Do kontenera serwującego treść wykorzystaj nginx.
- Jeśli nie posiadasz strony w repo możesz wykorzystać https://github.com/PoznajKubernetes/poznajkubernetes.github.io.

---

tworzymy bazowy pod.yaml

```
kubectl run web --image=nginx --restart=Never --dry-run -o yaml > pod.yml
```

dodajemy `volume` i `initContainer`

```
apiVersion: v1
kind: Pod
metadata:
  creationTimestamp: null
  name: web
spec:
  initContainers:
    - image: ubuntu:latest
      name: setup
      volumeMounts:
        - name: workdir
          mountPath: "/repo"
      command: ['bin/sh', '-c', 'apt-get update', 'apt-get install git', 'git clone https://github.com/KrzyskowK/poznajKubernetes.git /repo']
  containers:
    - image: nginx:1.17.6
      name: web
      ports:
        - containerPort: 80
      volumeMounts:
        - name: workdir
          mountPath: /usr/share/nginx/html
  dnsPolicy: ClusterFirst
  restartPolicy: Never
  volumes:
    - name: workdir
      emptyDir: {}
```

odpalamy i czemay na status `Running`

```
kubectl apply -f pod.yml 
```

nastepnie

```
kubectl port-forward webx 8080:80
```

i wchodzimy na http://localhost:8080/  :+1: 