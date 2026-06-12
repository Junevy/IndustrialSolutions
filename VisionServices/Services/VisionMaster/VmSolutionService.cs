using System.Reflection;
using VisionServices.Core;
using VM.Core;

namespace VisionServices.Services.VisionMaster
{
    public class VmSolutionService : ISolution, IGroupSolution
    {
        private VmSolution Solution => VmSolution.Instance;

        public VmSolutionService()
        {
        }

        public void Load(string solutionPath)
        {
            if (solutionPath == null) throw new ArgumentNullException(nameof(solutionPath));
            VmSolution.Load(solutionPath);
        }

        public async Task LoadAsync(string solutionPath)
        {
            await Task.Run(() =>
            {
                Load(solutionPath);
            });
        }

        public void Run() => Solution.SyncRun();

        public void RunAsync() => Solution.Run();

        public void Save() => VmSolution.Save();

        public void SaveAs(string targetPath) => VmSolution.SaveAs(targetPath);

        public T? GetModuResult<T>(string algorithmName, string paramName, string groupName = "流程1")
        {

            if (string.IsNullOrEmpty(algorithmName)
                || string.IsNullOrEmpty(paramName)
                || string.IsNullOrEmpty(groupName))
                return default;

            var algorithm = Solution[$"{groupName}.{algorithmName}"];
            if (algorithm == null)
                return default;

            var result = GetNestedPropertyValue(algorithm, paramName);
            return result == null ? default : (T)result;

        }

        public T? GetGroupOutput<T>(string paramName, string groupName = "流程1")
        {
            if (string.IsNullOrEmpty(groupName) || string.IsNullOrEmpty(paramName))
                return default;

            VmProcedure vp = (VmProcedure)VmSolution.Instance[groupName];
            if (vp == null) return default;

            if (typeof(T) == typeof(string))
            {
                object ocrResult = vp.ModuResult.GetOutputString("result").astStringVal[0].strValue;
                return (T)ocrResult;
            }

            return default;
        }

        private object? GetNestedPropertyValue(object algorithm, string propertyPath)
        {
            if (algorithm == null) return null;
            Type currentType = algorithm.GetType();
            object? currentProperty = algorithm;

            foreach (string propName in propertyPath.Split('.'))
            {
                if (currentProperty == null) return null;

                PropertyInfo? prop = currentType.GetProperty(propName);
                if (prop == null) return null;

                currentProperty = prop?.GetValue(currentProperty);
                if (currentProperty == null) return null;

                currentType = currentProperty.GetType();
                if (currentType == null) return null;
            }
            return currentProperty;
        }

        public void Dispose()
        {
            Solution.Dispose();
        }

        public bool SetModuParam<T>(string paramName, T value, string groupName = "流程1")
        {
            throw new NotImplementedException();
        }

        public object? GetModule(string moduleName, string groupName = "流程1") => this.Solution[$"{groupName}.{moduleName}"];

        public T? GetModule<T>(string moduleName, string groupName = "流程1") where T : class => this.Solution[$"{groupName}.{moduleName}"] as T;

        //public void test()
        //{
        //    ImageSourceModuleCs.ImageSourceModuleTool
        //}

    }
}
