using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Melee.Me.Infrastructure;
using MeleeMeDatabase;

namespace Melee.Me.Models
{
    public class ConnectionModel
    {
        public int ConnectionId { get; set; }
        public string ConnectionName { get; set; }
        public string ConnectionIcon { get; set; }
        public string ConnectionUrl { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public IConnection ConnectionProvider { get; set; }
    }

}