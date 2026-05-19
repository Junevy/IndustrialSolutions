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

        public void Run() => vmSolution.SyncRun();

        public void RunAsync() => vmSolution.Run();


        public void Save() => VmSolution.Save();

        public void SaveAs(string targetPath)
        {
            throw new NotImplementedException();
        }

        public bool SetParam<T>(string key, T value)
        {
            throw new NotImplementedException();
        }

        public T GetResult<T>(string algorithmName, string paramName)
        {
            throw new NotImplementedException();

        }

        public Dictionary<string, object> GetResults(string algorithmName, string paramsName)
        {
            throw new NotImplementedException();
        }

        public T GetGroupOutput<T>(string paramName, string groupName = "流程1")
        {
            if (string.IsNullOrEmpty(groupName))
                throw new ArgumentNullException(nameof(groupName) + "can not be null or empty");

            VmProcedure vp = (VmProcedure)VmSolution.Instance[groupName];
            if (vp == null) return default;

            return default;
        }

        public Dictionary<string, object> GetGroupOutputs(string paramName, string groupName = "流程1")
        {
            throw new NotImplementedException();
        }
    }
}
