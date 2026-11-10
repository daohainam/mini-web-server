namespace MiniWebServer.Server.ProtocolHandlers.Http2;

/// <summary>
/// The HPACK decoding context: the static table plus a dynamic table (RFC 7541 2.3), addressed through a single index space.
/// Indexes 1..61 are static entries, 62.. are dynamic entries ordered from newest to oldest.
/// </summary>
public sealed class HPACKHeaderTable
{
    public const int DefaultMaxSize = 4096;

    // the newest entry is stored last, so dynamic index i (1-based) maps to entries[Count - i]
    private readonly List<HPACKHeader> entries = [];

    public HPACKHeaderTable() : this(DefaultMaxSize, DefaultMaxSize)
    {
    }

    public HPACKHeaderTable(int maxSize) : this(maxSize, maxSize)
    {
    }

    public HPACKHeaderTable(int maxSize, int sizeLimit)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxSize);
        ArgumentOutOfRangeException.ThrowIfNegative(sizeLimit);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(maxSize, sizeLimit);

        MaxSize = maxSize;
        SizeLimit = sizeLimit;
    }

    /// <summary>
    /// The current maximum size of the dynamic table, changed by dynamic table size updates (RFC 7541 6.3).
    /// </summary>
    public int MaxSize { get; private set; }

    /// <summary>
    /// The largest value a size update may set, this is the SETTINGS_HEADER_TABLE_SIZE we advertised (RFC 7541 4.2).
    /// </summary>
    public int SizeLimit { get; }

    public int Size { get; private set; }

    public int Count => entries.Count;

    public bool TryGetHeader(int index, out HPACKHeader? header)
    {
        if (index >= 1 && index <= HPACKStaticTable.TableSize)
        {
            header = HPACKStaticTable.Entries[index - 1];
            return true;
        }

        int dynamicIndex = index - HPACKStaticTable.TableSize; // 1-based, 1 == newest
        if (dynamicIndex >= 1 && dynamicIndex <= entries.Count)
        {
            header = entries[entries.Count - dynamicIndex];
            return true;
        }

        header = null;
        return false;
    }

    /// <summary>
    /// Adds an entry to the dynamic table, evicting the oldest entries until it fits (RFC 7541 4.4).
    /// </summary>
    public void Add(HPACKHeader header)
    {
        ArgumentNullException.ThrowIfNull(header);

        int entrySize = header.Size;
        if (entrySize > MaxSize)
        {
            // an entry larger than the table empties the table and is not added
            Clear();
            return;
        }

        while (Size + entrySize > MaxSize)
        {
            Evict();
        }

        entries.Add(header);
        Size += entrySize;
    }

    /// <summary>
    /// Applies a dynamic table size update sent by the encoder.
    /// </summary>
    public void SetMaxSize(int maxSize)
    {
        if (maxSize < 0 || maxSize > SizeLimit)
        {
            throw new HPACKDecodingException($"Dynamic table size update {maxSize} exceeds the limit of {SizeLimit}");
        }

        MaxSize = maxSize;
        while (Size > MaxSize)
        {
            Evict();
        }
    }

    public void Clear()
    {
        entries.Clear();
        Size = 0;
    }

    private void Evict()
    {
        Size -= entries[0].Size;
        entries.RemoveAt(0);
    }
}
