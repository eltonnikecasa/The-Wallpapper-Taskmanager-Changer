/*
 * ---------------------------------------------------------
 * Software:   Wallpaper Manager
 * Criador:    Elton Nike Casa ENC
 * Data:       27/08/2025
 * ---------------------------------------------------------
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

class Program
{
    [STAThread]
    static void Main()
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string cfgPath = Path.Combine(baseDir, "config.cfg");
        string logPath = Path.Combine(baseDir, "log.txt");

        try
        {
            CleanLog(logPath, 7);
            File.AppendAllText(logPath, DateTime.Now.ToString() + " - Iniciando Wallpaper Manager\n");

            var cfg = LoadConfig(cfgPath);

            if (!cfg.ContainsKey("ATIVO") || !cfg["ATIVO"].Equals("ON", StringComparison.OrdinalIgnoreCase))
            {
                File.AppendAllText(logPath, DateTime.Now.ToString() + " - ATIVO=OFF, encerrando\n");
                return;
            }

            // ------------------- Executável -------------------
            if (cfg.ContainsKey("REDE") && cfg.ContainsKey("EXECUTAVEL"))
            {
                string exeName = cfg["EXECUTAVEL"];
                string localExe = Path.Combine(baseDir, exeName);
                string remoteExe = Path.Combine(cfg["REDE"], exeName);

                if (File.Exists(remoteExe))
                {
                    string remoteHash = ComputeHash(remoteExe);
                    string localHash = File.Exists(localExe) ? ComputeHash(localExe) : "";

                    if (localHash != remoteHash)
                    {
                        // Cria arquivo temporário para atualização
                        string tempExe = Path.Combine(baseDir, exeName + "_new.exe");
                        File.Copy(remoteExe, tempExe, true);

                        // Inicia updater
                        string updaterPath = Path.Combine(baseDir, "Updater.exe");
                        if (File.Exists(updaterPath))
                        {
                            Process.Start(updaterPath, "\"" + localExe + "\" \"" + tempExe + "\"");
                            File.AppendAllText(logPath, DateTime.Now.ToString() + " - Iniciando updater para substituir executável\n");
                            return; // encerra o exe principal
                        }
                        else
                        {
                            File.AppendAllText(logPath, DateTime.Now.ToString() + " - Updater.exe não encontrado, não foi possível atualizar\n");
                        }
                    }
                    else
                    {
                        File.AppendAllText(logPath, DateTime.Now.ToString() + " - Executável já atualizado.\n");
                    }

                    cfg["HASH_EXE"] = remoteHash;
                    SaveConfig(cfgPath, cfg);
                }
                else
                {
                    File.AppendAllText(logPath, DateTime.Now.ToString() + " - Executável remoto não encontrado, teste ignorado.\n");
                }
            }

            // ------------------- Imagem -------------------
            if (cfg.ContainsKey("REDE") && cfg.ContainsKey("WALL"))
            {
                string wallName = cfg["WALL"];
                string localWall = Path.Combine(baseDir, wallName);
                string remoteWall = Path.Combine(cfg["REDE"], wallName);

                bool updated = false;

                if (File.Exists(remoteWall))
                {
                    string remoteHash = ComputeHash(remoteWall);
                    string localHash = File.Exists(localWall) ? ComputeHash(localWall) : "";

                    if (localHash != remoteHash)
                    {
                        File.Copy(remoteWall, localWall, true);
                        File.AppendAllText(logPath, DateTime.Now.ToString() + " - Imagem atualizada: " + wallName + "\n");
                        updated = true;
                    }
                    else
                    {
                        File.AppendAllText(logPath, DateTime.Now.ToString() + " - Imagem já atualizada.\n");
                    }

                    cfg["HASH_WALL"] = remoteHash;
                    SaveConfig(cfgPath, cfg);
                }
                else
                {
                    File.AppendAllText(logPath, DateTime.Now.ToString() + " - Imagem remota não encontrada, usando cópia local.\n");
                }

                // Sempre aplica a versão local, mesmo que não tenha conseguido baixar
                if (File.Exists(localWall))
                {
                    SetWallpaper(localWall);
                    File.AppendAllText(logPath, DateTime.Now.ToString() + " - Wallpaper aplicado (local" + (updated ? " atualizado" : "") + ")\n");
                }
                else
                {
                    File.AppendAllText(logPath, DateTime.Now.ToString() + " - Nenhuma cópia local de wallpaper disponível.\n");
                }
            }

            File.AppendAllText(logPath, DateTime.Now.ToString() + " - Finalizando execução\n");
        }
        catch (Exception ex)
        {
            File.AppendAllText(logPath, DateTime.Now.ToString() + " - ERRO: " + ex.Message + "\n");
        }
    }

    // -------- Limpa log mantendo apenas os últimos 'dias' --------
    static void CleanLog(string path, int dias)
    {
        try
        {
            if (!File.Exists(path)) return;
            var lines = File.ReadAllLines(path);
            var kept = new List<string>();
            DateTime cutoff = DateTime.Now.AddDays(-dias);

            foreach (var line in lines)
            {
                int idx = line.IndexOf(" - ");
                if (idx > 0)
                {
                    DateTime dt;
                    if (DateTime.TryParse(line.Substring(0, idx), out dt))
                    {
                        if (dt >= cutoff) kept.Add(line);
                        continue;
                    }
                }
                kept.Add(line);
            }

            File.WriteAllLines(path, kept.ToArray());
        }
        catch { }
    }

    // -------- Calcula hash MD5 de um arquivo --------
    static string ComputeHash(string path)
    {
        try
        {
            using (var md5 = MD5.Create())
            using (var stream = File.OpenRead(path))
            {
                var hash = md5.ComputeHash(stream);
                var sb = new StringBuilder();
                foreach (byte b in hash) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
        catch { return ""; }
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

    // -------- Leitura do cfg --------
    static Dictionary<string, string> LoadConfig(string path)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (!File.Exists(path)) return dict;

        foreach (var line in File.ReadAllLines(path))
        {
            if (string.IsNullOrWhiteSpace(line) || line.Trim().StartsWith("#")) continue;
            var parts = line.Split(new char[] { '=' }, 2);
            if (parts.Length == 2) dict[parts[0].Trim()] = parts[1].Trim();
        }
        return dict;
    }

    // -------- Salva cfg atualizado --------
    static void SaveConfig(string path, Dictionary<string, string> cfg)
    {
        var lines = new List<string>();
        foreach (var kv in cfg) lines.Add(kv.Key + "=" + kv.Value);
        File.WriteAllLines(path, lines);
    }
}
