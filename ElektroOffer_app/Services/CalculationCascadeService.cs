using System.Collections.ObjectModel;
using System.Linq;
using ElektroOffer_app.Data;
using ElektroOffer_app.Models;
using ElektroOffer_app.ViewModels.Items;

namespace ElektroOffer_app.Services
{
    /// <summary>
    /// Služba pro řízení kaskády Task → Specification → Material → Location.
    /// Obsahuje veškerou logiku, která byla dříve v CalculationItemViewModel.
    /// </summary>
    public class CalculationCascadeService
    {
        private readonly AppDbContext _db;

        public CalculationCascadeService(AppDbContext db)
        {
            _db = db;
        }

        // =========================================================
        // SPECIFICATIONS
        // =========================================================
        public void LoadSpecifications(CalculationItemViewModel vm)
        {
            vm.AvailableSpecifications.Clear();

            var list = _db.PriceItems
                .Where(x => x.Task == vm.SelectedTask)
                .Select(x => x.Specification)
                .Distinct()
                .ToList();

            foreach (var item in list)
                vm.AvailableSpecifications.Add(item);
        }

        // =========================================================
        // MATERIALS
        // =========================================================
        public void LoadMaterials(CalculationItemViewModel vm)
        {
            vm.AvailableMaterials.Clear();

            var list = _db.PriceItems
                .Where(x => x.Task == vm.SelectedTask &&
                            x.Specification == vm.SelectedSpecification)
                .Select(x => x.Material)
                .Distinct()
                .ToList();

            foreach (var item in list)
                vm.AvailableMaterials.Add(item);
        }

        // =========================================================
        // LOCATIONS
        // =========================================================
        public void LoadLocations(CalculationItemViewModel vm)
        {
            vm.AvailableLocations.Clear();

            if (vm.SelectedTask == null ||
                vm.SelectedSpecification == null ||
                vm.SelectedMaterial == null)
                return;

            var locations = _db.PriceItems
                .Where(p => p.Task == vm.SelectedTask &&
                            p.Specification == vm.SelectedSpecification &&
                            p.Material == vm.SelectedMaterial)
                .Select(p => p.Location)
                .Distinct()
                .ToList();

            foreach (var loc in locations)
                vm.AvailableLocations.Add(loc);
        }

        // =========================================================
        // WORK UNIT
        // =========================================================
        public void LoadWorkUnit(CalculationItemViewModel vm)
        {
            vm.WorkUnit = _db.PriceItems
                .Where(x => x.Specification == vm.SelectedSpecification)
                .Select(x => x.Unit)
                .FirstOrDefault();
        }

        // =========================================================
        // WORK ITEM
        // =========================================================
        public void UpdateWorkItem(CalculationItemViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.SelectedTask) ||
                string.IsNullOrWhiteSpace(vm.SelectedSpecification) ||
                string.IsNullOrWhiteSpace(vm.SelectedMaterial) ||
                string.IsNullOrWhiteSpace(vm.SelectedLocation))
            {
                vm.WorkItem = null;
                return;
            }

            vm.WorkItem = _db.PriceItems
                .FirstOrDefault(x =>
                    x.Task == vm.SelectedTask &&
                    x.Specification == vm.SelectedSpecification &&
                    x.Material == vm.SelectedMaterial &&
                    x.Location == vm.SelectedLocation);
        }

        // =========================================================
        // RESETY
        // =========================================================
        public void ResetBelowTask(CalculationItemViewModel vm)
        {
            vm.SelectedSpecification = null;
            vm.SelectedMaterial = null;
            vm.SelectedLocation = null;

            vm.AvailableSpecifications.Clear();
            vm.AvailableMaterials.Clear();
            vm.AvailableLocations.Clear();

            vm.WorkItem = null;
            vm.WorkUnit = null;
        }

        public void ResetBelowSpecification(CalculationItemViewModel vm)
        {
            vm.SelectedMaterial = null;
            vm.SelectedLocation = null;

            vm.AvailableMaterials.Clear();
            vm.AvailableLocations.Clear();

            vm.WorkItem = null;
        }

        public void ResetBelowMaterial(CalculationItemViewModel vm)
        {
            vm.SelectedLocation = null;

            vm.AvailableLocations.Clear();
            vm.WorkItem = null;
        }
    }
}
