using BankerDeskOps.Wpf.ViewModels;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace BankerDeskOps.Wpf.Views
{
    public partial class ProductView : UserControl
    {
        public ProductView()
        {
            InitializeComponent();
            Loaded += async (s, e) =>
            {
                if (DataContext is ProductViewModel viewModel)
                {
                    await viewModel.LoadCurrenciesCommand.ExecuteAsync(null);
                }
            };
        }

        private void NumericOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsNumericInput(e.Text);
        }

        private void DecimalOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsDecimalInput(e.Text);
        }

        private static bool IsNumericInput(string text) => Regex.IsMatch(text, "^[0-9]+$");
        private static bool IsDecimalInput(string text) => Regex.IsMatch(text, "^[0-9.]*$");
    }
}
