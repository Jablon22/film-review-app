# 🎬 Film Review App

Aplikacja webowa do katalogowania, oceniania i recenzowania filmów, zbudowana w **ASP.NET Core 8 MVC**.
Umożliwia przeglądanie katalogu filmów, dodawanie recenzji, prowadzenie własnej watchlisty oraz
import filmów z bazy **TMDB**. Posiada rozbudowany panel administracyjny z moderacją treści.

---

## ✨ Funkcje

### Część publiczna
- **Strona główna** – baner z losowym „filmem dnia”, sekcje „Najwyżej oceniane” i „Ostatnio dodane”, statystyki serwisu.
- **Katalog filmów** – kafelki z plakatami, filtrowanie po gatunku i roku, wyszukiwarka AJAX (bez przeładowania), paginacja po 12 filmów.
- **Szczegóły filmu** – pełne informacje, średnia ocena (gwiazdki), lista zatwierdzonych recenzji, formularz dodania recenzji.
- **Rejestracja i logowanie** (ASP.NET Core Identity).

### Dla zalogowanego użytkownika
- **Profil** – avatar z inicjałami, data rejestracji, statystyki, ostatnie recenzje.
- **Watchlista** – zakładki „Chcę obejrzeć” / „Obejrzane”, zmiana statusu i usuwanie.
- **Moje recenzje** – edycja i usuwanie własnych recenzji.

### Panel administracyjny (`/Admin`)
- **Dashboard** – liczniki i ostatnie recenzje do moderacji.
- **Zarządzanie filmami** – CRUD, **import z TMDB** (wyszukiwanie + import jednym kliknięciem), upload własnego plakatu.
- **Moderacja recenzji** – zatwierdzanie i odrzucanie recenzji.
- **Użytkownicy** – zmiana roli (User ↔ Admin), blokowanie/odblokowywanie konta.
- **Edycja treści strony** (CMS) – tytuł serwisu, opis hero, tekst stopki.

---

## 🧰 Stos technologiczny

| Warstwa | Technologia |
|---|---|
| Backend | ASP.NET Core 8 MVC (C#) |
| ORM / baza danych | Entity Framework Core 8 + SQL Server LocalDB |
| Uwierzytelnianie | ASP.NET Core Identity (role: `User`, `Admin`) |
| Frontend | Bootstrap 5 + Font Awesome 6 (CDN) |
| Interaktywność | Vanilla JavaScript (AJAX, brak frameworków JS) |
| Integracja zewn. | TMDB API v3 (import filmów) |

---

## 📁 Struktura projektu

```
FilmReviewApp/
├── Controllers/              # HomeController, MoviesController, ReviewsController, AccountController, WatchlistController
├── Areas/Admin/              # Panel administracyjny (Dashboard, Movies, Reviews, Users, Content)
├── Models/                   # Movie, Review, WatchlistItem, SiteSetting, ApplicationUser
│   ├── ViewModels/           # Osobne VM dla każdego widoku
│   └── Tmdb/                 # DTO dla odpowiedzi TMDB
├── Data/                     # ApplicationDbContext, DataSeeder
├── Services/                 # TmdbService, RatingService, SiteSettingsService
├── Views/                    # Widoki Razor (.cshtml)
└── wwwroot/
    ├── css/site.css          # Style + media queries RWD
    ├── js/site.js            # AJAX search, import TMDB, modal usuwania
    └── uploads/              # Przesłane plakaty filmów
```

---

## 🚀 Uruchomienie krok po kroku

### Wymagania wstępne
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server **LocalDB** (instalowany razem z Visual Studio lub jako osobny komponent)

### 1. Sklonuj repozytorium
```bash
git clone https://github.com/Jablon22/film-review-app.git
cd film-review-app
```

### 2. (Opcjonalnie) ustaw własny klucz TMDB
W pliku `FilmReviewApp/appsettings.json` znajduje się pole `TmdbApiKey`.
Domyślnie wpisany jest działający klucz; możesz podać własny z [TMDB](https://www.themoviedb.org/settings/api).

### 3. Przywróć narzędzia i utwórz bazę danych
```bash
dotnet tool restore
dotnet ef database update --project FilmReviewApp
```
> Baza danych zostanie również automatycznie utworzona i wypełniona danymi przy pierwszym uruchomieniu aplikacji.

### 4. Uruchom aplikację
```bash
dotnet run --project FilmReviewApp
```
Aplikacja będzie dostępna pod adresem wskazanym w konsoli (np. `https://localhost:7xxx` lub `http://localhost:5xxx`).

---

## 🔑 Dane logowania (konto administratora)

Tworzone automatycznie przy pierwszym uruchomieniu:

| Pole | Wartość |
|---|---|
| **E-mail** | `admin@filmapp.pl` |
| **Hasło** | `Admin123!` |

Panel administracyjny dostępny jest pod adresem **`/Admin`** po zalogowaniu na konto z rolą `Admin`.

---

## 📦 Dane startowe (seed)

Przy pierwszym uruchomieniu aplikacja automatycznie:
1. Tworzy role `Admin` i `User`.
2. Tworzy konto administratora (dane powyżej).
3. Dodaje 3 przykładowe filmy z opisami.
4. Dodaje 2 zatwierdzone recenzje.
5. Inicjuje ustawienia treści strony (CMS).

---

## 🔒 Bezpieczeństwo
- Atrybuty `[Authorize]` na akcjach wymagających logowania oraz `[Authorize(Roles = "Admin")]` na całym obszarze Admin.
- Token anti-forgery w każdym formularzu POST.
- Sanityzacja i walidacja plików przy uploadzie plakatów (dozwolone rozszerzenia, limit rozmiaru, nazwa generowana po stronie serwera).

---

*Projekt zaliczeniowy – Technologie Internetowe, AGH.*
