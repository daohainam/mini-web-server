namespace MiniWebServer.Abstractions.Http.Header
{
    public class RangePart
    {
        public RangePart(long start, long? end)
        {
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start));
            if (end < 0)
                throw new ArgumentOutOfRangeException(nameof(end));
            if (end < start)
                throw new ArgumentException("Start must not greater than End");

            Start = start;
            End = end;
        }

        public long Start { get; }
        public long? End { get; }

    }
}
