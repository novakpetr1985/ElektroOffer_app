using ElektroOffer.Contracts.Measurements;
using ElektroOffer.Contracts.Catalog;

namespace ElektroOffer.Field.Services;

public static class DemoMeasurementFactory
{
    public static MeasurementPackage Create(FieldCatalogSnapshot catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        var work = catalog.Options.First(option => option.Kind == FieldCatalogOptionKind.Work);
        var materialCategory = catalog.Options.First(option => option.Kind == FieldCatalogOptionKind.MaterialCategory);
        var kitchen = new MeasurementArea { Name = "Kuchyně" };
        kitchen.Items.Add(new MeasurementItem
        {
            Kind = MeasurementKind.CableRoute,
            DisplayName = work.Name,
            CatalogCode = work.Code,
            Quantity = 18.5m,
            Unit = string.IsNullOrWhiteSpace(work.Unit) ? "m" : work.Unit,
            Note = "Vedení pod omítkou",
            WorkHints =
            [
                new WorkHint { CatalogCode = work.Code, DisplayName = work.Name, Quantity = 18.5m, Unit = string.IsNullOrWhiteSpace(work.Unit) ? "m" : work.Unit, RuleId = "catalog-selection-v1", Confidence = 1m }
            ]
        });
        kitchen.Items.Add(new MeasurementItem
        {
            Kind = materialCategory.Name.Contains("kabel", StringComparison.OrdinalIgnoreCase) ? MeasurementKind.CableRoute : MeasurementKind.Custom,
            DisplayName = materialCategory.Name,
            CatalogCode = materialCategory.Code,
            Quantity = 20,
            Unit = string.IsNullOrWhiteSpace(materialCategory.Unit) ? "m" : materialCategory.Unit,
            ReservePercent = 10,
            MaterialRequirements = [new MaterialRequirement
            {
                CategoryCode = materialCategory.Code,
                Category = materialCategory.Name,
                Quantity = 20,
                Unit = string.IsNullOrWhiteSpace(materialCategory.Unit) ? "m" : materialCategory.Unit,
                ReservePercent = 10
            }]
        });

        return new MeasurementPackage
        {
            SourceAppVersion = "1.13.0-feature",
            CatalogVersion = catalog.CatalogVersion,
            Project = new MeasurementProject
            {
                Name = "Vzor – rodinný dům",
                CustomerName = "Jan Novák",
                SiteAddress = "Praha 1",
                TechnicianName = "Petr Novák",
                Note = "Testovací offline měření",
                Areas = [kitchen]
            }
        };
    }
}
