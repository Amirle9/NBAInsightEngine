
namespace NBAInsightEngine.Models
{
    public class Action
    {
        public int ActionNumber { get; set; }
        public string ActionType { get; set; }
        public int? TeamId { get; set; }
        public string PlayerNameI { get; set; }
        public int? PointsTotal { get; set; }
        public string PlayerName { get; set; }
        public string AssistPlayerNameInitial { get; set; }
        public string ReboundPlayerNameInitial { get; set; }
        public string JumpBallWonPlayerName { get; set; }
        public string JumpBallLostPlayerName { get; set; }
        public string FoulDrawnPlayerName { get; set; }
        public int? ReboundTotal { get; set; }
    }
}
