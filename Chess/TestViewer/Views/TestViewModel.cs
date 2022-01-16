using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Newtonsoft.Json;

namespace TestViewer.Views
{
    public class TestViewModel
    {
        public TestViewModel()
        {
            Tests = new ObservableCollection<TestItemViewModel>();

            var files = Directory.GetFiles(@"..\..\..\Tests\Redist\Log", "*.log", SearchOption.TopDirectoryOnly);
            List<TestModel> models = new List<TestModel>(files.Length);
            foreach (var file in files)
            {
                var content = File.ReadAllText(file);
                var testModel = JsonConvert.DeserializeObject<TestModel>(content);
                models.Add(testModel);
            }

            var map = models.GroupBy(m => m.Depth).ToDictionary(k => k.Key, v => v.ToList());
            foreach (var pair in map)
            {
                TestItemViewModel testItemViewModel = new TestItemViewModel(pair.Key);
                foreach (var testModel in pair.Value)
                {
                    testItemViewModel.TestItems.Add(testModel);
                }
                Tests.Add(testItemViewModel);
            }
        }

        public ObservableCollection<TestItemViewModel> Tests { get; }
    }
}
