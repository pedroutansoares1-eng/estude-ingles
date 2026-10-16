namespace EnglishStudy.Models;

public abstract class ExerciseBase
{
    public string Instruction { get; init; } = "";
    public string Prompt { get; init; } = "";
    public string Feedback { get; init; } = "";
}

/// <summary>Multiple choice: translate, or complete a sentence with "____".</summary>
public sealed class ChoiceExercise : ExerciseBase
{
    public IReadOnlyList<string> Options { get; init; } = Array.Empty<string>();
    public int AnswerIndex { get; init; }
    public string AnswerText => Options.Count > AnswerIndex ? Options[AnswerIndex] : "";
}

/// <summary>Tap the words in the right order to build the sentence.</summary>
public sealed class ScrambleExercise : ExerciseBase
{
    public string PromptPt { get; init; } = "";
    public IReadOnlyList<string> Tokens { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Answer { get; init; } = Array.Empty<string>();
}
