# 📘 Estude Inglês

Aplicativo de desktop (Linux/Windows/macOS) para estudar inglês no dia a dia.
Foi feito para quem fala português: toda a interface está em português e o
conteúdo é voltado para brasileiros.

## O que dá para fazer

| Modo | O que é |
| --- | --- |
| 🃏 **Flashcards** | Repetição espaçada (algoritmo SM-2, o mesmo estilo do Anki). Você vê a palavra em português, tenta lembrar em inglês e avalia: Errei / Difícil / Bom / Fácil. |
| 📚 **Lições** | Exercícios estilo Duolingo: escolher a tradução, completar frases e montar frases tocando nas palavras. Você tem 3 vidas por lição. |
| ❓ **Gramática** | Quiz com 48 perguntas em 8 temas (presente, passado, futuro, verbos irregulares, preposições, artigos, comparativos e perguntas), sempre com explicação em português. |
| 💬 **Conversar** | Bate-papo com a **Mia**, uma professora de inglês com IA que responde em inglês simples e corrige seus erros. |
| 📖 **Meu Vocabulário** | Banco embutido com **105 palavras e frases** (com exemplo em inglês e tradução) e espaço para você adicionar as suas. |
| ⚙️ **Configurações** | Seu nome, quantas palavras novas por dia, endereço da IA e opção de apagar o progresso. |

Também tem **XP, níveis e sequência diária (streak 🔥)** para te manter firme.

## Pré-requisitos

- **.NET 10** (SDK ou runtime). Para conferir: `dotnet --version`
- Linux, Windows ou macOS.

## Como rodar

Na pasta do projeto:

```bash
dotnet run
```

O aplicativo abre com o banco de palavras já preenchido — não precisa configurar nada
para começar a usar os flashcards, as lições e a gramática.

### Conversar com a IA (opcional)

O modo **Conversar** usa um servidor local compatível com a API da OpenAI.
A forma mais fácil no Linux é o **LM Studio**:

1. Abra o LM Studio e baixe/model um modelo (por exemplo um Llama ou Qwen pequeno).
2. Vá em **Developer** (ícone `</>`) e clique em **Start Server**.
3. O servidor fica em `http://localhost:1234/v1` — que já é o endereço padrão do app.
4. No app, abra **Configurações → Conversação com IA** e clique em **Testar conexão**.
   Se aparecer "✅ Conectado!", o nome do modelo é preenchido automaticamente.

Qualquer outro servidor compatível com OpenAI também funciona (llama.cpp server,
Ollama com porta compatível, etc.) — basta trocar o endereço nas Configurações.

### Ouvir a pronúncia (opcional)

Se o programa `espeak-ng` estiver instalado, o botão 🔊 aparece nos flashcards e a
palavra é falada.

```bash
sudo apt install espeak-ng      # Debian/Ubuntu
sudo dnf install espeak-ng      # Fedora
```

Sem o `espeak-ng` o app funciona normalmente, só não mostra o botão de áudio.

## Onde ficam seus dados

Tudo é salvo em um único arquivo JSON:

```
~/.local/share/EnglishStudy/data.json
```

Para fazer backup, basta copiar esse arquivo. Para começar do zero, use
**Configurações → Apagar progresso** (suas palavras próprias são mantidas).

## Testes

```bash
dotnet test tests/EnglishStudy.Tests/EnglishStudy.Tests.csproj
```

Os testes cobrem o algoritmo de repetição espaçada, a gamificação, o gerador de
lições, o conteúdo embutido, a persistência — e ainda **renderizam cada tela**
(Avalonia Headless + Skia) para garantir que nenhuma tela quebra ao abrir.
As imagens geradas saem em `/tmp/opencode/page-*.png`.

## Estrutura do projeto

```
EnglishStudy.csproj
Program.cs            # ponto de entrada
App.axaml(.cs)        # tema, cores e mapeamento tela ↔ view model
Models/               # dados (palavras, cartões SRS, perguntas, perfil, chat)
Services/             # armazenamento, SRS, gamificação, IA, fala, conteúdo
ViewModels/           # lógica das telas
Views/                # telas em XAML
tests/EnglishStudy.Tests/   # testes (xunit)
```

## Como funciona a repetição espaçada

- Cada palavra nova vira um cartão.
- Ao responder **Bom**, o intervalo até a próxima revisão cresce (2 dias, depois
  ~5, ~12, ...). **Fácil** cresce mais rápido, **Difícil** mais devagar e **Errei**
  zera o intervalo (a palavra volta na mesma sessão).
- Cada cartão tem uma "facilidade" própria, que sobe ou desce conforme suas respostas.
- O intervalo máximo é de 1 ano e a facilidade nunca fica abaixo de 1,3.
- Por dia, entram no máximo **15 palavras novas** (ajustável nas Configurações) —
  o resto da sessão são revisões de coisas que você já viu.
