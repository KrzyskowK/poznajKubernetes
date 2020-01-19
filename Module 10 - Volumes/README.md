# Volume

## do czego używamy?

### komunikacja i synchronizacja
- komunikacja między dwoma kontenerami (jeden produkuje logi, drugi konsumuje i je wysyła)
- przykład `emptyDir` - żyje tak długo jak POD

### cache
- typowy przykład: generowanie thumbnails
- przykład `emptyDir` - żyje tak długo jak POD

### host filesystem
- `hostPath` - żyje tak długo jak NODE
- gdy potrzebujemy diagnostyki noda, 
- gdy potrzebujemy rozwiązania do replikacji danych
- podłaczenie wydajnych dyskow (NoSQL)

### persistant data
- wszystko co musi przetrwać śmierć POD i NODE
- np. bazy danych
- 3rd party vendo

## Rodzaje Volumenów

specificzne dla kubernetes (nie zależne od vendorów)
- `emptyDir` - trzymane na POD
- `hostPath` - trzymane na NODE
- `configMap`
- `secret`
