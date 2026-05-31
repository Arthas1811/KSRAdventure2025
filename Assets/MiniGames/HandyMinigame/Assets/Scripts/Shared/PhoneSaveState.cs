using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;

//stores phone and mailbox save state
public class PhoneSaveState
{
    public const string StatesKey = "states";
    public const string PhoneKey = "phone";
    public const string HandyMinigameKey = "minigames";
    public const string LegacyHandyMinigameKey = "handyMinigame";
    public const string MailRecievedKey = "mailRecieved";
    public const string MailReadKey = "mailRead";
    public const string ReadMailIdsKey = "readMailIds";
    public const string MailStatesKey = "mailStates";
    public const string MailStateRecievedKey = "recieved";
    public const string MailStateReadKey = "read";
    public const string MailStateArchivedKey = "archived";
    public const string MailStateDeletedKey = "deleted";
    public const string PhoneDestroyedKey = "phoneDestroyed";
    public const string BlockBlastHighScoreKey = "blockBlastHighScore";
    public const string TetrisHighScoreKey = "tetrisHighScore";

    private const string BasementKey = "basement";
    private const string PrincipalMailId = "PrincipalsMail";
    private const string MailboxStateResourcePath = "Mailbox/MailboxState";
    private const string MailboxStateFileName = "HandyMailboxState.json";

    private readonly SaveDataManager saveDataManager;
    private JObject mailboxStateData;

    //sets phone save state
    public PhoneSaveState(SaveDataManager saveDataManager)
    {
        this.saveDataManager = saveDataManager;
    }

    public JObject SaveData { get; private set; }

    public bool MailRecieved => ReadMinigameBool(MailRecievedKey);

    public bool MailRead => IsMailRead(PrincipalMailId);

    public bool PhoneDestroyed => ReadPhoneDestroyedBool();

    public bool HasUnreadMail => MailRecieved && !MailRead;

    private static string MailboxStateFilePath => Path.Combine(Application.persistentDataPath, MailboxStateFileName);

    //checks mail states
    public void EnsureMailStates(IEnumerable<string> configuredMailIds, bool defaultRecieved, bool defaultRead)
    {
        EnsureMailboxStateLoaded();
        if (EnsureMailboxStates(configuredMailIds, defaultRecieved, defaultRead))
        {
            SaveMailboxStateData();
        }
    }

    //checks unread mail
    public bool HasUnreadMailFor(IEnumerable<string> configuredMailIds)
    {
        EnsureMailboxStateLoaded();

        foreach (string configuredMailId in EnumerateNormalizedMailIds(configuredMailIds))
        {
            if (IsMailRecieved(configuredMailId) && !IsMailDeleted(configuredMailId) && !IsMailRead(configuredMailId))
            {
                return true;
            }
        }

        return false;
    }

    //checks received mail
    public bool IsMailRecieved(string mailId)
    {
        string normalizedId = NormalizeMailId(mailId);
        if (normalizedId == null)
        {
            return false;
        }

        if (IsPrincipalMail(normalizedId))
        {
            return MailRecieved;
        }

        return ReadMailboxBool(normalizedId, MailStateRecievedKey);
    }

    //checks read mail
    public bool IsMailRead(string mailId)
    {
        string normalizedId = NormalizeMailId(mailId);
        return normalizedId != null && ReadMailboxBool(normalizedId, MailStateReadKey);
    }

    //checks archived mail
    public bool IsMailArchived(string mailId)
    {
        string normalizedId = NormalizeMailId(mailId);
        return normalizedId != null && ReadMailboxBool(normalizedId, MailStateArchivedKey);
    }

    //checks deleted mail
    public bool IsMailDeleted(string mailId)
    {
        string normalizedId = NormalizeMailId(mailId);
        return normalizedId != null && ReadMailboxBool(normalizedId, MailStateDeletedKey);
    }

    //marks mail read
    public void MarkMailRead(string mailId, IEnumerable<string> configuredMailIds)
    {
        SetMailRead(mailId, configuredMailIds, true);
    }

    //sets mail read
    public void SetMailRead(string mailId, IEnumerable<string> configuredMailIds, bool read)
    {
        EnsureMailboxStateLoaded();
        bool changed = EnsureMailboxStates(configuredMailIds, true, false);
        changed |= SetMailboxBool(mailId, MailStateReadKey, read);

        if (changed)
        {
            SaveMailboxStateData();
        }
    }

    //sets mail archive
    public void SetMailArchived(string mailId, IEnumerable<string> configuredMailIds, bool archived)
    {
        EnsureMailboxStateLoaded();
        bool changed = EnsureMailboxStates(configuredMailIds, true, false);
        changed |= SetMailboxBool(mailId, MailStateArchivedKey, archived);

        if (changed)
        {
            SaveMailboxStateData();
        }
    }

    //sets mail deleted
    public void SetMailDeleted(string mailId, IEnumerable<string> configuredMailIds, bool deleted)
    {
        EnsureMailboxStateLoaded();
        bool changed = EnsureMailboxStates(configuredMailIds, true, false);
        changed |= SetMailboxBool(mailId, MailStateDeletedKey, deleted);

        if (changed)
        {
            SaveMailboxStateData();
        }
    }

    //loads save data
    public void Load()
    {
        SaveData = saveDataManager.readData();
        if (SaveData == null)
        {
            SaveData = new JObject();
        }

        mailboxStateData = LoadMailboxStateData();
        JObject legacyPhone = GetLegacyPhoneObject();
        bool mailboxChanged = MigrateLegacyMailboxState(legacyPhone);
        bool saveDataChanged = EnsureDefaults(legacyPhone);

        if (mailboxChanged)
        {
            SaveMailboxStateData();
        }

        if (saveDataChanged)
        {
            saveDataManager.saveData(SaveData);
        }
    }

    //checks save defaults
    private bool EnsureDefaults(JObject legacyPhone)
    {
        bool changed = false;

        JObject states = GetOrCreateObject(SaveData, StatesKey, ref changed);
        JObject minigames = GetOrCreateObject(states, HandyMinigameKey, ref changed);
        JObject basement = GetOrCreateObject(states, BasementKey, ref changed);
        JObject legacyMinigames = states[LegacyHandyMinigameKey] as JObject;
        bool phoneDestroyed = ReadBool(basement, PhoneDestroyedKey)
            || ReadBool(minigames, PhoneDestroyedKey)
            || ReadBool(legacyMinigames, PhoneDestroyedKey)
            || ReadBool(legacyPhone, PhoneDestroyedKey);

        SetDefaultBool(minigames, MailRecievedKey, ReadBool(legacyPhone, MailRecievedKey), ref changed);
        if (phoneDestroyed)
        {
            SetBool(basement, PhoneDestroyedKey, true, ref changed);
        }
        else
        {
            SetDefaultBool(basement, PhoneDestroyedKey, false, ref changed);
        }

        if (minigames.Remove(PhoneDestroyedKey))
        {
            changed = true;
        }

        SetDefaultInt(minigames, BlockBlastHighScoreKey, ReadInt(legacyMinigames, BlockBlastHighScoreKey), ref changed);
        SetDefaultInt(minigames, TetrisHighScoreKey, ReadInt(legacyMinigames, TetrisHighScoreKey), ref changed);

        if (legacyMinigames != null && states.Remove(LegacyHandyMinigameKey))
        {
            changed = true;
        }

        if (legacyPhone != null && states.Remove(PhoneKey))
        {
            changed = true;
        }

        return changed;
    }

    //checks mailbox defaults
    private bool EnsureMailboxStates(IEnumerable<string> configuredMailIds, bool defaultRecieved, bool defaultRead)
    {
        bool changed = false;
        JObject mailStates = GetOrCreateMailboxStates(ref changed);

        foreach (string configuredMailId in EnumerateNormalizedMailIds(configuredMailIds))
        {
            JObject mailState = GetOrCreateObject(mailStates, configuredMailId, ref changed);
            if (!IsPrincipalMail(configuredMailId))
            {
                SetDefaultBool(mailState, MailStateRecievedKey, defaultRecieved, ref changed);
            }

            SetDefaultBool(mailState, MailStateReadKey, defaultRead, ref changed);
            SetDefaultBool(mailState, MailStateArchivedKey, false, ref changed);
            SetDefaultBool(mailState, MailStateDeletedKey, false, ref changed);
        }

        return changed;
    }

    //loads mailbox state
    private static JObject LoadMailboxStateData()
    {
        JObject loadedState = TryLoadMailboxStateFromFile();
        if (loadedState == null)
        {
            loadedState = TryLoadMailboxStateFromResources();
        }

        return loadedState ?? new JObject();
    }

    //loads mailbox state from persistent file
    private static JObject TryLoadMailboxStateFromFile()
    {
        try
        {
            if (!File.Exists(MailboxStateFilePath))
            {
                return null;
            }

            return JObject.Parse(File.ReadAllText(MailboxStateFilePath));
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning($"PhoneSaveState: Could not read mailbox state file. {exception.Message}");
            return null;
        }
    }

    //loads default mailbox state
    private static JObject TryLoadMailboxStateFromResources()
    {
        TextAsset defaultState = Resources.Load<TextAsset>(MailboxStateResourcePath);
        if (defaultState == null)
        {
            Debug.LogWarning($"PhoneSaveState: Could not find mailbox state at Resources/{MailboxStateResourcePath}.json.");
            return null;
        }

        try
        {
            return JObject.Parse(defaultState.text);
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning($"PhoneSaveState: Could not parse mailbox state JSON. {exception.Message}");
            return null;
        }
    }

    //stores mailbox state
    private void SaveMailboxStateData()
    {
        string directoryPath = Path.GetDirectoryName(MailboxStateFilePath);
        if (!string.IsNullOrEmpty(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        File.WriteAllText(MailboxStateFilePath, mailboxStateData.ToString());
    }

    //checks mailbox state object
    private void EnsureMailboxStateLoaded()
    {
        if (mailboxStateData == null)
        {
            mailboxStateData = LoadMailboxStateData();
        }
    }

    //moves old phone mail data into the mailbox state file
    private bool MigrateLegacyMailboxState(JObject legacyPhone)
    {
        if (legacyPhone == null)
        {
            return false;
        }

        bool changed = false;
        JObject legacyMailStates = legacyPhone[MailStatesKey] as JObject;
        JArray legacyReadMailIds = legacyPhone[ReadMailIdsKey] as JArray;

        if (legacyMailStates != null)
        {
            foreach (JProperty legacyMailStateProperty in legacyMailStates.Properties())
            {
                string mailId = NormalizeMailId(legacyMailStateProperty.Name);
                JObject legacyMailState = legacyMailStateProperty.Value as JObject;
                if (mailId == null || legacyMailState == null)
                {
                    continue;
                }

                JObject mailState = GetOrCreateMailboxState(mailId, ref changed);
                if (!IsPrincipalMail(mailId))
                {
                    SetDefaultBool(mailState, MailStateRecievedKey, ReadBool(legacyMailState, MailStateRecievedKey), ref changed);
                }

                SetDefaultBool(mailState, MailStateReadKey, ReadBool(legacyMailState, MailStateReadKey), ref changed);
                SetDefaultBool(mailState, MailStateArchivedKey, ReadBool(legacyMailState, MailStateArchivedKey), ref changed);
                SetDefaultBool(mailState, MailStateDeletedKey, false, ref changed);
            }
        }

        if (legacyReadMailIds != null)
        {
            foreach (JToken token in legacyReadMailIds)
            {
                string mailId = token.Type == JTokenType.String ? NormalizeMailId(token.Value<string>()) : null;
                if (mailId == null)
                {
                    continue;
                }

                JObject mailState = GetOrCreateMailboxState(mailId, ref changed);
                SetBool(mailState, MailStateReadKey, true, ref changed);
            }
        }

        return changed;
    }

    //gets legacy phone state
    private JObject GetLegacyPhoneObject()
    {
        JObject states = SaveData?[StatesKey] as JObject;
        return states?[PhoneKey] as JObject;
    }

    //reads minigame bool
    private bool ReadMinigameBool(string key)
    {
        JObject states = SaveData?[StatesKey] as JObject;
        JObject minigames = states?[HandyMinigameKey] as JObject;
        return ReadBool(minigames, key);
    }

    //reads destroyed phone state
    private bool ReadPhoneDestroyedBool()
    {
        JObject states = SaveData?[StatesKey] as JObject;
        JObject basement = states?[BasementKey] as JObject;
        JObject minigames = states?[HandyMinigameKey] as JObject;
        JObject legacyPhone = states?[PhoneKey] as JObject;

        return ReadBool(basement, PhoneDestroyedKey)
            || ReadBool(minigames, PhoneDestroyedKey)
            || ReadBool(legacyPhone, PhoneDestroyedKey);
    }

    //reads mailbox bool
    private bool ReadMailboxBool(string mailId, string key)
    {
        EnsureMailboxStateLoaded();
        JObject mailState = GetMailboxStateObject(mailId);
        return ReadBool(mailState, key);
    }

    //sets mailbox bool
    private bool SetMailboxBool(string mailId, string key, bool value)
    {
        string normalizedId = NormalizeMailId(mailId);
        if (normalizedId == null)
        {
            return false;
        }

        bool changed = false;
        JObject mailState = GetOrCreateMailboxState(normalizedId, ref changed);
        if (!IsPrincipalMail(normalizedId))
        {
            SetDefaultBool(mailState, MailStateRecievedKey, true, ref changed);
        }

        SetDefaultBool(mailState, MailStateReadKey, false, ref changed);
        SetDefaultBool(mailState, MailStateArchivedKey, false, ref changed);
        SetDefaultBool(mailState, MailStateDeletedKey, false, ref changed);
        SetBool(mailState, key, value, ref changed);
        return changed;
    }

    //gets mailbox state
    private JObject GetMailboxStateObject(string mailId)
    {
        JObject mailStates = mailboxStateData?[MailStatesKey] as JObject;
        return mailStates?[mailId] as JObject;
    }

    //gets mailbox state
    private JObject GetOrCreateMailboxState(string mailId, ref bool changed)
    {
        JObject mailStates = GetOrCreateMailboxStates(ref changed);
        return GetOrCreateObject(mailStates, mailId, ref changed);
    }

    //gets mailbox state list
    private JObject GetOrCreateMailboxStates(ref bool changed)
    {
        if (mailboxStateData == null)
        {
            mailboxStateData = new JObject();
            changed = true;
        }

        return GetOrCreateObject(mailboxStateData, MailStatesKey, ref changed);
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

    //sets default bool
    private static void SetDefaultBool(JObject parent, string key, bool defaultValue, ref bool changed)
    {
        if (parent[key] is JValue value && value.Type == JTokenType.Boolean)
        {
            return;
        }

        parent[key] = defaultValue;
        changed = true;
    }

    //sets bool
    private static void SetBool(JObject parent, string key, bool value, ref bool changed)
    {
        if (parent.Value<bool?>(key) == value)
        {
            return;
        }

        parent[key] = value;
        changed = true;
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

    //reads bool
    private static bool ReadBool(JObject parent, string key)
    {
        return parent != null && parent.Value<bool>(key);
    }

    //reads number
    private static int ReadInt(JObject parent, string key)
    {
        return parent != null ? parent.Value<int?>(key) ?? 0 : 0;
    }

    //checks principal mail
    private static bool IsPrincipalMail(string mailId)
    {
        return mailId == PrincipalMailId;
    }

    //gets clean mail ids
    private static IEnumerable<string> EnumerateNormalizedMailIds(IEnumerable<string> mailIds)
    {
        if (mailIds == null)
        {
            yield break;
        }

        foreach (string mailId in mailIds)
        {
            string normalizedId = NormalizeMailId(mailId);
            if (normalizedId != null)
            {
                yield return normalizedId;
            }
        }
    }

    //cleans mail id
    private static string NormalizeMailId(string mailId)
    {
        return string.IsNullOrWhiteSpace(mailId) ? null : mailId.Trim();
    }
}
