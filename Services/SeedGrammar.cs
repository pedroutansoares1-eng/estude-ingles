using EnglishStudy.Models;

namespace EnglishStudy.Services;

/// <summary>Banco de questões de gramática que acompanha o app.</summary>
public static class SeedGrammar
{
    public static List<GrammarQuestion> All()
    {
        var list = new List<GrammarQuestion>();
        var id = 0;

        void Q(string topic, string prompt, string[] options, int answer, string explanation) =>
            list.Add(new GrammarQuestion
            {
                Id = ++id,
                Topic = topic,
                Prompt = prompt,
                Options = options,
                AnswerIndex = answer,
                Explanation = explanation,
            });

        // Presente Simples
        Q("present", "She ____ to school every day.", new[] { "go", "goes", "going", "went" }, 1,
            "Com he/she/it, adicionamos -s ao verbo: she goes.");
        Q("present", "They ____ soccer on Sundays.", new[] { "plays", "play", "playing", "played" }, 1,
            "Com they, o verbo fica na forma base: they play.");
        Q("present", "I ____ coffee every morning.", new[] { "drinks", "drink", "drinking", "drank" }, 1,
            "Com I, usamos a forma base: I drink.");
        Q("present", "____ he like pizza?", new[] { "Do", "Does", "Is", "Are" }, 1,
            "Perguntas no presente com he/she/it usam Does.");
        Q("present", "My brother ____ TV at night.", new[] { "watch", "watches", "watching", "watched" }, 1,
            "Verbos terminados em -ch recebem -es: watches.");
        Q("present", "We ____ English on Mondays.", new[] { "studies", "study", "studying", "studied" }, 1,
            "Com we, o verbo fica na forma base: study.");

        // Passado Simples
        Q("past", "I ____ to the beach last week.", new[] { "go", "goes", "went", "going" }, 2,
            "Last week indica passado. O passado de go é went.");
        Q("past", "She ____ a new phone yesterday.", new[] { "buy", "buys", "bought", "buying" }, 2,
            "Yesterday indica passado. Buy vira bought.");
        Q("past", "They ____ at home last night.", new[] { "was", "were", "are", "is" }, 1,
            "No passado, they usa were.");
        Q("past", "He ____ the movie last Friday.", new[] { "watch", "watches", "watched", "watching" }, 2,
            "Verbo regular no passado recebe -ed: watched.");
        Q("past", "We didn't ____ to the party.", new[] { "went", "go", "going", "gone" }, 1,
            "Depois de didn't, o verbo fica na forma base: go.");
        Q("past", "I ____ very tired yesterday.", new[] { "is", "was", "were", "are" }, 1,
            "No passado, I usa was.");

        // Futuro
        Q("future", "I ____ call you tomorrow.", new[] { "will", "am", "was", "did" }, 0,
            "Usamos will para o futuro: I will call you.");
        Q("future", "She ____ going to travel next month.", new[] { "am", "is", "are", "be" }, 1,
            "'Going to' com she usa is: She is going to travel.");
        Q("future", "They ____ visit us next weekend.", new[] { "will", "was", "were", "did" }, 0,
            "Will serve para todos os sujeitos: they will visit.");
        Q("future", "What ____ you do next year?", new[] { "will", "do", "did", "are" }, 0,
            "Pergunta sobre o futuro: What will you do?");
        Q("future", "We are ____ to watch a movie tonight.", new[] { "go", "going", "went", "gone" }, 1,
            "'Are going to' indica futuro planejado.");
        Q("future", "It ____ rain later today.", new[] { "will", "was", "did", "does" }, 0,
            "Previsões usam will: It will rain.");

        // Verbos Irregulares
        Q("irregular", "I have ____ that movie three times.", new[] { "see", "saw", "seen", "seeing" }, 2,
            "Depois de have, usamos o particípio: seen.");
        Q("irregular", "She ____ her keys yesterday.", new[] { "lose", "loses", "lost", "losing" }, 2,
            "O passado de lose é lost.");
        Q("irregular", "They have ____ a new car.", new[] { "buy", "bought", "buying", "buys" }, 1,
            "Particípio de buy é bought.");
        Q("irregular", "I ____ the answer last night.", new[] { "know", "knew", "known", "knowing" }, 1,
            "Passado de know é knew.");
        Q("irregular", "We have ____ our homework.", new[] { "do", "did", "doing", "done" }, 3,
            "Particípio de do é done.");
        Q("irregular", "She ____ a beautiful song at the party.", new[] { "sing", "sang", "sung", "singing" }, 1,
            "Passado de sing é sang.");

        // Preposições
        Q("prepositions", "I wake up ____ 7 o'clock.", new[] { "in", "on", "at", "to" }, 2,
            "Usamos at com horas: at 7 o'clock.");
        Q("prepositions", "My birthday is ____ July.", new[] { "in", "on", "at", "to" }, 0,
            "Usamos in com meses: in July.");
        Q("prepositions", "The meeting is ____ Monday.", new[] { "in", "on", "at", "to" }, 1,
            "Usamos on com dias da semana: on Monday.");
        Q("prepositions", "She is interested ____ music.", new[] { "in", "on", "at", "for" }, 0,
            "interested in = interessado em.");
        Q("prepositions", "I go ____ work by bus.", new[] { "at", "to", "in", "on" }, 1,
            "Vamos TO work (indica direção).");
        Q("prepositions", "He is good ____ math.", new[] { "in", "on", "at", "for" }, 2,
            "good at = bom em (habilidade).");

        // Artigos
        Q("articles", "I need ____ umbrella.", new[] { "a", "an", "the", "-" }, 1,
            "Antes de som de vogal usamos an: an umbrella.");
        Q("articles", "She is ____ teacher.", new[] { "a", "an", "the", "-" }, 0,
            "Antes de som de consoante usamos a: a teacher.");
        Q("articles", "____ sun is very hot today.", new[] { "A", "An", "The", "Some" }, 2,
            "Usamos the para algo único e específico: the sun.");
        Q("articles", "I like ____ dogs.", new[] { "a", "an", "the", "-" }, 3,
            "Não usamos artigo ao falar de coisas em geral: I like dogs.");
        Q("articles", "He bought ____ new car.", new[] { "a", "an", "the", "-" }, 0,
            "Primeira menção com som de consoante: a new car.");
        Q("articles", "Can you open ____ window, please?", new[] { "a", "an", "the", "-" }, 2,
            "Janela específica: the window.");

        // Comparativos
        Q("comparatives", "My brother is ____ than me.", new[] { "tall", "taller", "tallest", "more tall" }, 1,
            "Comparativo de adjetivo curto: taller.");
        Q("comparatives", "This book is ____ than that one.", new[] { "interesting", "more interesting", "most interesting", "interested" }, 1,
            "Adjetivos longos usam more: more interesting.");
        Q("comparatives", "She is the ____ student in the class.", new[] { "smart", "smarter", "smartest", "more smart" }, 2,
            "Superlativo de adjetivo curto: smartest.");
        Q("comparatives", "Today is ____ than yesterday.", new[] { "hot", "hotter", "hottest", "more hot" }, 1,
            "Consoante + vogal + consoante dobra a letra: hotter.");
        Q("comparatives", "This is the ____ movie I have ever seen.", new[] { "bad", "worse", "worst", "more bad" }, 2,
            "Superlativo irregular de bad: worst.");
        Q("comparatives", "My phone is ____ than yours.", new[] { "good", "better", "best", "more good" }, 1,
            "Comparativo irregular de good: better.");

        // Perguntas
        Q("questions", "____ is your name?", new[] { "What", "Who", "Where", "When" }, 0,
            "Pergunta sobre o nome: What is your name?");
        Q("questions", "____ do you live?", new[] { "What", "Who", "Where", "When" }, 2,
            "Onde você mora? Where do you live?");
        Q("questions", "____ old are you?", new[] { "How", "What", "Who", "Where" }, 0,
            "How old are you? (pergunta de idade).");
        Q("questions", "____ is that man?", new[] { "What", "Who", "Where", "How" }, 1,
            "Pergunta sobre pessoa: Who is that man?");
        Q("questions", "____ do you wake up?", new[] { "What time", "Who", "Whose", "Which" }, 0,
            "Hora: What time do you wake up?");
        Q("questions", "____ are you sad?", new[] { "Why", "What", "Who", "Where" }, 0,
            "Motivo: Why are you sad?");

        return list;
    }
}
