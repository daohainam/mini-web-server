namespace MiniWebServer.Abstractions.Http.Header
{
    public class RangeHeader(RangeUnits unit, params RangePart[] parts)
    {
        public RangeUnits Unit { get; } = unit;
        public RangePart[] Parts { get; } = parts ?? throw new ArgumentNullException(nameof(parts));

        public override bool Equals(object? obj)
        {
            if (obj is not RangeHeader rangeHeader) return false;

            return rangeHeader.Unit == Unit && rangeHeader.Parts.SequenceEqual(Parts);
        }

        public override int GetHashCode()
        {
            return Unit.GetHashCode() ^ Parts.GetHashCode();
        }
    }
}
