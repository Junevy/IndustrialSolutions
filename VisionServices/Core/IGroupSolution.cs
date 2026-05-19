using VisionServices.Services.VisionMaster;

namespace VisionServices.Core
{
    public interface IGroupSolution
    {
        public T GetGroupOutput<T>(string paramName, string groupName = "流程1");

        //public Dictionary<string, object> GetGroupOutputs(string paramName, string groupName = "流程1");
    }
}
