namespace Melee.Me.Models
{
    public class MeleeResultModel
    {
        public string Winner { get; set; }
        public string WinnerLogo { get; set; }
        public int FollowerScore { get; set; }
        public int FriendScore { get; set; }
        public int PostScore { get; set; }
        public int RecentConnectionScore { get; set; }
        public int CheckInScore { get; set; }
        public int LikeFavouriteScore { get; set; }
        public int PhotoScore { get; set; }


        public MeleeResultModel(string winner, string logo)
        {
            Winner = winner;
            WinnerLogo = logo;
        }

        public MeleeResultModel()
        {

        }

    }
}