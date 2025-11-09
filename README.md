# Entity Framework - Tips & Tricks
## Corso Completo con Dispensa ed Esercizi Pratici

Repository completo per un corso di 4 ore su Entity Framework Core, con teoria, esempi pratici e laboratorio hands-on.

## 📚 Contenuti del Corso

### 1. Teoria - Dispensa Completa
📖 **File**: `dispensa.md`

La dispensa copre:
- Introduzione a Entity Framework e ORM
- DB First vs Code First
- DbContext e configurazione
- Migrations e gestione dello schema
- Query e operazioni CRUD
- Ottimizzazione e performance
- Best practices e anti-pattern

### 2. Presentazione
📊 **File**: `EntityFramework tip&tricks.pptx`

Slide del corso per la parte frontale.

### 3. Laboratorio Pratico
💻 **Cartelle**: `Exercises/` e `Solutions/`

4 esercizi progressivi con soluzioni complete.

## 🎯 Struttura Laboratorio

```
├── Exercises/              # Esercizi da completare
│   ├── Exercise01_Setup/
│   ├── Exercise02_CodeFirst/
│   ├── Exercise03_CRUD/
│   └── Exercise04_QueryOptimization/
│
└── Solutions/             # Soluzioni complete
    ├── Exercise01_Setup/
    ├── Exercise02_CodeFirst/
    ├── Exercise03_CRUD/
    └── Exercise04_QueryOptimization/
```

## 🚀 Come Iniziare

### Prerequisiti
- .NET 8.0 SDK ([Download](https://dotnet.microsoft.com/download))
- Editor di codice (VS Code, Visual Studio, o Rider)
- **Docker** (per PostgreSQL) - oppure usa SQLite come alternativa
- Conoscenze base di C# e database relazionali

### Setup Rapido

#### 1. Avvia PostgreSQL con Docker
```bash
# Dalla root del progetto
docker-compose up -d

# Verifica che sia in esecuzione
docker ps
```

#### 2. Inizia con gli esercizi
```bash
# Naviga al primo esercizio
cd Exercises/Exercise01_Setup

# Leggi la consegna
cat README.md

# Esegui quando sei pronto
dotnet run
```

#### Alternativa: Usare SQLite senza Docker

Se non hai Docker, apri i file `Program.cs` nelle soluzioni e:
- Commenta la riga con `UseNpgsql()`
- Decommenta la riga con `UseSqlite()`

📖 **Per maggiori dettagli su Docker e PostgreSQL**, leggi [DOCKER.md](./DOCKER.md)

## 📋 Percorso Consigliato

### Parte 1: Teoria (1.5h)
1. Leggi la `dispensa.md` - Sezioni 1-2
2. Segui le slide per i concetti chiave
3. Comprendi i vantaggi e svantaggi degli ORM

### Parte 2: Configurazione e Migrations (1h)
1. Leggi `dispensa.md` - Sezione 3 (Migrations)
2. **Esercizio 01**: Setup iniziale e connessione DB
3. **Esercizio 02**: Code First e prima migration

### Parte 3: Query e CRUD (1h)
1. Leggi `dispensa.md` - Sezione 4 (Query e CRUD)
2. **Esercizio 03**: Operazioni CRUD con best practices

### Parte 4: Ottimizzazione (0.5h)
1. **Esercizio 04**: Problema N+1 e ottimizzazioni
2. Ripasso dei concetti chiave

## 📝 Esercizi in Dettaglio

### Exercise 01 - Setup Iniziale
**Tempo stimato**: 15 minuti
**Obiettivo**: Configurare DbContext e testare la connessione

**Imparerai**:
- Configurazione di Entity Framework
- Connection string SQLite
- Verifica connessione al database

### Exercise 02 - Code First
**Tempo stimato**: 30 minuti
**Obiettivo**: Definire entità e generare migration

**Imparerai**:
- Definizione di entità POCO
- Configurazione con Fluent API
- Generazione e applicazione migration
- Relazioni 1:m tra entità

### Exercise 03 - Operazioni CRUD
**Tempo stimato**: 45 minuti
**Obiettivo**: Implementare Create, Read, Update, Delete

**Imparerai**:
- Best practices per inserimenti (AddRange)
- Query ottimizzate (AsNoTracking)
- Eager Loading con Include()
- Proiezioni con Select()
- Update con tracking vs esplicito

### Exercise 04 - Query Optimization
**Tempo stimato**: 30 minuti
**Obiettivo**: Identificare e risolvere il problema N+1

**Imparerai**:
- Cos'è il problema N+1
- Eager Loading vs Lazy Loading
- Include() per evitare query multiple
- Select() per ottimizzazione avanzata
- Paginazione efficiente

## ✅ Best Practices Insegnate

- ✅ `SaveChanges()` fuori dai loop
- ✅ `AsNoTracking()` per query read-only
- ✅ `Include()` per evitare N+1 Problem
- ✅ `Select()` per caricare solo dati necessari
- ✅ `AddRange()` per inserimenti batch
- ✅ Fluent API per configurazione pulita
- ✅ Migration per evoluzione controllata dello schema

## 🎓 Dopo il Corso

### Progetti Suggeriti
1. **Blog Engine**: Crea un sistema blog con Posts, Comments, Authors
2. **E-commerce**: Implementa Products, Orders, OrderItems, Customers
3. **Task Manager**: Sviluppa Tasks, Projects, Users con relazioni complesse

### Approfondimenti
- [Documentazione Ufficiale EF Core](https://docs.microsoft.com/en-us/ef/core/)
- [EF Core Performance](https://docs.microsoft.com/en-us/ef/core/performance/)
- [GitHub EF Core](https://github.com/dotnet/efcore)

## 🛠️ Comandi Utili

```bash
# Build progetto
dotnet build

# Esegui progetto
dotnet run

# Aggiungi migration
dotnet ef migrations add NomeMigration

# Applica migration
dotnet ef database update

# Rimuovi ultima migration
dotnet ef migrations remove

# Lista migration applicate
dotnet ef migrations list

# Genera script SQL da migration
dotnet ef migrations script
```

## 📂 File e Cartelle

```
entity_framework_tips_and_tricks/
├── README.md                          # Questo file
├── dispensa.md                        # Dispensa teorica completa
├── convenzioni_ef.md                  # Convenzioni di naming EF
├── EntityFramework tip&tricks.pptx    # Slide del corso
├── .gitignore                         # File da ignorare in Git
│
├── Exercises/                         # Esercizi da completare
│   ├── README.md                      # Guida agli esercizi
│   ├── Exercise01_Setup/
│   ├── Exercise02_CodeFirst/
│   ├── Exercise03_CRUD/
│   └── Exercise04_QueryOptimization/
│
└── Solutions/                         # Soluzioni complete
    ├── README.md                      # Guida alle soluzioni
    ├── Exercise01_Setup/
    ├── Exercise02_CodeFirst/
    ├── Exercise03_CRUD/
    └── Exercise04_QueryOptimization/
```

## 🤝 Contributi

Questo è materiale didattico. Se trovi errori o vuoi suggerire miglioramenti:
1. Apri una issue
2. Proponi una pull request
3. Contatta il docente

## 📄 Licenza

Materiale didattico per uso educativo.

## 👨‍🏫 Info Corso

- **Durata**: 4 ore
- **Livello**: Intermedio
- **Prerequisiti**: C# base, SQL base
- **Tecnologie**: .NET 8, EF Core 9, SQLite

---

**Buono studio e buon coding! 🚀**
