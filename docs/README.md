# ⚡ ElektroOffer

Desktopová WPF aplikace pro kalkulaci elektro prací a materiálu.
Cílem projektu je vytvořit přehledný nástroj pro tvorbu nabídek (rozpočtů) s možností ukládání, načítání, exportu a budoucím PDF / tisku.

---

## 📦 Požadavky

- Windows 10 / 11
- Visual Studio 2022+
- .NET 10 SDK
- workload: **.NET Desktop Development**

---

## 🚀 Spuštění aplikace

1. Otevři `ElektroOffer_app.slnx`
2. Sestav projekt (`Ctrl + Shift + B`)
3. Spusť aplikaci (`F5`)

📌 Databáze `elektrooffer.db` se vytvoří automaticky při prvním spuštění.

---

## ✨ Hlavní funkce

- 📊 Kalkulace práce podle ceníku (PriceItems)
- 📦 Kalkulace materiálu (Materials)
- 💾 Ukládání / načítání projektu (`*.eof`)
- 📤 Export / import ceníku (`*.eofcat`)
- 📑 Detailní rozpis položek (Budget view)
- 🧾 Tisk (PrintDialog – Windows systémový tisk / PDF přes tiskárnu)
- 💰 Automatický součet práce + materiálu

---

## 🧠 Architektura aplikace

Projekt je navržen jako **výuková WPF aplikace s postupným přechodem k oddělené architektuře**.

- UI: WPF (XAML)
- Logika: Code-behind + Services
- Data: EF Core + SQLite
- Export/Import: JSON serializace
- Testování: Unit + Integration

---

## 🧩 Modely (Data vrstva)

| Model | Popis |
|------|------|
| `PriceItems` | Ceník práce |
| `Material` | Ceník materiálu |
| `WorkItemData` | Řádek kalkulace práce |
| `MaterialItemData` | Řádek kalkulace materiálu |
| `ProjectData` | Celý uložený projekt (`.eof`) |
| `CatalogExportData` | Export/import ceníku |
| `BudgetItem` | Sloučený řádek rozpočtu (UI výstup) |

---

## ⚙️ Services (aplikační logika)

| Service | Účel |
|--------|------|
| `ProjectService` | Ukládání / načítání projektů + import/export |
| `CatalogService` | Načítání ceníku z databáze |
| `DialogService` | Abstrakce MessageBox UI |
| `PrintService` | Tisk / export nabídky (FlowDocument / PrintDialog) |

---

## 🖥️ UI vrstva

- `MainWindow.xaml` – hlavní kalkulační rozhraní
- `AboutWindow.xaml` – informace o aplikaci
- Toolbar + Menu + StatusBar
- Dynamické ItemsControl pro práce a materiál
- Rozpis (BudgetItems)
- Celková cena (GrandTotal)

---

## 📁 Struktura projektu

📁 Core Application
├── Views
│   ├── MainWindow (hlavní kalkulace)
│   └── AboutWindow (info o aplikaci)
│
├── ViewModels
│   ├── BaseViewModel
│   ├── CalculationItemViewModel (logika řádku kalkulace)
│   └── AboutWindowViewModel
│
├── Models
│   ├── ProjectData (uložený projekt)
│   ├── WorkItemData (práce)
│   ├── MaterialItemData (materiál)
│   ├── BudgetItem (výstupní rozpis)
│   └── CatalogExportData (import/export)
│
├── Services
│   ├── ProjectService (save/load/export/import)
│   ├── CatalogService (DB ceník)
│   ├── DialogService (UI dialogy)
│   └── PrintService (tisk / export)
│
├── Data
│   └── AppDbContext (EF Core + SQLite)
│
├── Commands
│   └── RelayCommand (MVVM commandy)
│
└── Resources
    ├── Styles.xaml
    ├── Colors.xaml
    └── Icons

🧪 Testing
├── Unit Tests
│   ├── LogicTests
│   ├── RepositoryTests
│   └── DatabaseTests
│
└── Integration Tests
    ├── Database Tests
    ├── Service Tests
    └── CRUD Scenarios

---

## 🧪 Testování

Projekt obsahuje dvě úrovně testů:

---

### 🔬 Unit testy

- izolovaná logika
- bez DB a UI
- rychlé testování výpočtů

📌 zaměření:
- kalkulace cen
- repository logika
- ViewModel logika

---

### 🧪 Integrační testy

Testují spolupráci:

- EF Core + SQLite
- Services + DB
- kompletní scénáře aplikace

📌 zahrnují:
- DB schema testy
- CRUD operace
- ProjectService testy
- CatalogService testy

---

## 🧠 Testovací architektura

- SQLite InMemory / test DB
- EF Core izolované DbContexty
- oddělení Unit vs Integration
- testování service vrstvy bez UI

---

## 📌 Verze

Aktuální verze: **1.7.0 (Print / Export)**

- přidán tiskový systém (Windows PrintDialog)
- připraven export přes PrintService
- zpřesněný datový model (ProjectData jako DTO)
- stabilizované integrační testy

---

## 🏗️ Architektonická rozhodnutí (ADR)

### SQLite
Lokální desktop aplikace → jednoduché nasazení bez serveru.

### Code-behind + Services
Výukový projekt → jednoduchost + postupný refactoring.

### JSON (.eof)
Snadná čitelnost, debug a kompatibilita.

### PrintService
Oddělení exportu/tisku od UI logiky.

---

## 🚧 Roadmap

- [x] Kalkulace práce a materiálu
- [x] SQLite databáze
- [x] Ukládání projektu
- [x] Integrační testy
- [x] Tisk / PrintDialog
- [ ] PDF export (QuestPDF nebo podobné)
- [ ] MVVM refactor (částečný → plný)