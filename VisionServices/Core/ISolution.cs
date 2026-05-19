namespace VisionServices.Core
{
    public interface ISolution
    {
        public void Load(string solutionPath);

        public Task LoadAsync(string solutionPath);

        public void Save();

        public void SaveAs(string targetPath);

        public void Run();

        public void RunAsync();

        //public bool SetParam<T>(string key, T value);

        public object GetResult(string algorithmName, string paramName, string groupName = "流程1");
        public T GetResult<T>(string algorithmName, string paramName, string groupName = "流程1");

        //public K GetListResult<T, K>(string algorithmName, string paramName, string groupName = "流程1");

        //public Dictionary<string, object> GetResults(string algorithmName, string paramsName);

    }
}
