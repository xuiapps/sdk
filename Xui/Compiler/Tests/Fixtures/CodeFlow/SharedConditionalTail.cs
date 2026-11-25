namespace Samples.CodeFlow;

public static class SharedConditionalTail
{
    private static int checks;

    private static bool Before(int now, int showAt)
    {
        checks++;
        return now < showAt;
    }

    public static int Run(int value)
    {
        checks = 0;
        bool tapped = (value & 1) != 0;
        int now = value, showAt = 2, fadeAt = 0;
        bool active = true;
        if (!tapped && Before(now, showAt)) active = false;
        else
        {
            showAt = now < showAt ? now : showAt;
            fadeAt = now + (tapped ? 20 : 0);
        }
        if (tapped) fadeAt += 3;
        return (active ? 1000 : 0) + showAt + fadeAt + checks * 100;
    }
}
