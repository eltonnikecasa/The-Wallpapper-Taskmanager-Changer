/*
 * ---------------------------------------------------------
 * Software:   Wallpaper Manager
 * Criador:    Elton Nike Casa
 * Data:       26/08/2025
 *
 * Descrição:
 * Este software verifica um arquivo de configuração (config.cfg)
 * e, caso o parâmetro ATIVO esteja definido como ON:
 *   - Verifica se há versão mais nova do executável na pasta de rede
 *   - Copia a imagem de wallpaper se for mais nova que a local
 *   - Define automaticamente a imagem como papel de parede do Windows
 *
 * Observações:
 * - Executa em segundo plano, sem exibir janelas
 * - Deve ser compilado como "Windows Application"
 * - Mantém backup automático do executável em caso de atualização
 * ---------------------------------------------------------
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

class Program
{
    [STAThread]
    static void Main()
    {
        try
        {
            var cfg = LoadConfig("config.cfg");

            // Se não tiver ATIVO ou estiver OFF, fecha silenciosamente
            if (!cfg.ContainsKey("ATIVO") || !cfg["ATIVO"].Equals("ON", StringComparison.OrdinalIgnoreCase))
                return;

            // --- Verifica atualização do executável ---
            if (cfg.ContainsKey("REDE") && cfg.ContainsKey("EXECUTAVEL"))
            {
                CheckAndUpdate(cfg["REDE"], cfg["EXECUTAVEL"]);
            }

            // --- Verifica imagem mais nova ---
            if (cfg.ContainsKey("REDE") && cfg.ContainsKey("WALL") && cfg.ContainsKey("LOCAL"))
            {
                string sourceImg = Path.Combine(cfg["REDE"], cfg["WALL"]);
                string destImg = cfg["LOCAL"];
                CopyIfNewer(sourceImg, destImg);

                // Se a imagem existe localmente, aplica como wallpaper
                if (File.Exists(destImg))
                {
                    SetWallpaper(destImg);
                }
            }
        }
        catch
        {
            // ignora qualquer erro silenciosamente
        }
    }

    // -------- Atualização do executável --------
    static void CheckAndUpdate(string updatePath, string exeName)
    {
        try
        {
            string currentExe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, exeName);
            string updateFile = Path.Combine(updatePath, exeName);

            if (File.Exists(updateFile) && File.Exists(currentExe))
            {
                Version currentVersion = AssemblyName.GetAssemblyName(currentExe).Version;
                Version updateVersion = AssemblyName.GetAssemblyName(updateFile).Version;

                if (updateVersion > currentVersion)
                {
                    string backup = currentExe + ".bak";
                    File.Copy(currentExe, backup, true); // backup
                    File.Copy(updateFile, currentExe, true); // substitui
                }
            }
        }
        catch { }
    }

    // -------- Cópia apenas se for mais novo --------
    static void CopyIfNewer(string source, string dest)
    {
        try
        {
            if (!File.Exists(source))
                return;

            bool needCopy = false;

            if (!File.Exists(dest))
            {
                needCopy = true; // não existe local
            }
            else
            {
                FileInfo fSource = new FileInfo(source);
                FileInfo fDest = new FileInfo(dest);

                if (fSource.LastWriteTimeUtc > fDest.LastWriteTimeUtc ||
                    fSource.Length != fDest.Length)
                {
                    needCopy = true;
                }
            }

            if (needCopy)
                File.Copy(source, dest, true);
        }
        catch { }
    }

    // -------- Aplica papel de parede --------
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

    // -------- Leitura do cfg --------
    static Dictionary<string, string> LoadConfig(string path)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (!File.Exists(path))
            return dict;

        foreach (var line in File.ReadAllLines(path))
        {
            if (string.IsNullOrWhiteSpace(line) || line.Trim().StartsWith("#"))
                continue;

            var parts = line.Split(new char[] { '=' }, 2);
            if (parts.Length == 2)
            {
                dict[parts[0].Trim()] = parts[1].Trim();
            }
        }

        return dict;
    }
}
