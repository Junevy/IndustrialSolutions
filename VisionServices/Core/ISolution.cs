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

        //public bool SetParam<T>(string key, T value);

        public object? GetModuResult(string algorithmName, string paramName, string groupName = "流程1");
        public T? GetModuResult<T>(string algorithmName, string paramName, string groupName = "流程1");

        public object? GetAlgorithmResult(string algorithmName, string paramName, string groupName = "流程1");
        public T? GetAlgorithmResult<T>(string algorithmName, string paramName, string groupName = "流程1");


        public object? GetModule(string moduleName);

        //public K GetListResult<T, K>(string algorithmName, string paramName, string groupName = "流程1");

        //public Dictionary<string, object> GetResults(string algorithmName, string paramsName);

    }
}
