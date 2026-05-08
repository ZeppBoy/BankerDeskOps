using System.Windows;
using BankerDeskOps.Wpf.ViewModels;

namespace BankerDeskOps.Wpf.Views
{
    public partial class CreateRepaymentScheduleWindow : Window
    {
        public CreateRepaymentScheduleWindow()
        {
            InitializeComponent();
            Loaded += async (s, e) =>
            {
                if (DataContext is CreateRepaymentScheduleViewModel viewModel)
                    await viewModel.InitializeAsync();
            };
        }
    }
}
