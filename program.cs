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
 * - Mantém backup automático do executável ao atualizar
 * - Pode ser executado como Console Application sem mostrar a tela
 * ---------------------------------------------------------
 */

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
        // Esconde a janela do console
        var handle = GetConsoleWindow();
        ShowWindow(handle, SW_HIDE);

        try
        {
            var cfg = LoadConfig("config.cfg");

            // Se ATIVO não estiver ON, fecha silenciosamente
            if (!cfg.ContainsKey("ATIVO") || !cfg["ATIVO"].Equals("ON", StringComparison.OrdinalIgnoreCase))
                return;

            // Atualiza
