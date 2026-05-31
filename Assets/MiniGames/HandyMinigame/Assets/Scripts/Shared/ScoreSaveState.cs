using Newtonsoft.Json.Linq;

//stores game scores
public class ScoreSaveState
{
    private readonly SaveDataManager saveDataManager;
    private JObject saveData;

    //sets score save state
    public ScoreSaveState(SaveDataManager saveDataManager)
    {
        this.saveDataManager = saveDataManager;
    }

    //reads block blast highscore
    public int LoadBlockBlastHighScore()
    {
        LoadAndRepair();
        JObject handyMinigame = (JObject)saveData[PhoneSaveState.StatesKey][PhoneSaveState.HandyMinigameKey];
        return handyMinigame.Value<int>(PhoneSaveState.BlockBlastHighScoreKey);
    }

    //stores block blast highscore
    public void SaveBlockBlastHighScore(int highScore)
    {
        LoadAndRepair();
        JObject handyMinigame = (JObject)saveData[PhoneSaveState.StatesKey][PhoneSaveState.HandyMinigameKey];
        handyMinigame[PhoneSaveState.BlockBlastHighScoreKey] = highScore;
        saveDataManager.saveData(saveData);
    }

    //reads tetris highscore
    public int LoadTetrisHighScore()
    {
        LoadAndRepair();
        JObject handyMinigame = (JObject)saveData[PhoneSaveState.StatesKey][PhoneSaveState.HandyMinigameKey];
        return handyMinigame.Value<int>(PhoneSaveState.TetrisHighScoreKey);
    }

    //stores tetris highscore
    public void SaveTetrisHighScore(int highScore)
    {
        LoadAndRepair();
        JObject handyMinigame = (JObject)saveData[PhoneSaveState.StatesKey][PhoneSaveState.HandyMinigameKey];
        handyMinigame[PhoneSaveState.TetrisHighScoreKey] = highScore;
        saveDataManager.saveData(saveData);
    }

    //loads and fixes scores
    private void LoadAndRepair()
    {
        saveData = saveDataManager.readData();
        if (saveData == null)
        {
            saveData = new JObject();
        }

        bool changed = false;
        JObject states = GetOrCreateObject(saveData, PhoneSaveState.StatesKey, ref changed);
        JObject legacyHandyMinigame = states[PhoneSaveState.LegacyHandyMinigameKey] as JObject;
        JObject handyMinigame = GetOrCreateObject(states, PhoneSaveState.HandyMinigameKey, ref changed);
        SetDefaultInt(handyMinigame, PhoneSaveState.BlockBlastHighScoreKey, ReadInt(legacyHandyMinigame, PhoneSaveState.BlockBlastHighScoreKey), ref changed);
        SetDefaultInt(handyMinigame, PhoneSaveState.TetrisHighScoreKey, ReadInt(legacyHandyMinigame, PhoneSaveState.TetrisHighScoreKey), ref changed);

        if (legacyHandyMinigame != null && states.Remove(PhoneSaveState.LegacyHandyMinigameKey))
        {
            changed = true;
        }

        if (changed)
        {
            saveDataManager.saveData(saveData);
        }
    }

    //gets save object
    private static JObject GetOrCreateObject(JObject parent, string key, ref bool changed)
    {
        if (parent[key] is JObject existing)
        {
            return existing;
        }

        JObject created = new JObject();
        parent[key] = created;
        changed = true;
        return created;
    }

    //sets default number
    private static void SetDefaultInt(JObject parent, string key, int defaultValue, ref bool changed)
    {
        if (parent[key] is JValue value && value.Type == JTokenType.Integer)
        {
            return;
        }

        parent[key] = defaultValue;
        changed = true;
    }

    //reads number
    private static int ReadInt(JObject parent, string key)
    {
        return parent != null ? parent.Value<int?>(key) ?? 0 : 0;
    }
}
