using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MeleeMeDatabase;

namespace Melee.Me.Models
{
    public class ConnectionModel
    {
        public int ConnectionId { get; set; }
        public string ConnectionName { get; set; }
        public string ConnectionIcon { get; set; }
        public string ConnectionUrl { get; set; }
        public bool UserHasConnection { get; set; }
    }

}