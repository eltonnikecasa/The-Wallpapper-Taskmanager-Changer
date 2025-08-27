# Wallpaper Manager

**Criador:** Elton Nike Casa  
**Data de Criação:** 26/08/2025  
**Última Atualização:** 27/08/2025  

---

## Descrição

O **Wallpaper Manager** é um software que atualiza automaticamente o papel de parede do Windows a partir de uma pasta de rede. Ele também verifica e atualiza o próprio executável se houver uma versão mais recente disponível.  

O software funciona de forma **silenciosa** (sem abrir janelas) e mantém logs das operações, com histórico limitado a **7 dias**.  

O processo de atualização do executável é feito com um **updater temporário** para permitir substituir o arquivo principal enquanto ele está em execução.  

---

## Estrutura de Arquivos

```
WallpaperManager/
│
├─ WallpapperChanger.exe      # Executável principal
├─ Updater.exe                # Atualizador temporário
├─ config.cfg                 # Arquivo de configuração
└─ log.txt                    # Log das operações
```

---

## Arquivo de Configuração (`config.cfg`)

```ini
# -----------------------------------------------------------
# Arquivo de configuração do Wallpaper Manager
# Criador: Elton Nike Casa
# Data: 27/08/2025
# -----------------------------------------------------------

REDE=\\localdarede
ATIVO=ON
WALL=ativo.png
EXECUTAVEL=WallpapperChanger.exe
HASH_EXE=
HASH_WALL=
```

---

## Fluxo de Funcionamento

### Fluxograma ASCII

```
┌───────────────────────┐
│ Inicia WallpapperChanger.exe │
└───────────┬───────────┘
            │
            ▼
  ┌────────────────────┐
  │ Lê config.cfg       │
  └───────────┬────────┘
              │
              ▼
    ┌─────────────────┐
    │ ATIVO = ON?      │
    └───┬─────────────┘
        │Não
        ▼
    Encerra execução
        │Sim
        ▼
  ┌────────────────────────────┐
  │ Verifica hash do exe local  │
  │ com hash do exe remoto      │
  └───────────┬────────────────┘
              │Diferente
              ▼
   ┌─────────────────────────┐
   │ Cria temp exe (_new.exe)│
   │ Executa Updater.exe     │
   └───────────┬────────────┘
               │
               ▼
   Substitui exe principal
               │
               ▼
       Reinicia exe principal
               │
               ▼
  ┌───────────────────────────┐
  │ Verifica hash da imagem    │
  │ Local x Remota             │
  └───────────┬───────────────┘
              │Diferente
              ▼
   ┌─────────────────────────┐
   │ Copia nova imagem       │
   │ Aplica como wallpaper   │
   └───────────┬────────────┘
              │
              ▼
       Atualização concluída
              │
              ▼
           Fim do processo
```

---

## Instruções de Uso

1. Coloque os arquivos na pasta desejada:  
   - `WallpapperChanger.exe`  
   - `Updater.exe`  
   - `config.cfg`  

2. Edite `config.cfg` conforme necessário:  
   - `REDE` → caminho da pasta de rede  
   - `WALL` → nome do arquivo de imagem  
   - `EXECUTAVEL` → nome do executável principal  
   - `ATIVO` → ON/OFF  

3. Execute `WallpapperChanger.exe`. Ele funciona **silenciosamente**.  

4. Para execução automática:  
   - Configure uma **tarefa no Agendador de Tarefas** do Windows para rodar `WallpapperChanger.exe` ao logar usuários administradores.  

---

## Observações Técnicas

- Compilar ambos os executáveis como **Windows Application** para não abrir janelas do console.  
- `Updater.exe` deve estar na mesma pasta que o principal.  
- Logs são gravados em `log.txt` e limitado a 7 dias.  
- Verificações de hash usam **MD5** para identificar alterações em arquivos.  

---

## Licença

Este projeto é de uso pessoal e educativo. Modificações e distribuição são permitidas mediante citação do criador.
