using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Melee.Me.Models;

namespace Melee.Me.Infrastructure.Repository
{
    public interface IUserRepository : IRepository<UserModel>
    {
        UserModel Add(string id, string token, string OAuthToken);
        void Delete(UserModel entity);
        UserModel Get(string id);

    }
}
