namespace VisionPlatformServices.Core
{
    public interface ISolution
    {
        void Load(string path, string password = null);

        void Save();

        void SaveAs(string path, string password = null);

        void Run();

        void RunAsync();

    }
}
