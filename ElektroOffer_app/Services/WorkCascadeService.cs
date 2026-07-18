using System.Linq;
using ElektroOffer_app.Data;
using ElektroOffer_app.Models;
using ElektroOffer_app.ViewModels.Items;

namespace ElektroOffer_app.Services
{
    // =========================================================================
    // 🔧 WorkCascadeService – řízení kaskády PRÁCE (1.9.0 – New Work Cascade)
    // =========================================================================
    //
    // Řídí řádkovou část kaskády PRÁCE:
    //   • WorkTask → WorkSpecification   (omezení přes TaskSpecifications)
    //   • WorkSpecification → BaseMaterial (UI povolení až po předchozím výběru)
    //   • BaseMaterial → WorkPosition      (UI povolení až po předchozím výběru)
    //
    // WorkTask/BaseMaterial/WorkPosition seznamy jsou SDÍLENÉ na úrovni
    // MainViewModel (nabízí se stejné pro každý řádek) – tahle služba pro ně
    // tedy nemá LoadX metodu. Jediný seznam vázaný na konkrétní řádek je
    // AvailableWorkSpecifications, protože závisí na SelectedWorkTask.
    //
    // U každé vybrané textové hodnoty (SelectedWorkTask,
    // SelectedBaseMaterial, SelectedWorkPosition) zároveň dohledává a ukládá
    // odpovídající EF entitu (SelectedWorkTaskEntity, SelectedBaseMaterialEntity,
    // SelectedWorkPositionEntity) – z nich CalculationPriceService čte
    // BasePrice / BaseMaterialCoef / PositionCoef.
    // =========================================================================
    public class WorkCascadeService
    {
        private readonly AppDbContext _db;

        public WorkCascadeService(AppDbContext db)
        {
            _db = db;
        }

        // =========================================================
        // SPECIFICATIONS (jediné omezení kaskády – podle SelectedWorkTask)
        // =========================================================
        public void LoadWorkSpecifications(CalculationItemViewModel vm)
        {
            vm.AvailableWorkSpecifications.Clear();

            if (string.IsNullOrWhiteSpace(vm.SelectedWorkTask))
                return;

            var list = _db.TaskSpecifications
                .Where(ts => ts.Task!.Name == vm.SelectedWorkTask)
                .Select(ts => ts.Specification!.Name)
                .Distinct()
                .ToList();

            foreach (var name in list)
                vm.AvailableWorkSpecifications.Add(name);
        }

        // =========================================================
        // WORK UNIT (z vybrané Specifikace – jen pro zobrazení, do ceny nevstupuje)
        // =========================================================
        public void LoadWorkUnit(CalculationItemViewModel vm)
        {
            vm.WorkUnit = _db.Specifications
                .Where(s => s.Name == vm.SelectedWorkSpecification)
                .Select(s => s.Unit)
                .FirstOrDefault();
        }

        // =========================================================
        // ENTITY RESOLUTION (pro výpočet ceny)
        // =========================================================

        /// <summary>Dohledá WorkTask entitu podle vybraného názvu a uloží ji do vm (kvůli BasePrice).</summary>
        public void UpdateSelectedWorkTask(CalculationItemViewModel vm)
        {
            vm.SelectedWorkTaskEntity = string.IsNullOrWhiteSpace(vm.SelectedWorkTask)
                ? null
                : _db.Tasks.FirstOrDefault(t => t.Name == vm.SelectedWorkTask);
        }

        /// <summary>Dohledá BaseMaterial entitu podle vybraného názvu a uloží ji do vm (kvůli BaseMaterialCoef).</summary>
        public void UpdateSelectedBaseMaterial(CalculationItemViewModel vm)
        {
            vm.SelectedBaseMaterialEntity = string.IsNullOrWhiteSpace(vm.SelectedBaseMaterial)
                ? null
                : _db.BaseMaterials.FirstOrDefault(b => b.Name == vm.SelectedBaseMaterial);
        }

        /// <summary>Dohledá WorkPosition entitu podle vybraného názvu a uloží ji do vm (kvůli PositionCoef).</summary>
        public void UpdateSelectedWorkPosition(CalculationItemViewModel vm)
        {
            vm.SelectedWorkPositionEntity = string.IsNullOrWhiteSpace(vm.SelectedWorkPosition)
                ? null
                : _db.Positions.FirstOrDefault(p => p.Name == vm.SelectedWorkPosition);
        }

        // =========================================================
        // RESETY
        // =========================================================

        /// <summary>
        /// Volá se při změně SelectedWorkTask. Resetuje všechny nižší kroky,
        /// aby UI vždy postupovalo Úkon → Upřesnění → Podklad → Umístění.
        /// </summary>
        public void ResetBelowWorkTask(CalculationItemViewModel vm)
        {
            vm.SelectedWorkSpecification = null;
            vm.SelectedBaseMaterial = null;
            vm.SelectedWorkPosition = null;
            vm.AvailableWorkSpecifications.Clear();
            vm.WorkUnit = null;
            vm.SelectedBaseMaterialEntity = null;
            vm.SelectedWorkPositionEntity = null;
        }

        public void ResetBelowWorkSpecification(CalculationItemViewModel vm)
        {
            vm.SelectedBaseMaterial = null;
            vm.SelectedWorkPosition = null;
            vm.SelectedBaseMaterialEntity = null;
            vm.SelectedWorkPositionEntity = null;
        }

        public void ResetBelowBaseMaterial(CalculationItemViewModel vm)
        {
            vm.SelectedWorkPosition = null;
            vm.SelectedWorkPositionEntity = null;
        }
    }
}
