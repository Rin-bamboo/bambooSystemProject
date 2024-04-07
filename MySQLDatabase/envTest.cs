namespace Database
{
    public class EnvFileReader
    {
        public static void LoadEnvFile(string path)
        {
            if (File.Exists(path))
            {
                string[] lines = File.ReadAllLines(path);
                foreach (string line in lines)
                {
                    string[] parts = line.Split('=', 2);
                    if (parts.Length == 2)
                    {
                        string key = parts[0].Trim();
                        string value = parts[1].Trim();
                        Environment.SetEnvironmentVariable(key, value);
                    }
                }
            }
            else
            {
                Console.WriteLine(".envファイルが見つかりません。");
            }
        }
    }
}
