[System.Serializable]
public class LeaderboardResponse
{
    public int levelId;
    public ScoreEntry[] scores;
}

[System.Serializable]
public class ScoreEntry
{
    public string playerName;
    public float timeSec;
    public string createdAt;
}
