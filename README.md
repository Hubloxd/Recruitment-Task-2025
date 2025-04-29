# Recruitment Task 2025

## Spis tre�ci

- [Opis projektu](#opis-projektu)
- [Uruchomienie aplikacji](#uruchomienie-aplikacji)
- [Uruchamianie test�w](#uruchamianie-test�w)
- [Implementacja HATEOAS](#implementacja-hateoas)

---

## Opis projektu

To aplikacja przygotowana jako zadanie rekrutacyjne. Sk�ada si� z aplikacji backendowej (kontener `app`) oraz bazy danych PostgreSQL (`postgres`).

---

## Uruchomienie aplikacji

Aby uruchomi� aplikacj� lokalnie z u�yciem Docker Compose, wykonaj:

```bash
docker-compose up --build
```

Domy�lnie zostan� uruchomione dwa kontenery:

- `postgres`: baza danych PostgreSQL (port: 5432)
- `app`: kontener aplikacji zbudowany z Dockerfile

---

## Uruchamianie test�w

Aby wykona� testy aplikacji wykonaj:

```bash
cd tests/
dotnet restore TodoAPITest.csproj
dotnet test
```

---

## Implementacja HATEOAS

Projekt zosta� rozszerzony o podstawow� obs�ug� HATEOAS (Hypermedia as the Engine of Application State), czyli podej�cia w kt�rym odpowiedzi API zawieraj� linki do mo�liwych dalszych akcji.

### Przyk�ad odpowiedzi z HATEOAS

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

Hubert Jak�bczuk
