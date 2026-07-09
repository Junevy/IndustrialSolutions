using VM.Core;

namespace VisionServices.Core
{
    public interface IGroupSolution : IDisposable
    {
        public bool TryGetGroup(string groupName, out VmProcedure? group);

        public T? TryGetGroupOutput<T>(string paramName, string groupName);

        public T? TryGetGroupOutput<T>(VmProcedure procedure, string paramName);


        /// <summary>
        /// 接受一个字典类型的paramInfo
        /// </summary>
        /// <param name="paramInfo">参数信息，Key：参数名，Value：参数类型（字符串声明）</param>
        /// <param name="groupName">流程名</param>
        /// <returns></returns>
        public Dictionary<string, object> GetGroupOutputs(Dictionary<string, string> paramInfo, string groupName);


    }
}
