namespace Erebus.Core;

/// <summary>
/// Represents a unique identifier for vault entities
/// </summary>
public readonly struct VaultId : IEquatable<VaultId>
{
    private readonly Guid _value;

    public VaultId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Vault ID cannot be empty", nameof(value));
        _value = value;
    }

    public VaultId(string value)
    {
        if (!Guid.TryParse(value, out var guid) || guid == Guid.Empty)
            throw new ArgumentException("Invalid vault ID format", nameof(value));
        _value = guid;
    }

    public static VaultId CreateNew() => new(Guid.NewGuid());

    public override string ToString() => _value.ToString("N");

    public bool Equals(VaultId other) => _value == other._value;

    public override bool Equals(object? obj) => obj is VaultId other && Equals(other);

    public override int GetHashCode() => _value.GetHashCode();

    public static bool operator ==(VaultId left, VaultId right) => left.Equals(right);

    public static bool operator !=(VaultId left, VaultId right) => !left.Equals(right);
}

/// <summary>
/// Represents a unique identifier for records within a vault
/// </summary>
public readonly struct RecordId : IEquatable<RecordId>
{
    private readonly Guid _value;

    public RecordId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Record ID cannot be empty", nameof(value));
        _value = value;
    }

    public RecordId(string value)
    {
        if (!Guid.TryParse(value, out var guid) || guid == Guid.Empty)
            throw new ArgumentException("Invalid record ID format", nameof(value));
        _value = guid;
    }

    public static RecordId CreateNew() => new(Guid.NewGuid());

    public override string ToString() => _value.ToString("N");

    public bool Equals(RecordId other) => _value == other._value;

    public override bool Equals(object? obj) => obj is RecordId other && Equals(other);

    public override int GetHashCode() => _value.GetHashCode();

    public static bool operator ==(RecordId left, RecordId right) => left.Equals(right);

    public static bool operator !=(RecordId left, RecordId right) => !left.Equals(right);
}
