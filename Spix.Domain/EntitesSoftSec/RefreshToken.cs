using Spix.Domain.Entities;

namespace Spix.Domain.EntitesSoftSec;

public class RefreshToken
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;   
    public string Token { get; set; } = null!;
    public DateTime Expiration { get; set; }
    public bool IsRevoked { get; set; }

    public User User { get; set; } = null!;
}
