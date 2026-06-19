namespace GameLogic.Cultivation
{
    public sealed class BattleLogEntry
    {
        public BattleLogEntry(string message)
        {
            Message = message;
        }

        public string Message { get; }
    }
}
