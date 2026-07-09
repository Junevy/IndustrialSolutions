using System.Reflection;
using VisionServices.Core;
using VisionServices.Store;
using VM.Core;

namespace VisionServices.Services.VisionMaster
{
    public class VmService : ISolution, IGroupSolution
    {
        private readonly ProcedureStore store;
        private volatile int isLoaded = 0;

        public bool IsLoaded => isLoaded == 1;

        private VmSolution Solution => VmSolution.Instance;

        public VmService(ProcedureStore store)
        {
            this.store = store;
        }

        public void Load(string solutionPath)
        {
#if NET6_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(solutionPath);
#elif NET46_OR_GREATER
            if (solutionPath == null) throw new ArgumentNullException(nameof(solutionPath));
#endif
            VmSolution.Load(solutionPath);
            Interlocked.Exchange(ref isLoaded, 1);
        }

        public async Task LoadAsync(string solutionPath)
        {
            await Task.Run(() =>
            {
                Load(solutionPath);
            });
        }

        public bool Run()
        {
            if (Interlocked.CompareExchange(ref isLoaded, 1, 1) == 1)
            {
                Solution.SyncRun();
                return true;
            }
            return false;
        }

        public bool RunAsync()
        {
            if (Interlocked.CompareExchange(ref isLoaded, 1, 1) == 1)
            {
                Solution.Run();
                return true;
            }
            return false;
        }

        public string Save()
        {
            if (Interlocked.CompareExchange(ref isLoaded, 1, 1) == 1)
            {
                return VmSolution.Save();
            }
            return string.Empty;
        }

        public string SaveAs(string targetPath)
        {
            if (Interlocked.CompareExchange(ref isLoaded, 1, 1) == 1)
            {
                return VmSolution.SaveAs(targetPath);
            }
            return string.Empty;
        }

        public bool SetModuParam<T>(string paramName, T value, string groupName)
        {
            throw new NotImplementedException();
        }

        public object? TryGetModule(string moduleName, string groupName) => this.Solution[$"{groupName}.{moduleName}"];

        public T? TryGetModule<T>(string moduleName, string groupName) where T : class => this.Solution[$"{groupName}.{moduleName}"] as T;

        public T? TryGetModuOutput<T>(string algorithmName, string paramName, string groupName)
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

        public bool TryGetGroup(string groupName, out VmProcedure? procedure)
        {
            procedure = null;
#if NET6_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(groupName);
#elif NET46_OR_GREATER
            if (groupName == null) throw new ArgumentNullException(nameof(groupName));
#endif

            if (store.TryGet(groupName, out var p))
            {
                procedure = p;
                return true;
            }
            return false;
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
            else if (typeof(T) == typeof(int))
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

        public T? TryGetGroupOutput<T>(string paramName, string groupName)
        {
            if (string.IsNullOrEmpty(groupName) || string.IsNullOrEmpty(paramName))
                return default;

            if (TryGetGroup(groupName, out var group))
            {
                if (group is VmProcedure g)
                    return TryGetGroupOutput<T>(g, paramName);
            }
            return default;
        }

        public Dictionary<string, object> GetGroupOutputs(Dictionary<string, string> paramInfo, string groupName)
        {
            Dictionary<string, object> paramPairs = new();
            var names = paramInfo.Select(x => x.Key).ToList();

            foreach (var n in names)
            {
                var type = paramInfo[n];
                if (type == "string")
                    paramPairs[n] = TryGetGroupOutput<string>(n, groupName) ?? string.Empty;
                else if (type == "int")
                    paramPairs[n] = TryGetGroupOutput<int>(n, groupName);
                else if (type == "float")
                    paramPairs[n] = TryGetGroupOutput<float>(n, groupName);
            }
            return paramPairs;
        }

        public Dictionary<string, object> GetGroupOutputs(Dictionary<string, string> paramInfo, VmProcedure group)
        {
            Dictionary<string, object> paramPairs = new();
            foreach (var n in paramInfo.Keys)
            {
                //var type = paramInfo[n];
                if (paramInfo[n] == "string")
                    paramPairs[n] = TryGetGroupOutput<string>(group, n) ?? string.Empty;
                else if (paramInfo[n] == "int")
                    paramPairs[n] = TryGetGroupOutput<int>(group, n);
                else if (paramInfo[n] == "float")
                    paramPairs[n] = TryGetGroupOutput<float>(group, n);
            }
            return paramPairs;
        }

        public void Dispose()
        {
            if (Solution == null) return;
            Solution.Dispose();
            Interlocked.Exchange(ref isLoaded, 0);
        }
    }
}
