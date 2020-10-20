namespace Scouts.Events
{
    public class FilterEventArgs
    {
        public FilterEventArgs(int eventType, int publicType, bool isUrgent)
        {
            EventType = eventType;
            PublicType = publicType;
            IsUrgent = isUrgent;
        }

        public int EventType { get; }
        public int PublicType { get; }
        public bool IsUrgent { get; }
    }
}