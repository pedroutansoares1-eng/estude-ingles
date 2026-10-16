using EnglishStudy.Models;
using EnglishStudy.Services;

namespace EnglishStudy.Tests;

public class ContentTests
{
    [Fact]
    public void SeedWords_HaveUniqueIdsAndKnownCategories()
    {
        var words = SeedWords.All();
        var categoryKeys = Categories.All.Select(c => c.Key).ToHashSet();

        Assert.True(words.Count >= 100, $"esperava 100+ palavras, achou {words.Count}");
        Assert.Equal(words.Count, words.Select(w => w.Id).Distinct().Count());

        foreach (var word in words)
        {
            Assert.Contains(word.Category, categoryKeys);
            Assert.False(string.IsNullOrWhiteSpace(word.Pt));
            Assert.False(string.IsNullOrWhiteSpace(word.En));
            Assert.False(string.IsNullOrWhiteSpace(word.ExampleEn));
            Assert.False(string.IsNullOrWhiteSpace(word.ExamplePt));
        }
    }

    [Fact]
    public void GrammarBank_IsValid()
    {
        var bank = SeedGrammar.All();
        var topicKeys = GrammarTopics.All.Select(t => t.Key).ToHashSet();

        Assert.True(bank.Count >= 40, $"esperava 40+ perguntas, achou {bank.Count}");
        Assert.Equal(bank.Count, bank.Select(q => q.Id).Distinct().Count());

        foreach (var question in bank)
        {
            Assert.Contains(question.Topic, topicKeys);
            Assert.Equal(4, question.Options.Length);
            Assert.InRange(question.AnswerIndex, 0, 3);
            Assert.Equal(4, question.Options.Distinct().Count());
            Assert.Contains("____", question.Prompt);
            Assert.False(string.IsNullOrWhiteSpace(question.Explanation));
        }
    }
}

public class LessonGeneratorTests
{
    [Fact]
    public void BuildLesson_ReturnsRequestedCount_WithValidExercises()
    {
        var words = SeedWords.All();
        var generator = new LessonGenerator(new Random(42));

        var lesson = generator.BuildLesson(words, "food", 6);

        Assert.Equal(6, lesson.Count);

        foreach (var exercise in lesson.OfType<ChoiceExercise>())
        {
            Assert.Equal(4, exercise.Options.Count);
            Assert.Equal(4, exercise.Options.Distinct().Count());
            Assert.InRange(exercise.AnswerIndex, 0, 3);
            Assert.False(string.IsNullOrWhiteSpace(exercise.AnswerText));
        }

        foreach (var exercise in lesson.OfType<ScrambleExercise>())
        {
            Assert.NotEmpty(exercise.Answer);
            Assert.Equal(
                exercise.Answer.OrderBy(t => t),
                exercise.Tokens.OrderBy(t => t));
            Assert.False(string.IsNullOrWhiteSpace(exercise.Prompt));
        }
    }

    [Fact]
    public void BuildLesson_WithoutCategory_UsesEveryWord()
    {
        var words = SeedWords.All();
        var generator = new LessonGenerator(new Random(7));

        var lesson = generator.BuildLesson(words, null, 6);

        Assert.Equal(6, lesson.Count);
    }

    [Fact]
    public void BuildLesson_ProducesFillInTheBlank_AndScramble()
    {
        var words = SeedWords.All();
        var generator = new LessonGenerator(new Random(2026));

        var prompts = new List<string>();
        var kinds = new List<Type>();
        for (var i = 0; i < 10; i++)
        {
            foreach (var exercise in generator.BuildLesson(words, "routine", 6))
            {
                prompts.Add(exercise.Prompt);
                kinds.Add(exercise.GetType());
            }
        }

        Assert.Contains(prompts, p => p.Contains("____"));
        Assert.Contains(kinds, t => t == typeof(ScrambleExercise));
    }
}

public class JsonStoreTests
{
    private static string TempPath() =>
        Path.Combine(Path.GetTempPath(), $"englishstudy-tests-{Guid.NewGuid():N}.json");

    [Fact]
    public void Load_SeedsContent_AndCreatesOneCardPerWord()
    {
        var path = TempPath();
        try
        {
            var store = new JsonStore(path);

            var data = store.Load();

            Assert.Equal(SeedWords.All().Count, data.Words.Count);
            Assert.Equal(data.Words.Count, data.Cards.Count);
            Assert.All(data.Cards, card => Assert.Contains(data.Words, w => w.Id == card.WordId));
            Assert.True(File.Exists(path), "a primeira execução deve gravar o arquivo de dados");
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Save_AndReload_KeepsProgress_AndUserWords()
    {
        var path = TempPath();
        try
        {
            var store = new JsonStore(path);
            var data = store.Load();
            data.Profile.Xp = 321;
            data.Words.Add(new WordEntry
            {
                Id = "u-test",
                Pt = "teste",
                En = "test",
                Category = "conversation",
                IsUser = true,
            });
            store.Save(data);

            var reloaded = new JsonStore(path).Load();

            Assert.Equal(321, reloaded.Profile.Xp);
            Assert.Contains(reloaded.Words, w => w.Id == "u-test" && w.IsUser);
            // O cartão da palavra nova é criado automaticamente.
            Assert.Contains(reloaded.Cards, c => c.WordId == "u-test");
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Load_DoesNotDuplicateSeedWords_OnSecondRun()
    {
        var path = TempPath();
        try
        {
            var store = new JsonStore(path);
            var first = store.Load();
            store.Save(first);

            var second = new JsonStore(path).Load();

            Assert.Equal(first.Words.Count, second.Words.Count);
            Assert.Equal(SeedWords.All().Count, second.Words.Count);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
