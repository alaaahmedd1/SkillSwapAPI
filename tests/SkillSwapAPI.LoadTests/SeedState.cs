using System.Text.Json;

namespace SkillSwapAPI.LoadTests;

public sealed class SeedState
{
    public const string Password = "LoadTest#123!";

    public string RunStamp { get; } = DateTimeOffset.UtcNow.ToString("yyMMddHHmmss");

    public string AdminEmail { get; } = $"load.core.admin.{DateTimeOffset.UtcNow:yyMMddHHmmss}@loadtest.local";
    public string AliceEmail { get; } = $"load.core.alice.{DateTimeOffset.UtcNow:yyMMddHHmmss}@loadtest.local";
    public string BobEmail { get; } = $"load.core.bob.{DateTimeOffset.UtcNow:yyMMddHHmmss}@loadtest.local";
    public string HammerEmail { get; } = $"load.core.hammer.{DateTimeOffset.UtcNow:yyMMddHHmmss}@loadtest.local";

    public string PoolEmailAt(int index) => $"load.pool.{RunStamp}.{index}@loadtest.local";

    public string OtpEmailAt(int index) => $"load.otp.{RunStamp}.{index}@loadtest.local";
    public string ResetEmailAt(int index) => $"load.reset.{RunStamp}.{index}@loadtest.local";
    public string TokenEmailAt(int index) => $"load.token.{RunStamp}.{index}@loadtest.local";
    public string RegisterEmailAt(int index) => $"load.reg.{RunStamp}.{index}@loadtest.local";

    public string AliceToken { get; set; } = "";
    public string BobToken { get; set; } = "";
    public string AdminToken { get; set; } = "";
    public string AliceRefreshToken { get; set; } = "";
    public string AliceExpiredAccessToken { get; set; } = "";

    public List<string> PoolEmails { get; } = [];
    public List<string> PoolTokens { get; } = [];
    public List<Guid> PoolUserIds { get; } = [];

    public Guid AliceId { get; set; }
    public Guid BobId { get; set; }
    public Guid AdminId { get; set; }
    public Guid HammerId { get; set; }

    public int CategoryId { get; set; }
    public Guid OfferedSkillId { get; set; }
    public Guid RequestedSkillId { get; set; }
    public Guid RemovableSkillId { get; set; }
    public Guid LoadRunSkillId { get; set; }
    public Guid AddSkillTargetId { get; set; }

    public Guid AcceptedSwapId { get; set; }
    public Guid CompletedSwapId { get; set; }
    public Guid RejectedSwapId { get; set; }
    public Guid CancelGuardSwapId { get; set; }
    public Guid ProposalSwapId { get; set; }
    public Guid ConversationId { get; set; }
    public Guid ProposalId { get; set; }
    public Guid LiveSessionRoomId { get; set; }
    public Guid EndableRoomId { get; set; }
    public Guid AliceWalletId { get; set; }
    public Guid CreditPackageId { get; set; }
    public Guid AliceUserSkillId { get; set; }
    public Guid RemovableUserSkillId { get; set; }
    public Guid OtpEmailUserId { get; set; }

    public List<Guid> WalletTransactionIds { get; } = [];
    public List<Guid> ReviewableSwapIds { get; } = [];
    public List<Guid> ReviewableReceiverIds { get; } = [];   // parallel – receiver of swap i
    public List<Guid> PendingForAcceptIds { get; } = [];
    public List<Guid> PendingForAcceptReceiverIds { get; } = [];
    public List<Guid> PendingForRejectIds { get; } = [];
    public List<Guid> PendingForRejectReceiverIds { get; } = [];
    public List<Guid> PendingForCancelIds { get; } = [];
    public List<Guid> AcceptedForCompleteIds { get; } = [];
    public List<Guid> AcceptedForCompleteReceiverIds { get; } = [];
    public List<Guid> ProposalAcceptSwapIds { get; } = [];
    public List<Guid> ProposalAcceptReceiverIds { get; } = [];
    public List<Guid> ProposalAcceptIds { get; } = [];
    public List<Guid> ProposalRejectSwapIds { get; } = [];
    public List<Guid> ProposalRejectReceiverIds { get; } = [];
    public List<Guid> ProposalRejectIds { get; } = [];
    public List<Guid> ProposalCreateSwapIds { get; } = [];
    public List<Guid> RemovableUserSkillIds { get; } = [];

    public Guid EndableSwapId { get; set; }

    public List<string> OtpEmails { get; } = [];
    public Dictionary<string, string> OtpCodes { get; } = new(StringComparer.OrdinalIgnoreCase);

    public List<string> ResetEmails { get; } = [];
    public Dictionary<string, string> ResetCodes { get; } = new(StringComparer.OrdinalIgnoreCase);

    public List<TokenPair> TokenPairs { get; } = [];

    public string OtpEmail(int index) =>
        OtpEmails.Count == 0 ? AliceEmail : OtpEmails[index % OtpEmails.Count];

    public string OtpCode(int index)
    {
        var email = OtpEmail(index);
        return OtpCodes.TryGetValue(email, out var code) ? code : "000000";
    }

    public string ResetEmail(int index) =>
        ResetEmails.Count == 0 ? BobEmail : ResetEmails[index % ResetEmails.Count];

    public string ResetCode(int index)
    {
        var email = ResetEmail(index);
        return ResetCodes.TryGetValue(email, out var code) ? code : "000000";
    }

    public TokenPair TokenPairAt(int index) =>
        TokenPairs.Count == 0
            ? new TokenPair(AliceRefreshToken, "expired-access-token")
            : TokenPairs[index % TokenPairs.Count];

    public Guid BatchId(List<Guid> batch, int index, Guid fallback) =>
        batch.Count == 0 ? fallback : batch[index % batch.Count];

    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };

    public Guid PoolUserId(int index) =>
        PoolUserIds.Count == 0 ? BobId : PoolUserIds[index % PoolUserIds.Count];

    public string PoolToken(int index) =>
        PoolTokens.Count == 0 ? BobToken : PoolTokens[index % PoolTokens.Count];

    public Guid ReviewableSwapId(int index) =>
        ReviewableSwapIds.Count == 0 ? CompletedSwapId : ReviewableSwapIds[index % ReviewableSwapIds.Count];

    public Guid WalletTransactionId(int index) =>
        WalletTransactionIds.Count == 0 ? Guid.Empty : WalletTransactionIds[index % WalletTransactionIds.Count];

    public Guid RemovableUserSkillIdAt(int index) =>
        RemovableUserSkillIds.Count == 0
            ? RemovableUserSkillId
            : RemovableUserSkillIds[index % RemovableUserSkillIds.Count];

    /// <summary>
    /// Returns the pool token that belongs to the receiver of swap [index].
    /// Falls back to <paramref name="fallback"/> when the parallel receiver list is empty.
    /// </summary>
    public string BatchReceiverToken(List<Guid> receiverIds, int index, string fallback)
    {
        if (receiverIds.Count == 0) return fallback;
        var receiverId = receiverIds[index % receiverIds.Count];
        var poolIndex = PoolUserIds.IndexOf(receiverId);
        return poolIndex >= 0 && poolIndex < PoolTokens.Count ? PoolTokens[poolIndex] : fallback;
    }

    public Guid ReviewableReceiverId(int index) =>
        ReviewableReceiverIds.Count == 0 ? BobId : ReviewableReceiverIds[index % ReviewableReceiverIds.Count];

    public static string? JsonString(JsonElement element, string propertyName)
    {
        return element.ValueKind == JsonValueKind.Object
               && element.TryGetProperty(propertyName, out var value)
               && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    public static Guid? JsonGuid(JsonElement element, string propertyName)
    {
        var raw = JsonString(element, propertyName);
        return raw is not null && Guid.TryParse(raw, out var id) ? id : null;
    }

    public static int? JsonInt(JsonElement element, string propertyName)
    {
        return element.ValueKind == JsonValueKind.Object
               && element.TryGetProperty(propertyName, out var value)
               && value.ValueKind == JsonValueKind.Number
            ? value.GetInt32()
            : null;
    }

    public static JsonElement ParseJson(string content)
    {
        using var document = JsonDocument.Parse(content);
        return document.RootElement.Clone();
    }

    public static T? Deserialize<T>(string content) where T : class =>
        JsonSerializer.Deserialize<T>(content, Json);
}

public sealed record TokenPair(string RefreshToken, string AccessToken);
