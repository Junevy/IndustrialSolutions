using System;
using System.Threading;
using VisionPlatformServices.Core;
using VM.Core;

namespace VisionPlatformServices.VisionMaster
{
    public class VmSol : ISolution, IDisposable
    {
        private readonly VmSolution vmSolution = VmSolution.Instance;
        private int isLoaded = 0;

        public void Load(string path, string password = null)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Path cannot be null or empty", nameof(path));

            if (Interlocked.CompareExchange(ref isLoaded, 1, 0) == 0)
            {
                VmSolution.Load(path, password);
            }
        }

        public void Run()
        {
            CheckIsLoaded();
            vmSolution.SyncRun();
        }

        public void RunAsync()
        {
            CheckIsLoaded();
            vmSolution.Run();
        }

        public void Save()
        {
            CheckIsLoaded();
            VmSolution.Save();
        }

        public void SaveAs(string path, string password = null)
        {
            CheckIsLoaded();
            VmSolution.SaveAs(path, password);
        }

        internal VmSolution GetSolutionInstance()
        {
            CheckIsLoaded();
            return vmSolution;
        }

        private void CheckIsLoaded()
        {
            if (isLoaded != 1)
                throw new System.InvalidOperationException("Solution must be loaded before running.");
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref isLoaded, 0) == 1)
            {
                vmSolution.Dispose();
            }
        }
    }
}
