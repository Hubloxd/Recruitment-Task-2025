# Recruitment Task 2025

## Spis treści

- [Opis projektu](#opis-projektu)
- [Uruchomienie aplikacji](#uruchomienie-aplikacji)
- [Uruchamianie testów](#uruchamianie-testów)
- [Implementacja HATEOAS](#implementacja-hateoas)

---

## Opis projektu

To aplikacja przygotowana jako zadanie rekrutacyjne. Składa się z aplikacji backendowej (kontener `app`) oraz bazy danych PostgreSQL (`postgres`).

---

## Uruchomienie aplikacji

Aby uruchomić aplikację lokalnie z użyciem Docker Compose, wykonaj:

```bash
docker-compose up --build
```

Domyślnie zostaną uruchomione dwa kontenery:

- `postgres`: baza danych PostgreSQL (port: 5432)
- `app`: kontener aplikacji zbudowany z Dockerfile

---

## Uruchamianie testów

Aby wykonać testy aplikacji wykonaj:

```bash
cd tests/
dotnet restore TodoAPITest.csproj
dotnet test
```

---

## Implementacja HATEOAS

Projekt został rozszerzony o podstawową obsługę HATEOAS (Hypermedia as the Engine of Application State), czyli podejścia w którym odpowiedzi API zawierają linki do możliwych dalszych akcji.

### Przykład odpowiedzi z HATEOAS

```json
{
 "item": {
  "id": 1,
  "title": "string",
  "description": "string",
  "completionPercentage": 0,
  "createdAt": "2025-04-29T17:11:26.1605721Z",
  "expireAt": "2025-05-06T17:11:26.1518475Z"
 },
 "_links": [
  {
   "href": "/todos/1",
   "rel": "self",
   "method": "GET"
  },
  {
   "href": "/todos/1",
   "rel": "update",
   "method": "PATCH"
  },
  {
   "href": "/todos/1",
   "rel": "delete",
   "method": "DELETE"
  },
  {
   "href": "/todos/1/complete",
   "rel": "complete",
   "method": "POST"
  },
  {
   "href": "/todos/1/completion",
   "rel": "set-completion",
   "method": "PATCH"
  }
 ]
}
```

---

## Autor

Hubert Jakóbczuk
