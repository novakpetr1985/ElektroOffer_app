using System.Linq;
using ElektroOffer_app.Data;
using ElektroOffer_app.Models;
using ElektroOffer_app.ViewModels.Items;

namespace ElektroOffer_app.Services
{
    // =========================================================================
    // 🔗 WorkCascadeService – kaskádový výběr úkonu, upřesnění a koeficientů
    // =========================================================================
    //
    // K čemu slouží:
    // - Analogie k MaterialCascadeService, ale pro normalizovanou kaskádu
    //   Práce: WorkTask → WorkSpecification → (BaseMaterial + Position).
    //
    // KASKÁDA:
    //     WorkTask → WorkSpecification
    //         (filtrováno přes vazební tabulku TaskSpecification –
    //          nabízí se jen PLATNÉ kombinace, viz zadání uživatele)
    //
    //     BaseMaterial, Position
    //         (VŽDY plný seznam, nezávisle na Task/Specification –
    //          u všech položek jsou platné všechny kombinace)
    //
    // VÝSLEDNÁ CENA:
    //     WorkTask.BasePrice × BaseMaterial.MaterialCoef × Position.PositionCoef
    //
    // Na rozdíl od MaterialCascadeService zde NENÍ potřeba hledat/lookupovat
    // uloženou cenu (obdobu MaterialPrice) – cena se vždy dopočítává vzorcem.
    // =========================================================================
    public class WorkCascadeService
    {
        private readonly AppDbContext _db;

        public WorkCascadeService(AppDbContext db)
        {
            _db = db;
        }

        // =========================================================
        // ÚKONY (počáteční seznam, nezávisí na ničem)
        // =========================================================
        public void LoadTasks(CalculationItemViewModel vm)
        {
            vm.AvailableWorkTasks.Clear();

            var list = _db.WorkTasks
                .OrderBy(t => t.Name)
                .ToList();

            foreach (var task in list)
                vm.AvailableWorkTasks.Add(task);
        }

        // =========================================================
        // UPŘESNĚNÍ (filtrované podle vybraného Úkonu přes TaskSpecification)
        // =========================================================
        public void LoadSpecifications(CalculationItemViewModel vm)
        {
            vm.AvailableWorkSpecifications.Clear();

            if (vm.SelectedWorkTask == null)
                return;

            var list = _db.TaskSpecifications
                .Where(ts => ts.TaskId == vm.SelectedWorkTask.Id)
                .Select(ts => ts.Specification)
                .Distinct()
                .OrderBy(s => s.Name)
                .ToList();

            foreach (var spec in list)
                vm.AvailableWorkSpecifications.Add(spec);
        }

        // =========================================================
        // PODKLADOVÝ MATERIÁL (vždy plný seznam, nefiltruje se)
        // =========================================================
        public void LoadBaseMaterials(CalculationItemViewModel vm)
        {
            vm.AvailableBaseMaterials.Clear();

            var list = _db.BaseMaterials
                .OrderBy(m => m.Id)
                .ToList();

            foreach (var m in list)
                vm.AvailableBaseMaterials.Add(m);
        }

        // =========================================================
        // POLOHA (vždy plný seznam, nefiltruje se)
        // =========================================================
        public void LoadPositions(CalculationItemViewModel vm)
        {
            vm.AvailablePositions.Clear();

            var list = _db.Positions
                .OrderBy(p => p.Id)
                .ToList();

            foreach (var p in list)
                vm.AvailablePositions.Add(p);
        }

        // =========================================================
        // VÝSLEDNÁ CENA (WorkTask.BasePrice × MaterialCoef × PositionCoef)
        // =========================================================
        public void UpdatePrice(CalculationItemViewModel vm)
        {
            if (vm.SelectedWorkTask == null ||
                vm.SelectedBaseMaterial == null ||
                vm.SelectedPosition == null)
            {
                vm.CalculatedWorkPrice = null;
                return;
            }

            vm.CalculatedWorkPrice =
                vm.SelectedWorkTask.BasePrice
                * (decimal)vm.SelectedBaseMaterial.MaterialCoef
                * (decimal)vm.SelectedPosition.PositionCoef;
        }

        // =========================================================
        // RESETY (stejný vzor jako u CalculationCascadeService/MaterialCascadeService)
        // =========================================================
        //
        // Pozn.: BaseMaterial a Position se resetovat NEMUSÍ – jejich
        // seznamy nejsou na Task/Specification závislé, takže zůstávají
        // platné bez ohledu na to, co uživatel vybere v prvních dvou krocích.
        // =========================================================
        public void ResetBelowTask(CalculationItemViewModel vm)
        {
            vm.SelectedWorkSpecification = null;
            vm.AvailableWorkSpecifications.Clear();
            vm.CalculatedWorkPrice = null;
        }
    }
}