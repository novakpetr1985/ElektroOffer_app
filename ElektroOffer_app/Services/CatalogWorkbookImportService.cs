using ElektroOffer_app.Data;
using ElektroOffer_app.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Xml;

namespace ElektroOffer_app.Services
{
    public sealed class CatalogWorkbookImportService
    {
        private readonly AppDbContext _db;
        private readonly XlsxCatalogReader _reader;

        public CatalogWorkbookImportService(AppDbContext db, XlsxCatalogReader? reader = null)
        {
            _db = db;
            _reader = reader ?? new XlsxCatalogReader();
        }

        public CatalogImportResult Import(string path)
        {
            var result = new CatalogImportResult();
            CatalogWorkbookData data;
            try
            {
                data = _reader.Read(path, result.Issues);
            }
            catch (Exception ex) when (ex is IOException or InvalidDataException or XmlException or UnauthorizedAccessException)
            {
                result.Issues.Add(new CatalogImportIssue("Workbook", 0, "", ex.Message));
                return result;
            }

            Validate(data, result.Issues);
            if (!result.Success) return result;

            using var transaction = _db.Database.BeginTransaction();
            try
            {
                UpsertPrimaryData(data, result);
                _db.SaveChanges();
                UpsertRelationships(data, result);
                _db.SaveChanges();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _db.ChangeTracker.Clear();
                result.Issues.Add(new CatalogImportIssue("Database", 0, "", $"Import byl vrácen zpět: {ex.Message}"));
            }

            return result;
        }

        private static void Validate(CatalogWorkbookData data, List<CatalogImportIssue> issues)
        {
            DuplicateNames(data.Tasks.Select(x => (x.Row, x.Name)), "Tasks", issues);
            DuplicateNames(data.Specifications.Select(x => (x.Row, x.Name)), "Specifications", issues);
            DuplicateNames(data.BaseMaterials.Select(x => (x.Row, x.Name)), "BaseMaterials", issues);
            DuplicateNames(data.Positions.Select(x => (x.Row, x.Name)), "Positions", issues);
            DuplicateNames(data.Categories.Select(x => (x.Row, x.Name)), "Categories", issues);
            DuplicateNames(data.Suppliers.Select(x => (x.Row, x.Name)), "Suppliers", issues);
            DuplicateNames(data.Materials.Select(x => (x.Row, x.Name)), "Materials", issues);

            var tasks = data.Tasks.Select(x => x.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var specifications = data.Specifications.Select(x => x.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var categories = data.Categories.Select(x => x.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var suppliers = data.Suppliers.Select(x => x.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var materials = data.Materials.Select(x => x.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var row in data.TaskSpecifications)
            {
                if (!tasks.Contains(row.TaskName)) issues.Add(new CatalogImportIssue("TaskSpecifications", row.Row, "TaskName", "Úkon není uveden v listu Tasks."));
                if (!specifications.Contains(row.SpecificationName)) issues.Add(new CatalogImportIssue("TaskSpecifications", row.Row, "SpecificationName", "Specifikace není uvedena v listu Specifications."));
            }

            foreach (var row in data.Materials)
                if (!categories.Contains(row.CategoryName)) issues.Add(new CatalogImportIssue("Materials", row.Row, "CategoryName", "Kategorie není uvedena v listu Categories."));

            var priceKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var row in data.MaterialPrices)
            {
                if (!materials.Contains(row.MaterialName)) issues.Add(new CatalogImportIssue("MaterialPrices", row.Row, "MaterialName", "Materiál není uveden v listu Materials."));
                if (!suppliers.Contains(row.SupplierName)) issues.Add(new CatalogImportIssue("MaterialPrices", row.Row, "SupplierName", "Dodavatel není uveden v listu Suppliers."));
                if (!priceKeys.Add($"{row.SupplierName}\u001f{row.SupplierCode}"))
                    issues.Add(new CatalogImportIssue("MaterialPrices", row.Row, "SupplierCode", "Kód dodavatele je v sešitu pro stejného dodavatele duplicitní."));
            }
        }

        private static void DuplicateNames(IEnumerable<(int Row, string Name)> rows, string sheet, List<CatalogImportIssue> issues)
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var row in rows)
                if (!names.Add(row.Name)) issues.Add(new CatalogImportIssue(sheet, row.Row, "Name", "Název je v listu duplicitní."));
        }

        private void UpsertPrimaryData(CatalogWorkbookData data, CatalogImportResult result)
        {
            UpsertNamed(data.Categories, _db.Categories, x => x.Name, name => new Category { Name = name }, result);
            UpsertNamed(data.Suppliers, _db.Suppliers, x => x.Name, name => new Supplier { Name = name }, result);

            var tasks = _db.Tasks.ToList().ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
            foreach (var row in data.Tasks)
            {
                if (tasks.TryGetValue(row.Name, out var entity)) { entity.BasePrice = row.BasePrice; result.Updated++; }
                else { entity = new WorkTask { Name = row.Name, BasePrice = row.BasePrice }; _db.Tasks.Add(entity); tasks.Add(row.Name, entity); result.Inserted++; }
            }

            var specifications = _db.Specifications.ToList().ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
            foreach (var row in data.Specifications)
            {
                if (specifications.TryGetValue(row.Name, out var entity)) { entity.Unit = row.Unit; result.Updated++; }
                else { entity = new WorkSpecification { Name = row.Name, Unit = row.Unit }; _db.Specifications.Add(entity); specifications.Add(row.Name, entity); result.Inserted++; }
            }

            var bases = _db.BaseMaterials.ToList().ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
            foreach (var row in data.BaseMaterials)
            {
                if (bases.TryGetValue(row.Name, out var entity)) { entity.BaseMaterialCoef = row.Coefficient; result.Updated++; }
                else { entity = new BaseMaterial { Name = row.Name, BaseMaterialCoef = row.Coefficient }; _db.BaseMaterials.Add(entity); bases.Add(row.Name, entity); result.Inserted++; }
            }

            var positions = _db.Positions.ToList().ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
            foreach (var row in data.Positions)
            {
                if (positions.TryGetValue(row.Name, out var entity)) { entity.PositionCoef = row.Coefficient; result.Updated++; }
                else { entity = new WorkPosition { Name = row.Name, PositionCoef = row.Coefficient }; _db.Positions.Add(entity); positions.Add(row.Name, entity); result.Inserted++; }
            }

            _db.SaveChanges();
            var categories = _db.Categories.ToList().ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
            var materials = _db.Materials.ToList().ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
            foreach (var row in data.Materials)
            {
                var category = categories[row.CategoryName];
                if (materials.TryGetValue(row.Name, out var entity))
                {
                    entity.CategoryId = category.Id; entity.Unit = row.Unit; entity.Price = row.DefaultPrice; result.Updated++;
                }
                else
                {
                    entity = new Material { Name = row.Name, CategoryId = category.Id, Unit = row.Unit, Price = row.DefaultPrice };
                    _db.Materials.Add(entity); materials.Add(row.Name, entity); result.Inserted++;
                }
            }
        }

        private static void UpsertNamed<TEntity>(
            IEnumerable<NamedImportRow> rows,
            DbSet<TEntity> set,
            Func<TEntity, string> getName,
            Func<string, TEntity> create,
            CatalogImportResult result)
            where TEntity : class
        {
            var entities = set.ToList().ToDictionary(getName, StringComparer.OrdinalIgnoreCase);
            foreach (var row in rows)
            {
                if (entities.ContainsKey(row.Name)) { result.Updated++; continue; }
                var entity = create(row.Name); set.Add(entity); entities.Add(row.Name, entity); result.Inserted++;
            }
        }

        private void UpsertRelationships(CatalogWorkbookData data, CatalogImportResult result)
        {
            var tasks = _db.Tasks.ToList().ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
            var specifications = _db.Specifications.ToList().ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
            var existingLinks = _db.TaskSpecifications.ToList().ToDictionary(x => $"{x.TaskId}:{x.SpecificationId}");
            foreach (var row in data.TaskSpecifications)
            {
                var task = tasks[row.TaskName]; var specification = specifications[row.SpecificationName];
                var key = $"{task.Id}:{specification.Id}";
                if (existingLinks.ContainsKey(key)) { result.Updated++; continue; }
                var link = new TaskSpecification { TaskId = task.Id, SpecificationId = specification.Id };
                _db.TaskSpecifications.Add(link); existingLinks.Add(key, link); result.Inserted++;
            }

            var materials = _db.Materials.ToList().ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
            var suppliers = _db.Suppliers.ToList().ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
            var prices = _db.MaterialPrices.Include(x => x.Supplier).ToList()
                .ToDictionary(x => $"{x.Supplier.Name}\u001f{x.SupplierCode}", StringComparer.OrdinalIgnoreCase);
            foreach (var row in data.MaterialPrices)
            {
                var material = materials[row.MaterialName]; var supplier = suppliers[row.SupplierName];
                var key = $"{row.SupplierName}\u001f{row.SupplierCode}";
                if (prices.TryGetValue(key, out var entity))
                {
                    entity.MaterialId = material.Id; entity.SupplierId = supplier.Id; entity.SupplierName = row.SupplierItemName;
                    entity.Unit = row.Unit; entity.Price = row.Price; entity.Currency = row.Currency; entity.UpdatedAt = row.UpdatedAt ?? DateTime.Now; result.Updated++;
                }
                else
                {
                    entity = new MaterialPrice { MaterialId = material.Id, SupplierId = supplier.Id, SupplierCode = row.SupplierCode,
                        SupplierName = row.SupplierItemName, Unit = row.Unit, Price = row.Price, Currency = row.Currency, UpdatedAt = row.UpdatedAt ?? DateTime.Now };
                    _db.MaterialPrices.Add(entity); prices.Add(key, entity); result.Inserted++;
                }
            }
        }
    }
}
