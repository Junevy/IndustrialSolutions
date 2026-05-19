namespace VisionServices.Core
{
    public interface ISolution
    {
        public void Load(string solutionPath);

        public void Save();

        public void SaveAs(string targetPath);

        public void Run();

        public void RunAsync();

        public bool SetParam<T>(string key, T value);

        public T GetResult<T>(string algorithmName, string paramName);

        public Dictionary<string, object> GetResults(string algorithmName, string paramsName);

    }
}
