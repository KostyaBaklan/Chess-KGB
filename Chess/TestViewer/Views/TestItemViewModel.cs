using System.Collections.ObjectModel;
using Common;
using Prism.Mvvm;

namespace TestViewer.Views
{
    public class TestItemViewModel:BindableBase
    {
        public TestItemViewModel(int depth)
        {
            Depth = depth;
            TestItems = new ObservableCollection<TestModel>();
        }

        public int Depth { get; }
        public ObservableCollection<TestModel> TestItems { get; }
    }
}