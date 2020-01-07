# Wdrożenie Ingress Controller – Ćwiczenia

### Zainstaluj na swoim lokalnym środowisku NGINX Ingress Controller, korzystając z prostej konfiguracji.

dodajemy generyczną konfigurację ingressa
```
> kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/nginx-0.26.2/deploy/static/mandatory.yaml

namespace/ingress-nginx created
configmap/nginx-configuration created
configmap/tcp-services created
configmap/udp-services created
serviceaccount/nginx-ingress-serviceaccount created
clusterrole.rbac.authorization.k8s.io/nginx-ingress-clusterrole created
role.rbac.authorization.k8s.io/nginx-ingress-role created
rolebinding.rbac.authorization.k8s.io/nginx-ingress-role-nisa-binding created
clusterrolebinding.rbac.authorization.k8s.io/nginx-ingress-clusterrole-nisa-binding created
deployment.apps/nginx-ingress-controller created
limitrange/ingress-nginx created
```
dodajemy konfigurację dla `docker for windows`
```
> kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/nginx-0.26.2/deploy/static/provider/cloud-generic.yaml

service/ingress-nginx created
```

stworzona konfiguracja wygląda następująco:
```
> kubectl get all -n ingress-nginx

NAME                                            READY   STATUS    RESTARTS   AGE
pod/nginx-ingress-controller-5974f8b65b-6fg5z   1/1     Running   0          9m55s

NAME                    TYPE           CLUSTER-IP     EXTERNAL-IP   PORT(S)                      AGE
service/ingress-nginx   LoadBalancer   10.108.17.80   localhost     80:30881/TCP,443:32644/TCP   78s

NAME                                       READY   UP-TO-DATE   AVAILABLE   AGE
deployment.apps/nginx-ingress-controller   1/1     1            1           9m55s

NAME                                                  DESIRED   CURRENT   READY   AGE
replicaset.apps/nginx-ingress-controller-5974f8b65b   1         1         1       9m55s
```