using ElektroOffer_app.Data;
using ElektroOffer_app.Models;
using ElektroOffer_app.ViewModels.Items;
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Unit.ViewModels
{
    // =====================================================================
    // 🔧 UNIT TESTS – CalculationItemViewModel – KASKÁDA PRÁCE
    // =====================================================================
    // Tento soubor obsahuje testy ověřující kaskádu výběru pro PRÁCI:
    //
    //   Task → Specification → Material → Location → WorkItem
    //
    // Zaměřuje se na:
    //   • ResetBelowX metody (CalculationCascadeService)
    //   • PropertyChanged pro CanSelectX vlastnosti
    //   • správné dohledání WorkItem z DB po vyplnění celé kaskády
    //
    // Rozsah testů v tomto souboru:
    //   T_008–T_014
    //
    // Sdílený databázový kontext (_db) a SetUp/TearDown jsou definované
    // v TestBase.cs, ze kterého tato třída dědí.
    // =====================================================================
    [TestFixture]
    public class CalculationItemViewModelTests_CascadeWork : TestBase
    {
    
        // -----------------------------------------------------------------
        // 🧪 TEST 008: ResetBelowTask – musí vymazat Specification, Material, Location
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že ResetBelowTask vymaže všechny hodnoty pod SelectedTask
        //  • že kaskáda se chová konzistentně
        // -----------------------------------------------------------------
        [Test]
        [Order(008)]
        public void T_008_CascadeWork_ResetBelowTask_Should_Clear_All_Selections()
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
        // 🧪 TEST 009: ResetBelowSpecification – musí vymazat Material + Location
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že ResetBelowSpecification vymaže materiál a lokaci
        // -----------------------------------------------------------------
        [Test]
        [Order(009)]
        public void T_009_CascadeWork_ResetBelowSpecification_Should_Clear_Material_And_Location()
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
        // 🧪 TEST 010: ResetBelowMaterial – musí vymazat Location
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že ResetBelowMaterial vymaže pouze SelectedLocation
        // -----------------------------------------------------------------
        [Test]
        [Order(010)]
        public void T_010_CascadeWork_ResetBelowMaterial_Should_Clear_Location()
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
        // 🧪 TEST 011: SelectedTask – musí vyvolat PropertyChanged pro CanSelectSpecification
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že změna SelectedTask aktivuje první krok WORK kaskády
        //  • že ViewModel správně vyvolá PropertyChanged pro CanSelectSpecification
        //  • že UI může reagovat (povolit výběr Specification)
        //  • že logika kaskády správně navazuje na výběr Task
        // -----------------------------------------------------------------
        [Test]
        [Order(011)]
        public void T_011_CascadeWork_SelectedTask_Should_Raise_PropertyChanged_For_CanSelectSpecification()
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
        // 🧪 TEST 012: SelectedSpecification – musí vyvolat PropertyChanged pro CanSelectMaterial
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že změna SelectedSpecification aktivuje druhý krok WORK kaskády
        //  • že ViewModel správně vyvolá PropertyChanged pro CanSelectMaterial
        //  • že UI může reagovat (povolit výběr Material)
        //  • že logika kaskády správně navazuje na výběr Specification
        // -----------------------------------------------------------------
        [Test]
        [Order(012)]
        public void T_012_CascadeWork_SelectedSpecification_Should_Raise_PropertyChanged_For_CanSelectMaterial()
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
        // 🧪 TEST 013: SelectedMaterial – musí vyvolat PropertyChanged pro CanSelectLocation
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že změna SelectedMaterial aktivuje třetí krok WORK kaskády
        //  • že ViewModel správně vyvolá PropertyChanged pro CanSelectLocation
        //  • že UI může reagovat (povolit výběr Location)
        //  • že logika kaskády správně navazuje na výběr Material
        // -----------------------------------------------------------------
        [Test]
        [Order(013)]
        public void T_013_CascadeWork_SelectedMaterial_Should_Raise_PropertyChanged_For_CanSelectLocation()
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
        // 🧪 TEST 014: SelectedLocation – musí aktualizovat WorkItem
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
        [Order(014)]
        public void T_014_CascadeWork_SelectedLocation_Should_Update_WorkItem()
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