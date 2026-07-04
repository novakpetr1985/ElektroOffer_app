
using NUnit.Framework;
using ElektroOffer_app.Commands;
using ElektroOffer_app.Models;


namespace ElektroOffer_app.Tests.Unit.CommandTests
{
    /// ============================================================
    /// 🧩 UNIT TESTS — RelayCommand
    /// Testujeme MVVM command logiku (Execute, CanExecute, eventy).
    /// ============================================================
    public class RelayCommandTests
    {
        [Test]
        public void Execute_Should_Invoke_Action()
        {
            // 📝 Arrange — flag pro ověření, že se akce vykonala
            bool executed = false;

            var cmd = new RelayCommand(_ =>
            {
                executed = true; // 🔥 akce, kterou očekáváme
            });

            // 🔧 Act
            cmd.Execute(null);

            // ✅ Assert
            Assert.IsTrue(executed);
        }

        [Test]
        public void CanExecute_Should_Block_Execution()
        {
            // 📝 Arrange — CanExecute vrací false
            var cmd = new RelayCommand(_ => { }, _ => false);

            // 🔧 Act + Assert
            Assert.IsFalse(cmd.CanExecute(null));
        }

        [Test]
        public void CanExecuteChanged_Should_Fire()
        {
            // 📝 Arrange
            var cmd = new RelayCommand(_ => { });
            bool fired = false;

            cmd.CanExecuteChanged += (_, __) =>
            {
                fired = true; // 🔥 event byl vyvolán
            };

            // 🔧 Act
            cmd.RaiseCanExecuteChanged();

            // ✅ Assert
            Assert.IsTrue(fired);
        }
    }
}
