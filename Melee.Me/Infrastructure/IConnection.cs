using Melee.Me.Models;

namespace Melee.Me.Infrastructure
{
    public interface IConnection
    {
        double GetScore(UserModel meleeUser);
    }
}
