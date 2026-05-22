using System.Reflection;
using VisionServices.Core;
using VM.Core;

namespace VisionServices.Services.VisionMaster
{
    public class VmSolutionService : ISolution, IGroupSolution, IDisposable
    {
        private VmSolution vmSolution;

        public VmSolutionService(VmSolution vmSolution)
        {
            if (vmSolution == null) throw new ArgumentNullException(nameof(vmSolution));
            this.vmSolution = vmSolution;
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

        public void Run() => vmSolution.SyncRun();

        public void RunAsync() => vmSolution.Run();

        public void Save() => VmSolution.Save();

        public void SaveAs(string targetPath)
        {
            throw new NotImplementedException();
        }

        public object? GetModuResult(string algorithmName, string paramName, string groupName = "流程1")
        {
            if (string.IsNullOrEmpty(paramName))
                return default;
            return GetAlgorithmResult(algorithmName, "ModuResult." + paramName, groupName);
        }

        public T? GetModuResult<T>(string algorithmName, string paramName, string groupName = "流程1")
        {
            if (string.IsNullOrEmpty(paramName))
                return default;
            return GetAlgorithmResult<T>(algorithmName, "ModuResult." + paramName, groupName);
        }

        public object? GetAlgorithmResult(string algorithmName, string paramName, string groupName = "流程1")
            => GetAlgorithmResult<object>(algorithmName, paramName, groupName);

        public T? GetAlgorithmResult<T>(string algorithmName, string paramName, string groupName = "流程1")
        {
            if (string.IsNullOrEmpty(algorithmName)
                || string.IsNullOrEmpty(paramName)
                || string.IsNullOrEmpty(groupName))
                return default;

            var algorithm = vmSolution[groupName + "." + algorithmName];
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
            vmSolution.Dispose();
        }
    }
}
