namespace ElektroOffer_app.Models
{
    public sealed class CatalogWorkbookData
    {
        public List<WorkTaskImportRow> Tasks { get; } = new();
        public List<WorkSpecificationImportRow> Specifications { get; } = new();
        public List<TaskSpecificationImportRow> TaskSpecifications { get; } = new();
        public List<BaseMaterialImportRow> BaseMaterials { get; } = new();
        public List<WorkPositionImportRow> Positions { get; } = new();
        public List<NamedImportRow> Categories { get; } = new();
        public List<NamedImportRow> Suppliers { get; } = new();
        public List<MaterialImportRow> Materials { get; } = new();
        public List<MaterialPriceImportRow> MaterialPrices { get; } = new();
    }

    public sealed record WorkTaskImportRow(int Row, string Name, decimal BasePrice);
    public sealed record WorkSpecificationImportRow(int Row, string Name, string Unit);
    public sealed record TaskSpecificationImportRow(int Row, string TaskName, string SpecificationName);
    public sealed record BaseMaterialImportRow(int Row, string Name, decimal Coefficient);
    public sealed record WorkPositionImportRow(int Row, string Name, decimal Coefficient);
    public sealed record NamedImportRow(int Row, string Name);
    public sealed record MaterialImportRow(int Row, string Name, string CategoryName, string Unit, double DefaultPrice);
    public sealed record MaterialPriceImportRow(
        int Row,
        string MaterialName,
        string SupplierName,
        string SupplierCode,
        string SupplierItemName,
        string Unit,
        decimal Price,
        string Currency,
        DateTime? UpdatedAt);

    public sealed record CatalogImportIssue(string Sheet, int Row, string Column, string Message);

    public sealed class CatalogImportResult
    {
        public bool Success => Issues.Count == 0;
        public int Inserted { get; set; }
        public int Updated { get; set; }
        public List<CatalogImportIssue> Issues { get; } = new();
    }
}
