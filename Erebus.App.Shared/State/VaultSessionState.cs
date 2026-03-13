using Erebus.Core;
using Erebus.Core.Interfaces;

namespace Erebus.App.Shared.State;

/// <summary>
/// Application state management for Blazor
/// </summary>
public class VaultSessionState
{
    private IVaultRecordRepository? _repository;
    private VaultId? _currentVaultId;
    private byte[]? _encryptionKey;
    private DateTime _sessionStart;
    private TimeSpan _sessionTimeout;

    public IVaultRecordRepository? Repository => _repository;
    public VaultId? CurrentVaultId => _currentVaultId;
    public bool IsVaultOpen => _repository != null && _currentVaultId.HasValue;

    public event Action? OnStateChanged;

    public void InitializeSession(TimeSpan timeout)
    {
        _sessionStart = DateTime.UtcNow;
        _sessionTimeout = timeout;
    }

    public void OpenVault(VaultId vaultId, IVaultRecordRepository repository, byte[] encryptionKey)
    {
        _currentVaultId = vaultId;
        _repository = repository;
        _encryptionKey = encryptionKey;
        _sessionStart = DateTime.UtcNow;
        OnStateChanged?.Invoke();
    }

    public byte[] GetEncryptionKey()
    {
        if (_encryptionKey == null)
            throw new InvalidOperationException("Vault is not open");
        return _encryptionKey;
    }

    public void CloseVault()
    {
        _repository?.Dispose();
        _repository = null;
        _currentVaultId = null;
        if (_encryptionKey != null)
        {
            Array.Clear(_encryptionKey);
            _encryptionKey = null;
        }
        OnStateChanged?.Invoke();
    }

    public bool IsSessionExpired()
    {
        return DateTime.UtcNow - _sessionStart > _sessionTimeout;
    }

    public void ResetSessionTimeout()
    {
        _sessionStart = DateTime.UtcNow;
    }
}
