using VM.Core;

namespace VisionServices.Store
{
    public class ProcedureStore
    {
        private readonly Dictionary<string, VmProcedure> procedures = new();

        public bool TryGet(string procedureName, out VmProcedure? vmProcedure)
        {
            vmProcedure = null;

            if (VmSolution.Instance[procedureName] is VmProcedure p)
            {
                vmProcedure = p;
#if NET8_0_OR_GREATER
                return procedures.TryAdd(procedureName, p);
#elif NET48
                procedures.Add(procedureName, p);
                return true;
#endif
            }
            return false;
        }
    }
}
