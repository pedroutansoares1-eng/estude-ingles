using Avalonia;
using Avalonia.Headless;
using EnglishStudy.Tests;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

namespace EnglishStudy.Tests;

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<EnglishStudy.App>()
        .UseSkia()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
}
