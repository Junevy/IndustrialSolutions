namespace VisionServices.Core
{
    public interface ISolution : IDisposable
    {
        public void Load(string solutionPath);

        public Task LoadAsync(string solutionPath);

        public void Save();

        public void SaveAs(string targetPath);

        public void Run();

        public void RunAsync();
        
        public bool SetModuParam<T>(string paramName, T value, string groupName = "流程1");

        public T? GetModuResult<T>(string algorithmName, string paramName, string groupName = "流程1");

        public object? GetModule(string moduleName,string groupName = "流程1");

        public T? GetModule<T>(string moduleName, string groupName = "流程1") where T : class;
    }
}
