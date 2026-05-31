using System.ComponentModel;
using ElektroOffer_app.Models;

public class CalculationItems : INotifyPropertyChanged
{
    private Material _item;
    public Material Item
    {
        get => _item;
        set
        {
            _item = value;
            OnPropertyChanged(nameof(Item));
            OnPropertyChanged(nameof(Total));
        }
    }

    private double _quantity;
    public double Quantity
    {
        get => _quantity;
        set
        {
            _quantity = value;
            OnPropertyChanged(nameof(Quantity));
            OnPropertyChanged(nameof(Total));
        }
    }

    public double Total
    {
        get
        {
            if (Item == null)
                return 0;

            return Item.Price * Quantity;
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}