# Wolumeny w Pod – Ćwiczenia

## Przypomnij sobie jak działa emptyDir i hostPath. Powtórz ćwiczenia z lekcji Kontenery typu Init – ćwiczenia dla utrwalenia informacji. Możesz zmodyfikować zadanie by pobierać i cachować repozytorium git z wykorzystaniem hostPath.


Modyfikujemy konfigurację z lekcji Kontenery typu Init zamieniając `emptyDir` na `hostPath`.
`hostpath-volume.yaml`:
```
apiVersion: v1
kind: Pod
metadata:
  name: web
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
      name: web
      ports:
        - containerPort: 80
      volumeMounts:
        - name: workdir
          mountPath: /usr/share/nginx/html
  volumes:
    - name: workdir
      hostPath:
        path: /d/k8s_vol/host_path
```

odpalamy i czemay na status `Running`

```
> kubectl apply -f hostpath-volume.yaml
> kubectl get pod -o wide
NAME   READY   STATUS    RESTARTS   AGE   IP           NODE             NOMINATED NODE   READINESS GATES
web    1/1     Running   0          67s   10.1.1.188   docker-desktop   <none>           <none>
```
nastepnie udostępniamy nginx na zwenątrz klastra
```
> kubectl port-forward web 8080:80
```
widzimy, że repo zostało poprawnie wyciągniete i nasz nginx serwuje teraz https://github.com/KrzyskowK/poznajKubernetes/blob/master/index.html
```
> curl localhost:8080
<h1>Ahoj! 🚢📦🏴‍☠️</h1>
```

możemy potwierdzić, że `hostPath` jest sharowany na poziomie NODE.
Tworzymy osobnego pod z nginx który będzie podpięty do tej samej ścieżki na `hostPath`

```
apiVersion: v1
kind: Pod
metadata:
  name: web-2
spec:
  containers:
    - image: nginx:1.17.6
      name: web-2
      ports:
        - containerPort: 80
      volumeMounts:
        - name: workdir
          mountPath: /usr/share/nginx/html
  volumes:
    - name: workdir
      hostPath:
        path: /d/k8s_vol/host_path
```
```
> kubectl apply -f .\hostpath-volume-attached-after.yaml
pod/web-2 created
> kubectl get pod -o wide
NAME    READY   STATUS    RESTARTS   AGE   IP           NODE             NOMINATED NODE   READINESS GATES
web     1/1     Running   0          11m   10.1.1.188   docker-desktop   <none>           <none>
web-2   1/1     Running   0          7s    10.1.1.190   docker-desktop   <none>           <none>
```

sprawdzamy czy oba POD nadal poprawnie serwują index.html dostępny pod wskazaną ścieżką na NODE
```
> kubectl port-forward web-2 8080:80
> curl localhost:8080
<h1>Ahoj! 🚢📦🏴‍☠️</h1>

> kubectl port-forward web 8080:80
> curl localhost:8080
<h1>Ahoj! 🚢📦🏴‍☠️</h1>
```

## Wykonaj dwa zadania z subPath (dla ułatwienia skorzystaj z ConfigMap):
### Spróbój nadpisać plik /etc/udhcpd.conf i zweryfikuj czy jego zawartosć jest poprawna i czy zawartość innych plików w katalogu jest poprawna.

tworzymy configmap z kluczem `udhcpd.conf`.
Następnie montując volumen z configmap do naszego poda wskazujemy mu za pomocą `subPath` zawartość jakiego klucza z configmap chcemy podmontować w pliku w naszym kontenerze wskaznym przez `mountPath`
```
apiVersion: v1
kind: ConfigMap
metadata:
  name: pkad-config
data:
  udhcpd.conf: "hello from configmap :wave:"
---
apiVersion: v1
kind: Pod
metadata:
  name: pkad
  labels:
    name: pkad
spec:
  containers:
  - name: pkad
    volumeMounts:
    - name: config-volume
      mountPath: /etc/udhcpd.conf
      subPath: udhcpd.conf
    image: poznajkubernetes/pkad
  volumes:
    - name: config-volume
      configMap:
        name: pkad-config
  restartPolicy: Never
```

```
> kubectl apply -f .\subpath-volume.yaml
configmap/pkad-config created
pod/pkad created
```

widzimy, że folder `/etc/` nie został w całości nadpisany przez podłączony wolumen
```
> kubectl exec pkad -- ls /etc/
alpine-release
apk
conf.d
crontabs
fstab
group
hostname
hosts
init.d
inittab
issue
logrotate.d
modprobe.d
modules
modules-load.d
motd
network
opt
os-release
passwd
periodic
profile
profile.d
protocols
resolv.conf
securetty
services
shadow
shells
ssl
sysctl.conf
sysctl.d
udhcpd.conf
```

widzimy również, że wartość klucza z configmap `pkad-config` została poprawnie wpisana do pliku
```
> kubectl exec pkad -- cat /etc/udhcpd.conf
hello from configmap :wave:
```

⚠️ Uwaga jeżeli używamy subpath, zmiany wprowadzone w `pkad-config` nie zostaną odzwierciedlone w pod automatycznie. wymagany będzie restart

modyfikujemy naszą konfigurację:
```
udhcpd.conf: "hello from configmap :wave: 3"
```
następnie wgrywamy nową wersję na klaster, i potwierdzamy że nic się nie zmieniło
```
> kubectl apply -f .\subpath-volume.yaml
configmap/pkad-config configured
pod/pkad configured

> kubectl exec pkad -- cat /etc/udhcpd.conf
hello from configmap :wave:
```

### Zrób to samo dla pliku /usr/bin/wget

zmieniamy poprzedni template tak żeby mountpath wskazywał teraz na `/usr/bin/wget`
```
apiVersion: v1
kind: Pod
metadata:
  name: pkad-wget
  labels:
    name: pkad-wget
spec:
  containers:
  - name: pkad-wget
    volumeMounts:
    - name: config-volume
      mountPath: /usr/bin/wget
      subPath: udhcpd.conf
    image: poznajkubernetes/pkad
  volumes:
    - name: config-volume
      configMap:
        name: pkad-config
  restartPolicy: Never
```

```
> kubectl apply -f .\subpath-volume-wget.yaml
pod/pkad-wget created
```

w tym wypadku jednak nie udaje nam się dostać do ścieżki. Wygląda na to że caly katalog został wyszyszczony 
```
> kubectl exec pkad-wget -- ls /usr/bin       
OCI runtime exec failed: exec failed: container_linux.go:346: starting container process caused "exec: \"ls\": executable file not found in $PATH": unknown
command terminated with exit code 126
```

nie uda nam się również wejść do kontenera
```
> kubectl exec -it  pkad-wget bin/sh        
OCI runtime exec failed: exec failed: container_linux.go:346: starting container process caused "exec: \"bin/sh\": permission denied": unknown
command terminated with exit code 126
```

⚠️⚠️⚠️

Niestety nie mam pojęcia jak poprawnie zdjagnozować ten przypadek.
Nie widzę nic ciekawego w diagnostyce POD ani w logach

```
> kubectl describe pod pkad-wget
Name:               pkad-wget
Namespace:          default
Priority:           0
PriorityClassName:  <none>
Node:               docker-desktop/192.168.65.3
Start Time:         Sun, 19 Jan 2020 23:00:16 +0100
Labels:             name=pkad-wget
Annotations:        kubectl.kubernetes.io/last-applied-configuration:
                      {"apiVersion":"v1","kind":"Pod","metadata":{"annotations":{},"labels":{"name":"pkad-wget"},"name":"pkad-wget","namespace":"default"},"spec...
Status:             Running
IP:                 10.1.1.203
Containers:
  pkad-wget:
    Container ID:   docker://55a381923d7d1184155fa953410e7bf15b28381b7293f82c5ee85c2dd5195a6d
    Image:          poznajkubernetes/pkad
    Image ID:       docker-pullable://poznajkubernetes/kuard@sha256:230ff75987cf38d9d90ac1684d445f2d02f3edfa45865a0de35bc94f4a38c83b
    Port:           <none>
    Host Port:      <none>
    State:          Running
      Started:      Sun, 19 Jan 2020 23:00:19 +0100
    Ready:          True
    Restart Count:  0
    Environment:    <none>
    Mounts:
      /usr/bin/wget from config-volume (rw,path="wget")
      /var/run/secrets/kubernetes.io/serviceaccount from default-token-48slf (ro)
Conditions:
  Type              Status
  Initialized       True
  Ready             True
  ContainersReady   True
  PodScheduled      True
Volumes:
  config-volume:
    Type:      ConfigMap (a volume populated by a ConfigMap)
    Name:      pkad-config
    Optional:  false
  default-token-48slf:
    Type:        Secret (a volume populated by a Secret)
    SecretName:  default-token-48slf
    Optional:    false
QoS Class:       BestEffort
Node-Selectors:  <none>
Tolerations:     node.kubernetes.io/not-ready:NoExecute for 300s
                 node.kubernetes.io/unreachable:NoExecute for 300s
Events:
  Type    Reason     Age    From                     Message
  ----    ------     ----   ----                     -------
  Normal  Scheduled  4m41s  default-scheduler        Successfully assigned default/pkad-wget to docker-desktop
  Normal  Pulling    4m40s  kubelet, docker-desktop  Pulling image "poznajkubernetes/pkad"
  Normal  Pulled     4m39s  kubelet, docker-desktop  Successfully pulled image "poznajkubernetes/pkad"
  Normal  Created    4m39s  kubelet, docker-desktop  Created container pkad-wget
  Normal  Started    4m38s  kubelet, docker-desktop  Started container pkad-wget
```

```
> kubectl get pod pkad-wget -o yaml 
apiVersion: v1
kind: Pod
metadata:
  annotations:
    kubectl.kubernetes.io/last-applied-configuration: |
      {"apiVersion":"v1","kind":"Pod","metadata":{"annotations":{},"labels":{"name":"pkad-wget"},"name":"pkad-wget","namespace":"default"},"spec":{"containers":[{"image":"poznajkubernetes/pkad","name":"pkad-wget","volumeMounts":[{"mountPath":"/usr/bin/wget","name":"config-volume","subPath":"wget"}]}],"restartPolicy":"Never","volumes":[{"configMap":{"items":[{"key":"udhcpd.conf","path":"wget"}],"name":"pkad-config"},"name":"config-volume"}]}}
  creationTimestamp: "2020-01-19T22:00:15Z"
  labels:
    name: pkad-wget
  name: pkad-wget
  namespace: default
  resourceVersion: "7637"
  selfLink: /api/v1/namespaces/default/pods/pkad-wget
  uid: 13205658-3b07-11ea-b471-00155d006a01
spec:
  containers:
  - image: poznajkubernetes/pkad
    imagePullPolicy: Always
    name: pkad-wget
    resources: {}
    terminationMessagePath: /dev/termination-log
    terminationMessagePolicy: File
    volumeMounts:
    - mountPath: /usr/bin/wget
      name: config-volume
      subPath: wget
    - mountPath: /var/run/secrets/kubernetes.io/serviceaccount
      name: default-token-48slf
      readOnly: true
  dnsPolicy: ClusterFirst
  enableServiceLinks: true
  nodeName: docker-desktop
  priority: 0
  restartPolicy: Never
  schedulerName: default-scheduler
  securityContext: {}
  serviceAccount: default
  serviceAccountName: default
  terminationGracePeriodSeconds: 30
  tolerations:
  - effect: NoExecute
    key: node.kubernetes.io/not-ready
    operator: Exists
    tolerationSeconds: 300
  - effect: NoExecute
    key: node.kubernetes.io/unreachable
    operator: Exists
    tolerationSeconds: 300
  volumes:
  - configMap:
      defaultMode: 420
      items:
      - key: udhcpd.conf
        path: wget
      name: pkad-config
    name: config-volume
  - name: default-token-48slf
    secret:
      defaultMode: 420
      secretName: default-token-48slf
status:
  conditions:
  - lastProbeTime: null
    lastTransitionTime: "2020-01-19T22:00:16Z"
    status: "True"
    type: Initialized
  - lastProbeTime: null
    lastTransitionTime: "2020-01-19T22:00:19Z"
    status: "True"
    type: Ready
  - lastProbeTime: null
    lastTransitionTime: "2020-01-19T22:00:19Z"
    status: "True"
    type: ContainersReady
  - lastProbeTime: null
    lastTransitionTime: "2020-01-19T22:00:15Z"
    status: "True"
    type: PodScheduled
  containerStatuses:
  - containerID: docker://55a381923d7d1184155fa953410e7bf15b28381b7293f82c5ee85c2dd5195a6d
    image: poznajkubernetes/kuard:latest
    imageID: docker-pullable://poznajkubernetes/kuard@sha256:230ff75987cf38d9d90ac1684d445f2d02f3edfa45865a0de35bc94f4a38c83b
    lastState: {}
    name: pkad-wget
    ready: true
    restartCount: 0
    state:
      running:
        startedAt: "2020-01-19T22:00:19Z"
  hostIP: 192.168.65.3
  phase: Running
  podIP: 10.1.1.203
  qosClass: BestEffort
  startTime: "2020-01-19T22:00:16Z"
```

```
> kubectl log pkad-wget
log is DEPRECATED and will be removed in a future version. Use logs instead.
2020/01/19 22:00:19 Starting pkad version: blue
2020/01/19 22:00:19 **********************************************************************
2020/01/19 22:00:19 * WARNING: This server may expose sensitive
2020/01/19 22:00:19 * and secret information. Be careful.
2020/01/19 22:00:19 **********************************************************************
2020/01/19 22:00:19 Config:
{
  "address": ":8080",
  "debug": false,
  "debug-sitedata-dir": "./sitedata",
  "keygen": {
    "enable": false,
    "exit-code": 0,
    "exit-on-complete": false,
    "memq-queue": "",
    "memq-server": "",
    "num-to-gen": 0,
    "time-to-run": 0
  },
  "liveness": {
    "fail-next": 0
  },
  "readiness": {
    "fail-next": 0
  },
  "tls-address": ":8443",
  "tls-dir": "/tls"
}
2020/01/19 22:00:19 Could not find certificates to serve TLS
2020/01/19 22:00:19 Serving on HTTP on :8080
```

sprawdźmy czy przyczyna nie leży w samym obrazie.
Spróbujmy zamontować lokalny plik w miejsce `/usr/bin/wget`
```
> docker run -it -v c:\foo\wget:/usr/bin/wget poznajkubernetes/pkad bin/sh
standard_init_linux.go:211: exec user process caused "exec format error"
```
Widzimy, że efekt jest ten sam. Dla pewności sprawdźmy, że jeżeli nie używamy volumenu obraz działa poprawnie
```
> docker run -it poznajkubernetes/pkad bin/sh
~ $ 
```
Sprawdźmy jeszcze jak zachowa się busybox image.
Widzimy, że wget zostal poprawnie nadpisany przez nasz plik.
Widzimy, że pozostałe pliki z lokacji bin nie zostały usunięte
zachowanie przy montowaniu volumenu do image busybox jest więc poprawne.
```
> cat c:/foo/wget
ups
> docker run -it -v c:\foo\wget:/bin/wget busybox
> # cat bin/wget
ups
> # ls bin
[                  chpst              du                 fstrim             ifup               logread            modprobe           pivot_root         rtcwake            sort               top                users
[[                 chroot             dumpkmap           fsync              inetd              losetup            more               pkill              run-init           split              touch              usleep
acpid              chrt               dumpleases         ftpd               init               lpd                mount              pmap               run-parts          ssl_client         tr                 uudecode
add-shell          chvt               echo               ftpget             insmod             lpq                mountpoint         popmaildir         runlevel           start-stop-daemon  traceroute         uuencode
addgroup           cksum              ed                 ftpput             install            lpr                mpstat             poweroff           runsv              stat               traceroute6        vconfig
adduser            clear              egrep              fuser              ionice             ls                 mt                 powertop           runsvdir           strings            true               vi
adjtimex           cmp                eject              getconf            iostat             lsattr             mv                 printenv           rx                 stty               truncate           vlock
ar                 comm               env                getopt             ip                 lsmod              nameif             printf             script             su                 ts                 volname
arch               conspy             envdir             getty              ipaddr             lsof               nanddump           ps                 scriptreplay       sulogin            tty                w
arp                cp                 envuidgid          grep               ipcalc             lspci              nandwrite          pscan              sed                sum                ttysize            wall
arping             cpio               ether-wake         groups             ipcrm              lsscsi             nbd-client         pstree             sendmail           sv                 tunctl             watch
ash                crond              expand             gunzip             ipcs               lsusb              nc                 pwd                seq                svc                ubiattach          watchdog
awk                crontab            expr               gzip               iplink             lzcat              netstat            pwdx               setarch            svlogd             ubidetach          wc
base64             cryptpw            factor             halt               ipneigh            lzma               nice               raidautorun        setconsole         svok               ubimkvol           wget
basename           cttyhack           fakeidentd         hd                 iproute            lzop               nl                 rdate              setfattr           swapoff            ubirename          which
bc                 cut                fallocate          hdparm             iprule             makedevs           nmeter             rdev               setfont            swapon             ubirmvol           who
beep               date               false              head               iptunnel           makemime           nohup              readahead          setkeycodes        switch_root        ubirsvol           whoami
blkdiscard         dc                 fatattr            hexdump            kbd_mode           man                nologin            readlink           setlogcons         sync               ubiupdatevol       whois
blkid              dd                 fbset              hexedit            kill               md5sum             nproc              readprofile        setpriv            sysctl             udhcpc             xargs
blockdev           deallocvt          fbsplash           hostid             killall            mdev               nsenter            realpath           setserial          syslogd            udhcpc6            xxd
bootchartd         delgroup           fdflush            hostname           killall5           mesg               nslookup           reboot             setsid             tac                udhcpd             xz
brctl              deluser            fdformat           httpd              klogd              microcom           ntpd               reformime          setuidgid          tail               udpsvd             xzcat
bunzip2            depmod             fdisk              hush               last               mkdir              nuke               remove-shell       sh                 tar                uevent             yes
busybox            devmem             fgconsole          hwclock            less               mkdosfs            od                 renice             sha1sum            taskset            umount             zcat
bzcat              df                 fgrep              i2cdetect          link               mke2fs             openvt             reset              sha256sum          tc                 uname              zcip
bzip2              dhcprelay          find               i2cdump            linux32            mkfifo             partprobe          resize             sha3sum            tcpsvd             unexpand
cal                diff               findfs             i2cget             linux64            mkfs.ext2          passwd             resume             sha512sum          tee                uniq
cat                dirname            flock              i2cset             linuxrc            mkfs.minix         paste              rev                showkey            telnet             unix2dos
chat               dmesg              fold               i2ctransfer        ln                 mkfs.vfat          patch              rm                 shred              telnetd            unlink
chattr             dnsd               free               id                 loadfont           mknod              pgrep              rmdir              shuf               test               unlzma
chgrp              dnsdomainname      freeramdisk        ifconfig           loadkmap           mkpasswd           pidof              rmmod              slattach           tftp               unshare
chmod              dos2unix           fsck               ifdown             logger             mkswap             ping               route              sleep              tftpd              unxz
chown              dpkg               fsck.minix         ifenslave          login              mktemp             ping6              rpm                smemcap            time               unzip
chpasswd           dpkg-deb           fsfreeze           ifplugd            logname            modinfo            pipe_progress      rpm2cpio           softlimit          timeout            uptime
> #
```

Wróćmy zatem do naszego poznajkubernetes/pkad i przyjrzyjmy się plikowi `wget` jeszcze raz.
Zauważamy że jest on tak naprawde symlinkiem do `/bin/busybox`
```
> docker run -it poznajkubernetes/pkad bin/sh
~ $ ls -l usr/bin/wget
lrwxrwxrwx    1 root     root            12 Aug 20 10:30 usr/bin/wget -> /bin/busybox
```
Jeżeli teraz sprawdzimy teraz inne kluczowe pliki zobaczymy, że one również ustawione są jako symlink do `/bin/busybox`
```
~ $ ls -l bin/sh
lrwxrwxrwx    1 root     root            12 Aug 20 10:30 bin/sh -> /bin/busybox
~ $ ls -l bin/ls
lrwxrwxrwx    1 root     root            12 Aug 20 10:30 bin/ls -> /bin/busybox
~ $ ls -l bin/cat
lrwxrwxrwx    1 root     root            12 Aug 20 10:30 bin/cat -> /bin/busybox
```

oznacza to, że gdy nadpisujemy `wget` tak naprawdę nadpisujemy `/bin/busybox` co powoduje również usunięcie wszystkich innych plików,
w tym `sh` przez co nie jesteśmy w stanie dostać się do shella