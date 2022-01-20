using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Common;
using Newtonsoft.Json;
using Prism.Commands;

namespace TestViewer.Views
{
    public class TestViewModel
    {
        public TestViewModel()
        {
            var files = Directory.GetFiles(@"..\..\..\Tests\Redist\Log", "*.log", SearchOption.TopDirectoryOnly);
            List<TestModel> models = new List<TestModel>(files.Length);
            foreach (var file in files)
            {
                var content = File.ReadAllText(file);
                var testModel = JsonConvert.DeserializeObject<TestModel>(content);
                models.Add(testModel);
            }

            var map = models.GroupBy(m => m.Depth)
                .ToDictionary(k => k.Key, v => v.ToList());

            var t = new List<TestItemViewModel>();

            foreach (var pair in map)
            {
                var ti = new List<TestModel>();
                foreach (var testModel in pair.Value)
                {
                    ti.Add(testModel);
                }

                TestItemViewModel testItemViewModel = new TestItemViewModel(pair.Key,ti);
                t.Add(testItemViewModel);
            }

            Tests = new List<TestItemViewModel>(t);

            LoadedCommand = new DelegateCommand(OnLoaded);
        }

        public ICollection<TestItemViewModel> Tests { get; }

        public ICommand LoadedCommand { get; }

        private void OnLoaded()
        {
            var tests = Tests.ToArray();
            foreach (var testItemViewModel in tests)
            {
                testItemViewModel.CreateResults();
            }
        }
    }
}
