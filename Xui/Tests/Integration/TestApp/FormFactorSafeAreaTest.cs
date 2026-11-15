using System.Runtime.CompilerServices;
using Xui.Apps.TestApp;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Runtime.Test;

namespace Xui.Tests.Integration.TestApp;

public class FormFactorSafeAreaTest
{
    private static readonly Size WindowSize = (600, 400);

    private static TestSinglePageApp<Application, MainWindow> CreateApp(
        TestRuntimeVariant runtimeVariant,
        [CallerFilePath] string callerPath = "",
        [CallerMemberName] string testName = "") =>
        IntegrationRuntimeVariants.CreateApp<Application, MainWindow>(
            WindowSize, runtimeVariant, callerPath, testName);

    [Theory]
    [MemberData(nameof(IntegrationRuntimeVariants.PhoneFormFactors), MemberType = typeof(IntegrationRuntimeVariants))]
    public void HomePage_Respects_SafeArea(TestRuntimeVariant runtimeVariant)
    {
        using var app = CreateApp(runtimeVariant);
        app.Run(() =>
        {
            app.Prepare();
            app.MarkdownHeading("Scenario");
            app.MarkdownParagraph($"Render home page in {runtimeVariant}.");
            app.Snapshot("HomePage");
        });
    }

    [Theory]
    [MemberData(nameof(IntegrationRuntimeVariants.PhoneFormFactors), MemberType = typeof(IntegrationRuntimeVariants))]
    public void TextMetrics_Respects_SafeArea(TestRuntimeVariant runtimeVariant)
    {
        using var app = CreateApp(runtimeVariant);
        app.Run(() =>
        {
            app.Prepare();
            app.MarkdownHeading("Scenario");
            app.MarkdownParagraph($"Navigate to TextMetrics in {runtimeVariant}.");
            app.Open<Xui.Apps.TestApp.Examples.TextMetricsExample>("TextMetrics");
            app.Snapshot("TextMetrics");
        });
    }
}
