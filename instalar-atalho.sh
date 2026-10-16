#!/usr/bin/env bash
#
# Instala o "Estude Inglês" no menu de aplicativos do sistema.
#
# O que ele faz:
#   1. compila a versão de produção (Release);
#   2. instala os arquivos em ~/.local/share/estude-ingles/app;
#   3. cria o inicializador ~/.local/bin/estude-ingles;
#   4. instala o ícone e o atalho no menu (~/.local/share/applications).
#
# Para atualizar depois de mexer no código, rode este script de novo.
#
set -euo pipefail

REPO_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
APP_DIR="$HOME/.local/share/estude-ingles/app"
BIN_DIR="$HOME/.local/bin"
DESKTOP_DIR="$HOME/.local/share/applications"
ICON_DIR="$HOME/.local/share/icons/hicolor/scalable/apps"
DOTNET="$(command -v dotnet || echo /usr/bin/dotnet)"

echo "▶ Compilando versão de produção…"
dotnet publish "$REPO_DIR/EnglishStudy.csproj" -c Release -o "$APP_DIR" --nologo

echo "▶ Criando o inicializador ($BIN_DIR/estude-ingles)…"
mkdir -p "$BIN_DIR"
cat > "$BIN_DIR/estude-ingles" <<EOF
#!/usr/bin/env bash
# Gerado por EnglishStudy/instalar-atalho.sh
APP_DIR="$APP_DIR"
if [ -x "\$APP_DIR/EnglishStudy" ]; then
    exec "\$APP_DIR/EnglishStudy" "\$@"
fi
exec "$DOTNET" "\$APP_DIR/EnglishStudy.dll" "\$@"
EOF
chmod +x "$BIN_DIR/estude-ingles"

echo "▶ Instalando o ícone…"
mkdir -p "$ICON_DIR"
cp "$REPO_DIR/assets/estude-ingles.svg" "$ICON_DIR/estude-ingles.svg"

echo "▶ Criando o atalho no menu…"
mkdir -p "$DESKTOP_DIR"
cat > "$DESKTOP_DIR/estude-ingles.desktop" <<EOF
[Desktop Entry]
Type=Application
Version=1.0
Name=Estude Inglês
GenericName=Aprender idiomas
Comment=Aprenda inglês com flashcards, lições, quiz de gramática e conversação com IA
Exec=$BIN_DIR/estude-ingles
Icon=estude-ingles
Terminal=false
Categories=Education;Languages;
Keywords=inglês;english;idiomas;flashcards;estudo;vocabulário;gramática;
StartupNotify=true
StartupWMClass=EnglishStudy
EOF
chmod +x "$DESKTOP_DIR/estude-ingles.desktop"

if command -v desktop-file-validate >/dev/null; then
    desktop-file-validate "$DESKTOP_DIR/estude-ingles.desktop" || true
fi
if command -v update-desktop-database >/dev/null; then
    update-desktop-database "$DESKTOP_DIR" >/dev/null 2>&1 || true
fi
if command -v gtk-update-icon-cache >/dev/null; then
    gtk-update-icon-cache -f -t "$HOME/.local/share/icons/hicolor" >/dev/null 2>&1 || true
fi

echo
echo "✅ Pronto! Procure por “Estude Inglês” no menu de aplicativos."
echo "   Pelo terminal também funciona: estude-ingles"
echo "   Seu progresso continua em: ~/.local/share/EnglishStudy/data.json"
