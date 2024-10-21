using SequentialSelector.ViewModels;

namespace SequentialSelector.Views
{
    public partial class SequentialSelectorView
    {
        public SequentialSelectorView(SequentialSelectorViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}