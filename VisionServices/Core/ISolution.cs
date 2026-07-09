namespace VisionServices.Core
{
    public interface ISolution : IDisposable
    {
        public void Load(string solutionPath);

        public Task LoadAsync(string solutionPath);

        public string Save();

        public string SaveAs(string targetPath);

        public bool Run();

        public bool RunAsync();
        
        public bool SetModuParam<T>(string paramName, T value, string groupName);

        /// <summary>
        /// 获取指定模块的指定结果
        /// </summary>
        /// <typeparam name="T">指定的数据类型</typeparam>
        /// <param name="algorithmName">算子模块名称</param>
        /// <param name="paramName">指定的结果</param>
        /// <param name="groupName">流程名称</param>
        /// <returns></returns>
        public T? TryGetModuOutput<T>(string algorithmName, string paramName, string groupName);

        /// <summary>
        /// 获取指定的算子模块
        /// </summary>
        /// <param name="moduleName">算子模块名称</param>
        /// <param name="groupName">流程名称</param>
        /// <returns>指定的算子模块</returns>
        public object? TryGetModule(string moduleName, string groupName);

        /// <summary>
        /// 获得指定类型的算子模块
        /// </summary>
        /// <typeparam name="T">指定算子模块的类型</typeparam>
        /// <param name="moduleName">算子模块名称</param>
        /// <param name="groupName">流程名称</param>
        /// <returns>指定的算子模块</returns>
        public T? TryGetModule<T>(string moduleName, string groupName) where T : class;


    }
}
