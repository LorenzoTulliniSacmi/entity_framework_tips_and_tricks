# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository Overview

This is an educational repository for a 4-hour Entity Framework Core course (Italian language). It contains:
- **Theoretical materials**: `dispensa.md` (comprehensive guide), `convenzioni_ef.md` (EF conventions reference)
- **Exercises**: 4 progressive hands-on labs in `Exercises/`
- **Solutions**: Complete working solutions in `Solutions/`
- **Presentation**: PowerPoint slides for the course

## Technology Stack

- **.NET 8.0** - Target framework
- **Entity Framework Core 9.0** - ORM framework
- **Database options**:
  - PostgreSQL 16 (primary, via Docker)
  - SQLite (alternative for users without Docker)
- **Packages**:
  - `Microsoft.EntityFrameworkCore.Design` (9.0.10)
  - `Microsoft.EntityFrameworkCore.Sqlite` (9.0.10)
  - `Npgsql.EntityFrameworkCore.PostgreSQL` (9.0.0)

## Project Structure

```
entity_framework_tips_and_tricks/
├── Exercises/           # Lab exercises (templates to complete)
│   ├── Exercise01_Setup/             # DbContext setup and connection
│   ├── Exercise02_CodeFirst/         # Entities and migrations
│   ├── Exercise03_CRUD/              # CRUD operations with best practices
│   └── Exercise04_QueryOptimization/ # N+1 problem and optimization
│
├── Solutions/          # Complete working solutions
│   └── (same structure as Exercises/)
│
├── dispensa.md         # Comprehensive theoretical guide
├── convenzioni_ef.md   # EF Core conventions reference
└── docker-compose.yml  # PostgreSQL setup
```

### Project Organization Pattern

Each exercise/solution follows this structure:
- `Program.cs` - Entry point with demonstrations
- `Data/AppDbContext.cs` - DbContext configuration
- `Models/` - Entity classes (POCO)
- Migrations folder (when applicable)

## Common Development Commands

### Building and Running

```bash
# Build a project
dotnet build

# Run a project
dotnet run

# Build entire solution (from Exercises/ or Solutions/)
dotnet build Exercises.sln
dotnet build Solutions.sln
```

### Database Setup

**PostgreSQL (recommended):**
```bash
# Start PostgreSQL container
docker-compose up -d

# Check container status
docker ps

# Stop PostgreSQL
docker-compose down
```

**PostgreSQL Connection Details:**
- Host: localhost
- Port: 5432
- Username: efuser
- Password: efpass
- Database: ef_lab (default)

**SQLite Alternative:**
To use SQLite instead of PostgreSQL, edit `Program.cs`:
- Comment out the `UseNpgsql()` line
- Uncomment the `UseSqlite()` line

### Entity Framework Migrations

```bash
# Add a new migration
dotnet ef migrations add NomeMigration

# Apply migrations to database
dotnet ef database update

# Remove last migration (before applying)
dotnet ef migrations remove

# List all migrations
dotnet ef migrations list

# Generate SQL script from migrations
dotnet ef migrations script
```

## Code Architecture Patterns

### DbContext Configuration

Each project uses `DbContextOptionsBuilder` with either PostgreSQL or SQLite:

```csharp
var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

// PostgreSQL (requires Docker)
optionsBuilder.UseNpgsql("Host=localhost;Database=ef_lab_codefirst;Username=efuser;Password=efpass");

// SQLite (alternative)
// optionsBuilder.UseSqlite("Data Source=catalog.db");
```

### Entity Structure

- Entities in `Models/` folder
- Navigation properties for relationships
- Follows EF Core conventions: `Id` or `{ClassName}Id` for primary keys
- Foreign keys: `{NavigationProperty}Id` pattern

### DbContext Pattern

- Context classes in `Data/` folder
- `DbSet<T>` properties for entities
- Fluent API configuration in `OnModelCreating()`
- Explicit relationship configuration preferred over conventions

## Best Practices Taught in Course

The course emphasizes these EF Core best practices:

1. **SaveChanges() outside loops** - Call once after bulk operations
2. **AsNoTracking() for read-only queries** - Better performance
3. **Include() to avoid N+1 Problem** - Eager loading of related entities
4. **Select() for projections** - Load only necessary data
5. **AddRange() for batch inserts** - More efficient than multiple Add() calls
6. **Fluent API over Data Annotations** - More powerful configuration
7. **Migrations for schema evolution** - Version-controlled database changes

### Anti-patterns to Avoid

- ❌ Calling `SaveChanges()` inside loops
- ❌ Not using `AsNoTracking()` for read-only queries
- ❌ Lazy loading causing N+1 queries (prefer explicit Include())
- ❌ Loading entire entities when only few properties needed

## Language and Naming Conventions

- **Code**: English (variables, classes, methods)
- **Comments and documentation**: Italian (educational material in Italian)
- **Database naming**: Follow EF Core conventions (PascalCase for tables/columns)
- **Git commits**: Italian

## Testing and Running Exercises

To test an exercise:
1. Ensure PostgreSQL is running (`docker-compose up -d`) or switch to SQLite
2. Navigate to exercise folder
3. Run `dotnet run`
4. Compare output with expected results in README.md

Each exercise README contains:
- Detailed objectives
- Step-by-step instructions
- Expected output examples
- Hints for implementation

## Key Course Concepts by Exercise

### Exercise 01 - Setup
- DbContext configuration
- Connection strings
- Database connection verification
- EnsureCreated() vs Migrations

### Exercise 02 - Code First
- Entity definition (POCO classes)
- Navigation properties (1:N relationships)
- Fluent API configuration
- Migration generation and application
- Primary/Foreign key conventions

### Exercise 03 - CRUD
- Create operations (Add, AddRange)
- Read queries (Find, Where, Include, AsNoTracking)
- Update operations (tracked vs explicit)
- Delete operations
- Projection with Select()

### Exercise 04 - Query Optimization
- N+1 Problem identification
- Eager Loading with Include()
- Explicit Loading
- Projection optimization with Select()
- Pagination (Skip/Take)
- Query logging for debugging

## Working with Solutions vs Exercises

- **Exercises/**: Template projects with TODO comments and minimal code
- **Solutions/**: Complete implementations with detailed comments
- Each solution demonstrates the best practice approach
- Solutions include console output for validation

When helping with exercises:
1. Check if user is working on Exercise or Solution code
2. For Exercises: Guide toward solution without giving complete code
3. For Solutions: Explain implementation details and patterns used
4. Always reference the relevant best practices from the course

## Database Conventions Reference

The file `convenzioni_ef.md` contains comprehensive EF Core convention documentation including:
- Primary key detection rules
- Foreign key patterns
- Relationship configurations
- Type mappings (C# to SQL)
- Nullability handling
- Index creation
- Delete behaviors
- Inheritance strategies (TPH/TPT/TPC)
- Shadow properties

Reference this file when questions arise about EF Core conventions.

## Notes

- This is educational material for intermediate-level C# developers
- Course duration: 4 hours (1.5h theory, 2.5h hands-on)
- All code uses .NET 8 and EF Core 9
- Nullable reference types enabled (`<Nullable>enable</Nullable>`)
