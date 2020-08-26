namespace Scouts.Events
{
    public class MessageEventArgs
    {
        public MessageEventArgs(string message, string userId)
        {
            Message = message;
            UserId = userId;
        }

        public string Message { get; }
        public string UserId { get; }
    }
}