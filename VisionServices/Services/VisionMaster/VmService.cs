using System.Reflection;
using VisionServices.Core;
using VisionServices.Store;
using VM.Core;

namespace VisionServices.Services.VisionMaster
{
    public class VmService : ISolution, IGroupSolution
    {
        private readonly ProcedureStore store;

        private VmSolution Solution => VmSolution.Instance;

        public VmService(ProcedureStore store)
        {
            this.store = store;
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

        public void test()
        {
            //Solution.
        }

        public void Run() => Solution.SyncRun();

        public void RunAsync() => Solution.Run();

        public void Save() => VmSolution.Save();

        public void SaveAs(string targetPath) => VmSolution.SaveAs(targetPath);

        public bool SetModuParam<T>(string paramName, T value, string groupName = "流程1")
        {
            throw new NotImplementedException();
        }

        public object? TryGetModule(string moduleName, string groupName = "流程1") => this.Solution[$"{groupName}.{moduleName}"];

        public T? TryGetModule<T>(string moduleName, string groupName = "流程1") where T : class => this.Solution[$"{groupName}.{moduleName}"] as T;

        public T? TryGetModuOutput<T>(string algorithmName, string paramName, string groupName = "流程1")
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

        public object? TryGetGroup(string groupName = "流程1")
        {
            store.TryGet(groupName, out var p);
            return p;
        }

        public T? TryGetGroupOutput<T>(VmProcedure vp, string paramName)
        {
            if (vp == null) return default;

            if (typeof(T) == typeof(string))
            {
                var result = vp.ModuResult.GetOutputString(paramName);
                if (result.nValueNum > 0)
                    return (T)(object)result.astStringVal[0].strValue;
                return default;
            }

            if (typeof(T) == typeof(int))
            {
                var result = vp.ModuResult.GetOutputInt(paramName);
                if (result.nValueNum > 0)
                    return (T)(object)result.pIntVal[0];
                return default;
            }
            else if (typeof(T) == typeof(float))
            {
                var result = vp.ModuResult.GetOutputFloat(paramName);
                if (result.nValueNum > 0)
                    return (T)(object)result.pFloatVal[0];
                return default;
            }

            return default;
        }

        public T? TryGetGroupOutput<T>(string paramName, string groupName = "流程1")
        {
            if (string.IsNullOrEmpty(groupName) || string.IsNullOrEmpty(paramName))
                return default;

            VmProcedure vp = (VmProcedure)VmSolution.Instance[groupName];
            return TryGetGroupOutput<T>(vp, paramName);
        }

        public Dictionary<string, object> GetGroupOutputs(Dictionary<string, string> paramInfo, string groupName = "流程1")
        {
            Dictionary<string, object> paramPairs = new();

            var names = paramInfo.Select(x => x.Key).ToList();
            //if (names == null) return paramPairs;

            foreach (var n in names)
            {
                var type = paramInfo[n];
                if (type == "string")
                    paramPairs[n] = TryGetGroupOutput<string>(n) ?? string.Empty;
                else if (type == "int")
                    paramPairs[n] = TryGetGroupOutput<float>(n);
                else if (type == "float")
                    paramPairs[n] = TryGetGroupOutput<float>(n);
            }
            return paramPairs;
        }

        public void Dispose()
        {
            Solution.Dispose();
        }


    }
}
