using System.Reflection;
using VisionServices.Core;
using VM.Core;

namespace VisionServices.Services.VisionMaster
{
    public class VmSolutionService : ISolution, IGroupSolution
    {
        private VmSolution vmSolution;

        public VmSolutionService(VmSolution vmSolution)
        {
            ArgumentNullException.ThrowIfNull(vmSolution);
            this.vmSolution = vmSolution;
        }

        public void Load(string solutionPath)
        {
            ArgumentNullException.ThrowIfNull(solutionPath);
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

        public object GetResult(string algorithmName, string paramName, string groupName = "流程1")
            => GetResult<object>(algorithmName, paramName, groupName);

        public T GetResult<T>(string algorithmName, string paramName, string groupName = "流程1")
        {
            ArgumentNullException.ThrowIfNullOrEmpty(algorithmName);
            ArgumentNullException.ThrowIfNullOrEmpty(paramName);
            ArgumentNullException.ThrowIfNullOrEmpty(groupName);

            //var algorithm = (IMVSOcrDlModuCs.IMVSOcrDlModuTool)VmSolution.Instance[groupName + "." + algorithmName];
            //var algorithm = (IMVSVerticalLineFindModuCs.IMVSVerticalLineFindModuTool)VmSolution.Instance[groupName + "." + algorithmName];
            //var algorithm = (ImageSourceModuleCs.ImageSourceModuleTool)VmSolution.Instance[groupName + "." + algorithmName];
            //algorithm.ModuResult.ImageData.Width;
            //var tes = algorithm.ModuResult;
            //algorithm.ModuResult
            //algorithm.ModuResult.CharNum

            //T algorithm = (T)VmSolution.Instance[groupName + "." + algorithmName];
            //ArgumentNullException.ThrowIfNull(algorithm);

            //// 获取算子类型
            //Type type = algorithm.GetType();
            //ArgumentNullException.ThrowIfNull(type);

            var algorithm = VmSolution.Instance[groupName + "." + algorithmName];
            ArgumentNullException.ThrowIfNull(algorithm);

            // 获取算子类型
            //Type type = algorithm.GetType();
            //ArgumentNullException.ThrowIfNull(type);

            var teset = GetNestedPropertyValue(algorithm, paramName);

            // 获取算子内部的ModuResult属性
            //PropertyInfo property = type.GetProperty("ModuResult");
            //ArgumentNullException.ThrowIfNull(property);
            //var moduResult = property.GetValue(algorithm);

            //// 获取ModuResult属性中的paramName
            //ArgumentNullException.ThrowIfNull(moduResult);
            //var dataValue = moduResult.GetType().GetProperty(paramName)?.GetValue(moduResult);

            return (T)teset;
        }

        public T GetGroupOutput<T>(string paramName, string groupName = "流程1")
        {
            if (string.IsNullOrEmpty(groupName))
                throw new ArgumentNullException(nameof(groupName) + "can not be null or empty");

            VmProcedure vp = (VmProcedure)VmSolution.Instance[groupName];
            if (vp == null) return default;

            if (typeof(T) == typeof(string))
            {
                object ocrResult = vp.ModuResult.GetOutputString("result").astStringVal[0].strValue;
                return (T)ocrResult;
            }

            return default;
        }


        public object GetNestedPropertyValue(object obj, string propertyPath)
        {
            if (obj == null) return null;
            Type currentType = obj.GetType();
            object currentObj = obj;

            foreach (string propName in propertyPath.Split('.'))
            {
                if (currentObj == null) return null;
                PropertyInfo prop = currentType.GetProperty(propName);
                if (prop == null) return null;
                currentObj = prop.GetValue(currentObj);
                currentType = currentObj?.GetType();
            }
            return currentObj;
        }


    }
}
