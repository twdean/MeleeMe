using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Melee.Me.Models;

namespace Melee.Me.Infrastructure
{
    public interface IMeleeRepository : IRepository<MeleeModel>
    {
        MeleeModel Get(string id);
        void Add(UserModel challenger, UserModel opponent, string winner, string loser);
    }
}
