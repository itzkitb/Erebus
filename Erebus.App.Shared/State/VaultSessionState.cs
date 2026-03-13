using Erebus.Core;
using Erebus.Core.Interfaces;
using Microsoft.Extensions.Logging;

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
    private readonly ILogger<VaultSessionState> _logger;

    public IVaultRecordRepository? Repository => _repository;
    public VaultId? CurrentVaultId => _currentVaultId;
    public bool IsVaultOpen => _repository != null && _currentVaultId.HasValue;

    public event Action? OnStateChanged;

    public VaultSessionState(ILogger<VaultSessionState> logger)
    {
        _logger = logger;
    }

    public void InitializeSession(TimeSpan timeout)
    {
        _sessionStart = DateTime.UtcNow;
        _sessionTimeout = timeout;
        _logger.LogDebug("Session initialized with timeout: {Timeout} minutes", timeout.TotalMinutes);
    }

    public void OpenVault(VaultId vaultId, IVaultRecordRepository repository, byte[] encryptionKey)
    {
        _logger.LogInformation("Opening vault: {VaultId}", vaultId);
        _currentVaultId = vaultId;
        _repository = repository;
        _encryptionKey = encryptionKey;
        _sessionStart = DateTime.UtcNow;
        OnStateChanged?.Invoke();
        _logger.LogInformation("Vault opened successfully: {VaultId}", vaultId);
    }

    public byte[] GetEncryptionKey()
    {
        if (_encryptionKey == null)
        {
            _logger.LogWarning("Attempt to get encryption key when vault is not open");
            throw new InvalidOperationException("Vault is not open");
        }
        return _encryptionKey;
    }

    public void CloseVault()
    {
        _logger.LogInformation("Closing vault: {VaultId}", _currentVaultId);
        _repository?.Dispose();
        _repository = null;
        _currentVaultId = null;
        if (_encryptionKey != null)
        {
            Array.Clear(_encryptionKey);
            _encryptionKey = null;
        }
        OnStateChanged?.Invoke();
        _logger.LogInformation("Vault closed successfully");
    }

    public bool IsSessionExpired()
    {
        var expired = DateTime.UtcNow - _sessionStart > _sessionTimeout;
        if (expired)
        {
            _logger.LogWarning("Session expired after {Elapsed} minutes", (DateTime.UtcNow - _sessionStart).TotalMinutes);
        }
        return expired;
    }

    public void ResetSessionTimeout()
    {
        _logger.LogDebug("Resetting session timeout");
        _sessionStart = DateTime.UtcNow;
    }
}
