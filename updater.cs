/*
 * ---------------------------------------------------------
 * Software:   Updater
 * Criador:    Elton Nike Casa
 * Data:       27/08/2025
 * ---------------------------------------------------------
 */

using System;
using System.Diagnostics;
using System.IO;

class Updater
{
    static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Uso: Updater.exe <caminho do exe principal> <caminho do exe novo>");
            return;
        }

        string exePath = args[0];
        string newExePath = args[1];

        try
        {
            // Espera processo principal encerrar
            Process[] running = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(exePath));
            foreach (var proc in running)
            {
                try { proc.WaitForExit(); } catch { }
            }

            // Substitui executável
            File.Copy(newExePath, exePath, true);

            // Remove arquivo temporário
            try { File.Delete(newExePath); } catch { }

            // Reinicia executável principal
            Process.Start(exePath);
        }
        catch (Exception ex)
        {
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "updater_log.txt");
            File.AppendAllText(logPath, DateTime.Now.ToString() + " - ERRO: " + ex.Message + "\n");
        }
    }
}
