using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Xml.Linq;
using ElektroOffer_app.Models;

namespace ElektroOffer_app.Services
{
    /// <summary>
    /// Čte nízkoúrovňovou strukturu XLSX a převádí listy na řádky použitelné importérem katalogu.
    /// </summary>
    public sealed class XlsxCatalogReader
    {
        private static readonly XNamespace Main = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        private static readonly XNamespace Relationships = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        private static readonly XNamespace PackageRelationships = "http://schemas.openxmlformats.org/package/2006/relationships";

        public CatalogWorkbookData Read(string path, List<CatalogImportIssue> issues)
        {
            using var archive = ZipFile.OpenRead(path);
            var sharedStrings = ReadSharedStrings(archive);
            var sheetPaths = ReadSheetPaths(archive);
            var data = new CatalogWorkbookData();

            ReadTasks(ReadSheet(archive, sheetPaths, "Tasks", sharedStrings, issues), data, issues);
            ReadSpecifications(ReadSheet(archive, sheetPaths, "Specifications", sharedStrings, issues), data, issues);
            ReadTaskSpecifications(ReadSheet(archive, sheetPaths, "TaskSpecifications", sharedStrings, issues), data, issues);
            ReadBaseMaterials(ReadSheet(archive, sheetPaths, "BaseMaterials", sharedStrings, issues), data, issues);
            ReadPositions(ReadSheet(archive, sheetPaths, "Positions", sharedStrings, issues), data, issues);
            ReadNamed(ReadSheet(archive, sheetPaths, "Categories", sharedStrings, issues), "Categories", data.Categories, issues);
            ReadNamed(ReadSheet(archive, sheetPaths, "Suppliers", sharedStrings, issues), "Suppliers", data.Suppliers, issues);
            ReadMaterials(ReadSheet(archive, sheetPaths, "Materials", sharedStrings, issues), data, issues);
            ReadMaterialPrices(ReadSheet(archive, sheetPaths, "MaterialPrices", sharedStrings, issues), data, issues);

            return data;
        }

        private static Dictionary<string, string> ReadSheetPaths(ZipArchive archive)
        {
            var workbook = LoadXml(archive, "xl/workbook.xml");
            var rels = LoadXml(archive, "xl/_rels/workbook.xml.rels")
                .Root!.Elements(PackageRelationships + "Relationship")
                .ToDictionary(x => (string)x.Attribute("Id")!, x => (string)x.Attribute("Target")!);

            return workbook.Root!.Descendants(Main + "sheet").ToDictionary(
                x => (string)x.Attribute("name")!,
                x => NormalizeWorksheetPath(rels[(string)x.Attribute(Relationships + "id")!]),
                StringComparer.OrdinalIgnoreCase);
        }

        private static string NormalizeWorksheetPath(string target)
        {
            var clean = target.Replace('\\', '/').TrimStart('/');
            return clean.StartsWith("xl/", StringComparison.OrdinalIgnoreCase) ? clean : $"xl/{clean}";
        }

        private static List<string> ReadSharedStrings(ZipArchive archive)
        {
            var entry = archive.GetEntry("xl/sharedStrings.xml");
            if (entry == null) return new List<string>();
            using var stream = entry.Open();
            var doc = XDocument.Load(stream);
            return doc.Root!.Elements(Main + "si")
                .Select(si => string.Concat(si.Descendants(Main + "t").Select(t => t.Value)))
                .ToList();
        }

        private static List<SheetRow> ReadSheet(
            ZipArchive archive,
            Dictionary<string, string> sheetPaths,
            string sheetName,
            IReadOnlyList<string> sharedStrings,
            List<CatalogImportIssue> issues)
        {
            if (!sheetPaths.TryGetValue(sheetName, out var path))
            {
                issues.Add(new CatalogImportIssue(sheetName, 0, "", "Povinný list v sešitu chybí."));
                return new List<SheetRow>();
            }

            var doc = LoadXml(archive, path);
            return doc.Descendants(Main + "row").Select(row =>
            {
                var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var cell in row.Elements(Main + "c"))
                {
                    var reference = (string?)cell.Attribute("r") ?? string.Empty;
                    var column = new string(reference.TakeWhile(char.IsLetter).ToArray());
                    values[column] = ReadCellValue(cell, sharedStrings);
                }
                return new SheetRow((int?)row.Attribute("r") ?? 0, values);
            }).ToList();
        }

        private static string ReadCellValue(XElement cell, IReadOnlyList<string> sharedStrings)
        {
            var type = (string?)cell.Attribute("t");
            if (type == "inlineStr")
                return string.Concat(cell.Descendants(Main + "t").Select(x => x.Value)).Trim();

            var raw = cell.Element(Main + "v")?.Value ?? string.Empty;
            if (type == "s" && int.TryParse(raw, out var index) && index >= 0 && index < sharedStrings.Count)
                return sharedStrings[index].Trim();
            return raw.Trim();
        }

        private static void ReadTasks(List<SheetRow> rows, CatalogWorkbookData data, List<CatalogImportIssue> issues)
        {
            foreach (var row in DataRows(rows, "Tasks", new[] { "Name", "BasePrice" }, issues))
                if (Required(row, "Tasks", "A", "Name", issues, out var name) && Decimal(row, "Tasks", "B", "BasePrice", issues, out var price))
                    data.Tasks.Add(new WorkTaskImportRow(row.Number, name, price));
        }

        private static void ReadSpecifications(List<SheetRow> rows, CatalogWorkbookData data, List<CatalogImportIssue> issues)
        {
            foreach (var row in DataRows(rows, "Specifications", new[] { "Name", "Unit" }, issues))
                if (Required(row, "Specifications", "A", "Name", issues, out var name) && Required(row, "Specifications", "B", "Unit", issues, out var unit))
                    data.Specifications.Add(new WorkSpecificationImportRow(row.Number, name, unit));
        }

        private static void ReadTaskSpecifications(List<SheetRow> rows, CatalogWorkbookData data, List<CatalogImportIssue> issues)
        {
            foreach (var row in DataRows(rows, "TaskSpecifications", new[] { "TaskName", "SpecificationName" }, issues))
                if (Required(row, "TaskSpecifications", "A", "TaskName", issues, out var task) && Required(row, "TaskSpecifications", "B", "SpecificationName", issues, out var specification))
                    data.TaskSpecifications.Add(new TaskSpecificationImportRow(row.Number, task, specification));
        }

        private static void ReadBaseMaterials(List<SheetRow> rows, CatalogWorkbookData data, List<CatalogImportIssue> issues)
        {
            foreach (var row in DataRows(rows, "BaseMaterials", new[] { "Name", "BaseMaterialCoef" }, issues))
                if (Required(row, "BaseMaterials", "A", "Name", issues, out var name) && Decimal(row, "BaseMaterials", "B", "BaseMaterialCoef", issues, out var coefficient))
                    data.BaseMaterials.Add(new BaseMaterialImportRow(row.Number, name, coefficient));
        }

        private static void ReadPositions(List<SheetRow> rows, CatalogWorkbookData data, List<CatalogImportIssue> issues)
        {
            foreach (var row in DataRows(rows, "Positions", new[] { "Name", "PositionCoef" }, issues))
                if (Required(row, "Positions", "A", "Name", issues, out var name) && Decimal(row, "Positions", "B", "PositionCoef", issues, out var coefficient))
                    data.Positions.Add(new WorkPositionImportRow(row.Number, name, coefficient));
        }

        private static void ReadNamed(List<SheetRow> rows, string sheet, List<NamedImportRow> target, List<CatalogImportIssue> issues)
        {
            foreach (var row in DataRows(rows, sheet, new[] { "Name" }, issues))
                if (Required(row, sheet, "A", "Name", issues, out var name))
                    target.Add(new NamedImportRow(row.Number, name));
        }

        private static void ReadMaterials(List<SheetRow> rows, CatalogWorkbookData data, List<CatalogImportIssue> issues)
        {
            foreach (var row in DataRows(rows, "Materials", new[] { "Name", "CategoryName", "Unit", "DefaultPrice" }, issues))
            {
                if (Required(row, "Materials", "A", "Name", issues, out var name) &&
                    Required(row, "Materials", "B", "CategoryName", issues, out var category) &&
                    Required(row, "Materials", "C", "Unit", issues, out var unit) &&
                    Double(row, "Materials", "D", "DefaultPrice", issues, out var price))
                    data.Materials.Add(new MaterialImportRow(row.Number, name, category, unit, price));
            }
        }

        private static void ReadMaterialPrices(List<SheetRow> rows, CatalogWorkbookData data, List<CatalogImportIssue> issues)
        {
            var headers = new[] { "MaterialName", "SupplierName", "SupplierCode", "SupplierItemName", "Unit", "Price", "Currency", "UpdatedAt" };
            foreach (var row in DataRows(rows, "MaterialPrices", headers, issues))
            {
                if (!Required(row, "MaterialPrices", "A", "MaterialName", issues, out var material) ||
                    !Required(row, "MaterialPrices", "B", "SupplierName", issues, out var supplier) ||
                    !Required(row, "MaterialPrices", "C", "SupplierCode", issues, out var code) ||
                    !Required(row, "MaterialPrices", "D", "SupplierItemName", issues, out var supplierItem) ||
                    !Required(row, "MaterialPrices", "E", "Unit", issues, out var unit) ||
                    !Decimal(row, "MaterialPrices", "F", "Price", issues, out var price) ||
                    !Required(row, "MaterialPrices", "G", "Currency", issues, out var currency)) continue;

                DateTime? updatedAt = null;
                var dateValue = row["H"];
                if (!string.IsNullOrWhiteSpace(dateValue))
                {
                    if (double.TryParse(dateValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var serial))
                        updatedAt = DateTime.FromOADate(serial);
                    else if (DateTime.TryParse(dateValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                        updatedAt = parsed;
                    else
                    {
                        issues.Add(new CatalogImportIssue("MaterialPrices", row.Number, "UpdatedAt", "Datum není platné excelové nebo ISO datum."));
                        continue;
                    }
                }

                data.MaterialPrices.Add(new MaterialPriceImportRow(row.Number, material, supplier, code, supplierItem, unit, price, currency, updatedAt));
            }
        }

        private static IEnumerable<SheetRow> DataRows(List<SheetRow> rows, string sheet, string[] expectedHeaders, List<CatalogImportIssue> issues)
        {
            if (rows.Count == 0) return Array.Empty<SheetRow>();
            var header = rows[0];
            for (var i = 0; i < expectedHeaders.Length; i++)
            {
                var column = ColumnName(i + 1);
                if (!string.Equals(header[column], expectedHeaders[i], StringComparison.Ordinal))
                    issues.Add(new CatalogImportIssue(sheet, header.Number, column, $"Očekávána hlavička '{expectedHeaders[i]}'."));
            }
            return rows.Skip(1).Where(row => row.Values.Any(x => !string.IsNullOrWhiteSpace(x.Value)));
        }

        private static bool Required(SheetRow row, string sheet, string column, string header, List<CatalogImportIssue> issues, out string value)
        {
            value = row[column].Trim();
            if (!string.IsNullOrWhiteSpace(value)) return true;
            issues.Add(new CatalogImportIssue(sheet, row.Number, header, "Povinná hodnota chybí."));
            return false;
        }

        private static bool Decimal(SheetRow row, string sheet, string column, string header, List<CatalogImportIssue> issues, out decimal value)
        {
            if (decimal.TryParse(row[column], NumberStyles.Number, CultureInfo.InvariantCulture, out value) && value >= 0) return true;
            issues.Add(new CatalogImportIssue(sheet, row.Number, header, "Očekáváno nezáporné číslo."));
            return false;
        }

        private static bool Double(SheetRow row, string sheet, string column, string header, List<CatalogImportIssue> issues, out double value)
        {
            if (double.TryParse(row[column], NumberStyles.Number, CultureInfo.InvariantCulture, out value) && value >= 0) return true;
            issues.Add(new CatalogImportIssue(sheet, row.Number, header, "Očekáváno nezáporné číslo."));
            return false;
        }

        private static string ColumnName(int index)
        {
            var name = string.Empty;
            while (index > 0) { index--; name = (char)('A' + index % 26) + name; index /= 26; }
            return name;
        }

        private static XDocument LoadXml(ZipArchive archive, string path)
        {
            var entry = archive.GetEntry(path) ?? throw new InvalidDataException($"XLSX část '{path}' chybí.");
            using var stream = entry.Open();
            return XDocument.Load(stream);
        }

        private sealed record SheetRow(int Number, Dictionary<string, string> Values)
        {
            public string this[string column] => Values.TryGetValue(column, out var value) ? value : string.Empty;
        }
    }
}
