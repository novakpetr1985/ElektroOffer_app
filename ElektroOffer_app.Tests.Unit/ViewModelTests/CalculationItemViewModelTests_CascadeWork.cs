using ElektroOffer_app.Data;
using ElektroOffer_app.Models;
using ElektroOffer_app.ViewModels.Items;
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Unit.ViewModels
{
    // =====================================================================
    // 🔧 UNIT TESTS – CalculationItemViewModel – KASKÁDA PRÁCE
    // =====================================================================
    // Tato část třídy (partial) ověřuje kaskádu výběru pro PRÁCI:
    //   Task → Specification → Material → Location → WorkItem
    //
    // Zaměřuje se na:
    //   • ResetBelowX metody (CalculationCascadeService) – že se při změně
    //     vyšší úrovně vyčistí všechny nižší úrovně
    //   • PropertyChanged pro CanSelectX vlastnosti (např. CanSelectSpecification)
    //   • že se po vyplnění celé kaskády včetně Location správně dohledá
    //     odpovídající PriceItems záznam z DB (WorkItem)
    //
    // Rozsah testů v tomto souboru:
    //   T_073–T_079
    //
    // Setup/TearDown a sdílená pole _db/_connection jsou definované
    // v CalculationItemViewModelTests_Base.cs (partial class).
    // =====================================================================
    public partial class CalculationItemViewModelTests
    {
    
        // -----------------------------------------------------------------
        // 🧪 TEST 01: ResetBelowTask – musí vymazat Specification, Material, Location
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že ResetBelowTask vymaže všechny hodnoty pod SelectedTask
        //  • že kaskáda se chová konzistentně
        // -----------------------------------------------------------------
        [Test]
        [Order(01)]
        public void T_01_CascadeWork_ResetBelowTask_Should_Clear_All_Selections()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                SelectedTask = "Montáž",
                SelectedSpecification = "Zásuvka",
                SelectedMaterial = "CYKY 3x2.5",
                SelectedLocation = "Obývák"
            };

            vm.SelectedTask = "Nový úkon"; // vyvolá ResetBelowTask

            Assert.IsNull(vm.SelectedSpecification);
            Assert.IsNull(vm.SelectedMaterial);
            Assert.IsNull(vm.SelectedLocation);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 02: ResetBelowSpecification – musí vymazat Material + Location
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že ResetBelowSpecification vymaže materiál a lokaci
        // -----------------------------------------------------------------
        [Test]
        [Order(02)]
        public void T_02_CascadeWork_ResetBelowSpecification_Should_Clear_Material_And_Location()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                SelectedTask = "Montáž",
                SelectedSpecification = "Zásuvka",
                SelectedMaterial = "CYKY 3x2.5",
                SelectedLocation = "Obývák"
            };

            vm.SelectedSpecification = "Nová specifikace";

            Assert.IsNull(vm.SelectedMaterial);
            Assert.IsNull(vm.SelectedLocation);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 03: ResetBelowMaterial – musí vymazat Location
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že ResetBelowMaterial vymaže pouze SelectedLocation
        // -----------------------------------------------------------------
        [Test]
        [Order(03)]
        public void T_03_CascadeWork_ResetBelowMaterial_Should_Clear_Location()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                SelectedTask = "Montáž",
                SelectedSpecification = "Zásuvka",
                SelectedMaterial = "CYKY 3x2.5",
                SelectedLocation = "Obývák"
            };

            vm.SelectedMaterial = "Jiný materiál";

            Assert.IsNull(vm.SelectedLocation);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 04: SelectedTask – musí vyvolat PropertyChanged pro CanSelectSpecification
        // -----------------------------------------------------------------
        [Test]
        [Order(04)]
        public void T_04_CascadeWork_SelectedTask_Should_Raise_PropertyChanged_For_CanSelectSpecification()
        {
            var vm = new CalculationItemViewModel(_db);

            bool raised = false;

            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(vm.CanSelectSpecification))
                    raised = true;
            };

            vm.SelectedTask = "Montáž";

            Assert.IsTrue(raised);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 05: SelectedSpecification – musí vyvolat PropertyChanged pro CanSelectMaterial
        // -----------------------------------------------------------------
        [Test]
        [Order(05)]
        public void T_05_CascadeWork_SelectedSpecification_Should_Raise_PropertyChanged_For_CanSelectMaterial()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                SelectedTask = "Montáž"
            };

            bool raised = false;

            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(vm.CanSelectMaterial))
                    raised = true;
            };

            vm.SelectedSpecification = "Zásuvka";

            Assert.IsTrue(raised);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 06: SelectedMaterial – musí vyvolat PropertyChanged pro CanSelectLocation
        // -----------------------------------------------------------------
        [Test]
        [Order(06)]
        public void T_06_CascadeWork_SelectedMaterial_Should_Raise_PropertyChanged_For_CanSelectLocation()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                SelectedTask = "Montáž",
                SelectedSpecification = "Zásuvka"
            };

            bool raised = false;

            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(vm.CanSelectLocation))
                    raised = true;
            };

            vm.SelectedMaterial = "CYKY 3x2.5";

            Assert.IsTrue(raised);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 07: SelectedLocation – musí aktualizovat WorkItem
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že změna lokace vyvolá _cascade.UpdateWorkItem()
        //  • že WorkItem se změní (není null)
        //
        // 🔥 OPRAVA:
        //  • UpdateWorkItem() hledá řádek v _db.PriceItems podle
        //    Task + Specification + Material + Location.
        //  • Test dřív žádný takový řádek neseedoval, takže hledání
        //    vždy vrátilo null. Teď se odpovídající PriceItems záznam
        //    nejdřív uloží do testovací DB.
        // -----------------------------------------------------------------
        [Test]
        [Order(07)]
        public void T_07_CascadeWork_SelectedLocation_Should_Update_WorkItem()
        {
            // 1) Seed odpovídajícího PriceItems záznamu
            _db.PriceItems.Add(new PriceItems
            {
                Task = "Montáž",
                Specification = "Zásuvka",
                Material = "CYKY 3x2.5",
                Location = "Obývák",
                Unit = "ks"
            });
            _db.SaveChanges(); // 🔥 nutné, aby ho UpdateWorkItem() našel

            // 2) ViewModel – projede kaskádu Task → Specification → Material
            var vm = new CalculationItemViewModel(_db)
            {
                SelectedTask = "Montáž",
                SelectedSpecification = "Zásuvka",
                SelectedMaterial = "CYKY 3x2.5"
            };

            vm.SelectedLocation = "Obývák";

            // 3) Ověření
            Assert.IsNotNull(vm.WorkItem);
        }
    }
}