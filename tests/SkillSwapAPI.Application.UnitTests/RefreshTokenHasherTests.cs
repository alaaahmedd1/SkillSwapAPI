using SkillSwapAPI.Application.Common.Security;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests;

public class RefreshTokenHasherTests
{
    [Fact]
    public void Hash_ShouldReturnDeterministicSha256HexValue()
    {
        var hash = RefreshTokenHasher.Hash("refresh-token");

        Assert.Equal("0EB17643D4E9261163783A420859C92C7D212FA9624106A12B510AFBEC266120", hash);
        Assert.Equal(64, hash.Length);
    }
}
