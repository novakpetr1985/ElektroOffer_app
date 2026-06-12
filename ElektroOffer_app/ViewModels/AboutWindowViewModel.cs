using ElektroOffer_app.ViewModels.Base;

namespace ElektroOffer_app.ViewModels
{
    // =========================================================
    // ℹ ABOUT WINDOW VIEWMODEL
    // =========================================================
    public class AboutWindowViewModel : BaseViewModel
    {
        private string _appName = "ElektroOffer";
        private string _version = "1.5.1";
        private string _author = "Petr";
        private string _description =
            "Aplikace pro kalkulaci elektro zakázek.\n" +
            "Umožňuje pracovat s ceníkem práce a materiálu,\n" +
            "ukládat projekty a exportovat/importovat ceník.";

        public string AppName
        {
            get => _appName;
            set => SetProperty(ref _appName, value);
        }

        public string Version
        {
            get => _version;
            set => SetProperty(ref _version, value);
        }

        public string Author
        {
            get => _author;
            set => SetProperty(ref _author, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }
    }
}
