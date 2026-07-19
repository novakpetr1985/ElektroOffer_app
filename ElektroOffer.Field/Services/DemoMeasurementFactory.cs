using ElektroOffer.Contracts.Measurements;

namespace ElektroOffer.Field.Services;

public static class DemoMeasurementFactory
{
    public static MeasurementPackage Create()
    {
        var kitchen = new MeasurementArea { Name = "Kuchyně" };
        kitchen.Items.Add(new MeasurementItem
        {
            Kind = MeasurementKind.CableRoute,
            DisplayName = "Kabelová trasa CYKY 3×2,5",
            CatalogCode = "WORK-CABLE-CYKY-3X2.5",
            Quantity = 18.5m,
            Unit = "m",
            ReservePercent = 10,
            Note = "Vedení pod omítkou",
            WorkHints =
            [
                new WorkHint { WorkPositionCode = "WORK-GROOVE-001", DisplayName = "Drážkování ve zdivu", Quantity = 18.5m, Unit = "m", RuleId = "cable-wall-v1", Confidence = 0.9m },
                new WorkHint { WorkPositionCode = "WORK-CABLE-001", DisplayName = "Uložení kabelu", Quantity = 18.5m, Unit = "m", RuleId = "cable-wall-v1", Confidence = 0.95m }
            ],
            MaterialRequirements =
            [
                new MaterialRequirement { MaterialCode = "MAT-CYKY-3X2.5", Category = "Kabely", Specification = "CYKY-J 3×2,5", Quantity = 18.5m, Unit = "m", ReservePercent = 10 }
            ]
        });
        kitchen.Items.Add(new MeasurementItem
        {
            Kind = MeasurementKind.Socket,
            DisplayName = "Dvojzásuvka pod omítku",
            Quantity = 6,
            Unit = "ks",
            WorkHints = [new WorkHint { DisplayName = "Montáž zásuvky", Quantity = 6, Unit = "ks", RuleId = "socket-v1", Confidence = 0.85m }],
            MaterialRequirements = [new MaterialRequirement { Category = "Přístroje", Specification = "Dvojzásuvka 230 V pod omítku", Quantity = 6, Unit = "ks" }]
        });

        var hall = new MeasurementArea { Name = "Chodba" };
        hall.Items.Add(new MeasurementItem
        {
            Kind = MeasurementKind.Light,
            DisplayName = "Vývod pro svítidlo",
            Quantity = 3,
            Unit = "ks"
        });

        return new MeasurementPackage
        {
            SourceAppVersion = "1.13.0-feature",
            CatalogVersion = "demo-2026.07",
            Project = new MeasurementProject
            {
                Name = "Vzor – rodinný dům",
                CustomerName = "Jan Novák",
                SiteAddress = "Praha 1",
                TechnicianName = "Petr Novák",
                Note = "Testovací offline měření",
                Areas = [kitchen, hall]
            }
        };
    }
}
