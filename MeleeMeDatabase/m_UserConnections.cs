//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MeleeMeDatabase
{
    using System;
    using System.Collections.Generic;
    
    public partial class m_UserConnections
    {
        public int UserConnectionId { get; set; }
        public int UserId { get; set; }
        public int ConnectionId { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string OAuthToken { get; set; }
    
        public virtual m_Connections m_Connections { get; set; }
        public virtual m_User m_User { get; set; }
    }
}
