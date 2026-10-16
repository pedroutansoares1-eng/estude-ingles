using EnglishStudy.Models;

namespace EnglishStudy.Services;

/// <summary>Monta lições no estilo Duolingo a partir do banco de palavras.</summary>
public sealed class LessonGenerator
{
    private readonly Random _rng;

    public LessonGenerator(Random rng) => _rng = rng;

    /// <summary>Gera uma sequência de exercícios. categoryKey nulo mistura todas as categorias.</summary>
    public List<ExerciseBase> BuildLesson(IReadOnlyList<WordEntry> words, string? categoryKey, int count = 6)
    {
        var pool = words.Where(w => categoryKey is null || w.Category == categoryKey).ToList();
        if (pool.Count < 4)
            pool = words.ToList();

        var picks = pool.OrderBy(_ => _rng.Next()).Take(count).ToList();
        var kinds = new[] { 0, 1, 2, 3, 0, 2, 1, 3 }; // alterna os tipos de exercício

        var lesson = new List<ExerciseBase>(picks.Count);
        for (var i = 0; i < picks.Count; i++)
            lesson.Add(BuildOne(words, picks[i], kinds[i % kinds.Length]));
        return lesson;
    }

    private ExerciseBase BuildOne(IReadOnlyList<WordEntry> all, WordEntry target, int kind)
    {
        return kind switch
        {
            2 when HasFillableExample(target) => BuildFillBlank(all, target),
            3 when !string.IsNullOrWhiteSpace(target.ExampleEn) => BuildScramble(target),
            1 => BuildReverseChoice(all, target),
            _ => BuildChoice(all, target),
        };
    }

    /// <summary>Escolha múltipla: português → inglês.</summary>
    private ChoiceExercise BuildChoice(IReadOnlyList<WordEntry> all, WordEntry target)
    {
        var options = Options(all, target, w => w.En, target.En);
        return new ChoiceExercise
        {
            Instruction = "Como se diz em inglês?",
            Prompt = target.Pt,
            Options = options,
            AnswerIndex = options.IndexOf(target.En),
            Feedback = $"{target.En} = {target.Pt}",
        };
    }

    /// <summary>Escolha múltipla: inglês → português.</summary>
    private ChoiceExercise BuildReverseChoice(IReadOnlyList<WordEntry> all, WordEntry target)
    {
        var options = Options(all, target, w => w.Pt, target.Pt);
        return new ChoiceExercise
        {
            Instruction = "O que significa em português?",
            Prompt = target.En,
            Options = options,
            AnswerIndex = options.IndexOf(target.Pt),
            Feedback = $"{target.En} = {target.Pt}",
        };
    }

    /// <summary>Completar a frase com a palavra certa.</summary>
    private ChoiceExercise BuildFillBlank(IReadOnlyList<WordEntry> all, WordEntry target)
    {
        var example = target.ExampleEn!;
        var masked = MaskWord(example, target.En);
        var options = Options(all, target, w => w.En, target.En);
        return new ChoiceExercise
        {
            Instruction = "Complete a frase:",
            Prompt = masked,
            Options = options,
            AnswerIndex = options.IndexOf(target.En),
            Feedback = $"{target.En} = {target.Pt}. {example}",
        };
    }

    /// <summary>Montar a frase tocando nas palavras na ordem certa.</summary>
    private ScrambleExercise BuildScramble(WordEntry target)
    {
        var answer = target.ExampleEn!.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var shuffled = answer.ToList();
        for (var attempt = 0; attempt < 5 && shuffled.SequenceEqual(answer); attempt++)
            shuffled = shuffled.OrderBy(_ => _rng.Next()).ToList();

        return new ScrambleExercise
        {
            Instruction = "Monte a frase em inglês:",
            Prompt = target.ExamplePt ?? target.Pt,
            Tokens = shuffled,
            Answer = answer,
            Feedback = target.ExampleEn!,
        };
    }

    /// <summary>Monta 4 alternativas (1 correta + 3 distratores da mesma categoria).</summary>
    private List<string> Options(IReadOnlyList<WordEntry> all, WordEntry target, Func<WordEntry, string> selector, string correct)
    {
        var sameCategory = all
            .Where(w => w.Category == target.Category && w.Id != target.Id)
            .Select(selector)
            .Where(value => value != correct)
            .Distinct()
            .OrderBy(_ => _rng.Next())
            .Take(3)
            .ToList();

        if (sameCategory.Count < 3)
        {
            var extra = all
                .Select(selector)
                .Where(value => value != correct && !sameCategory.Contains(value))
                .Distinct()
                .OrderBy(_ => _rng.Next())
                .Take(3 - sameCategory.Count);
            sameCategory.AddRange(extra);
        }

        var options = sameCategory.Append(correct).OrderBy(_ => _rng.Next()).ToList();
        return options;
    }

    private static bool HasFillableExample(WordEntry word) =>
        !string.IsNullOrWhiteSpace(word.ExampleEn) &&
        word.ExampleEn.Contains(word.En, StringComparison.OrdinalIgnoreCase);

    private static string MaskWord(string sentence, string word)
    {
        var index = sentence.IndexOf(word, StringComparison.OrdinalIgnoreCase);
        return index < 0 ? sentence : sentence[..index] + "____" + sentence[(index + word.Length)..];
    }
}
