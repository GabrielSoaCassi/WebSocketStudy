namespace ClientWebSocketStudy
{
    public record ChatMessage(string Content, User UserSender, User UserTarget);
    public record User(int Id, string? Name);
}
