using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

class Program
{
    [DllImport("kernel32.dll")]
    static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    const int SW_HIDE = 0;

    [STAThread]
    static void Main()
    {
        // Oculta o console imediatamente
        var handle = GetConsoleWindow();
        ShowWindow(handle, SW_HIDE);

        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string cfgPath = Path.Combine(baseDir, "config.cfg");

        try
        {
            var cfg = LoadConfig(cfgPath);

            // Executa apenas se ATIVO=ON
            if (!cfg.ContainsKey("ATIVO") || !cfg["ATIVO"].Equals("ON", StringComparison.OrdinalIgnoreCase))
                return;

            // Auto-update do executável
            if (cfg.ContainsKey("REDE") && cfg.ContainsKey("EXECUTAVEL"))
                CheckAndUpdate(cfg["REDE"], cfg["EXECUTAVEL"]);

            // Atualiza a imagem apenas se for mais nova
            if (cfg.ContainsKey("REDE") && cfg.ContainsKey("WALL") && cfg.ContainsKey("LOCAL"))
            {
                string sourceImg = Path.Combine(cfg["REDE"], cfg["WALL"]);
                string destImg = cfg["LOCAL"];
                CopyIfNewer(sourceImg, destImg);

                if (File.Exists(destImg))
                    SetWallpaper(destImg);
            }
        }
        catch
        {
            // Ignora erros silenciosamente
        }
    }

    static void CheckAndUpdate(string updatePath, string exeName)
    {
        try
        {
            if (!Directory.Exists(updatePath))
                return;

            string currentExe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, exeName);
            string updateFile = Path.Combine(updatePath, exeName);

            if (File.Exists(updateFile) && File.Exists(currentExe))
            {
                Version currentVersion = AssemblyName.GetAssemblyName(currentExe).Version;
                Version updateVersion = AssemblyName.GetAssemblyName(updateFile).Version;

                if (updateVersion > currentVersion)
                {
                    string backup = currentExe + ".bak";
                    File.Copy(currentExe, backup, true);
                    File.Copy(updateFile, currentExe, true);
                }
            }
        }
        catch { }
    }

    static void CopyIfNewer(string source, string dest)
    {
        try
        {
            if (!File.Exists(source))
                return;

            bool needCopy = !File.Exists(dest) ||
                new FileInfo(source).LastWriteTimeUtc > new FileInfo(dest).LastWriteTimeUtc ||
                new FileInfo(source).Length != new FileInfo(dest).Length;

            if (needCopy)
                File.Copy(source, dest, true);
        }
        catch { }
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

    private const int SPI_SETDESKWALLPAPER = 20;
    private const int SPIF_UPDATEINIFILE = 0x01;
    private const int SPIF_SENDCHANGE = 0x02;

    static void SetWallpaper(string path)
    {
        try
        {
            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, path, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
        }
        catch { }
    }

    static Dictionary<string, string> LoadConfig(string path)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (!File.Exists(path)) return dict;

        foreach (var line in File.ReadAllLines(path))
        {
            if (string.IsNullOrWhiteSpace(line) || line.Trim().StartsWith("#"))
                continue;

            var parts = line.Split(new char[] { '=' }, 2);
            if (parts.Length == 2)
                dict[parts[0].Trim()] = parts[1].Trim();
        }

        return dict;
    }
}
