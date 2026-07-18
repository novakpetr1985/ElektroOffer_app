
namespace ElektroOffer_app.Models
{
    // =========================================================================
    // 🛠 WorkTask – jeden úkon v ceníku PRÁCE (tabulka "Tasks")
    // =========================================================================
    //
    // ⚠️ Pojmenováno WorkTask (ne "Task") schválně – "Task" koliduje
    // s System.Threading.Tasks.Task a způsobovalo by matoucí chyby.
    //
    // BasePrice je základní cena úkonu, ze které se v kaskádě PRÁCE počítá
    // výsledná cena: BasePrice × BaseMaterial.Coef × Position.Coef × Quantity.
    // =========================================================================
    public class WorkTask
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
    }
}