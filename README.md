# Wallpaper Manager

**Autor:** Elton Nike Casa  
**Data:** 26/08/2025  

---

## Descrição

O **Wallpaper Manager** é um software em C# que permite gerenciar automaticamente o papel de parede do Windows a partir de arquivos em uma pasta de rede.  
Ele verifica se há atualizações do executável e da imagem de papel de parede, e aplica a imagem no sistema automaticamente, de forma **silenciosa**, sem exibir janelas.

---

## Funcionalidades

- Executa em segundo plano (sem janela ou console).  
- Verifica se o parâmetro `ATIVO` está `ON` no arquivo de configuração antes de executar.  
- Atualiza automaticamente o executável (`EXECUTAVEL`) se houver versão mais recente na pasta de rede (`REDE`).  
- Copia a imagem de papel de parede (`WALL`) apenas se for mais nova ou inexistente localmente (`LOCAL`).  
- Aplica o papel de parede instantaneamente no Windows.  
- Mantém backup do executável antigo (`.bak`) ao atualizar.

---

## Estrutura do `config.cfg`

O arquivo de configuração deve estar na mesma pasta do executável. Exemplo com comentários:

```ini
# Caminho da pasta de rede onde ficam os arquivos de imagem e executável
REDE=\\localderede

# Caminho local onde a imagem será copiada e usada como papel de parede
LOCAL=C:\Imagen\ativo.png

# Controle principal: ON = ativo, OFF = desativa o software
ATIVO=ON

# Nome do arquivo de imagem dentro da pasta REDE
WALL=ativo.png

# Nome do executável que será atualizado se houver versão mais nova na REDE
EXECUTAVEL=WallpapperChanger.exe
