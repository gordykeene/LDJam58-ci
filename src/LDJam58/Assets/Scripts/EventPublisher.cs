public static class EventPublisher
{
    public static void FadeInScene() => Message.Publish(new UiFadeInRequested());
    public static void FadeOutScene() => Message.Publish(new UiFadeOutRequested());
}
