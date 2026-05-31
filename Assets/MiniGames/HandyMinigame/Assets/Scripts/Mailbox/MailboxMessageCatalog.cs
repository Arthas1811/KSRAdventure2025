using System.Collections.Generic;
using UnityEngine;

//loads mail messages
public class MailboxMessageCatalog
{
    public const string DefaultResourcePath = "Mailbox/MailboxMessages";

    private readonly List<MailboxMessage> mails;
    private readonly List<string> mailIds;

    //sets mailbox message catalog
    private MailboxMessageCatalog(List<MailboxMessage> mails)
    {
        this.mails = mails;
        mailIds = new List<string>(mails.Count);

        foreach (MailboxMessage mail in mails)
        {
            mailIds.Add(mail.Id);
        }
    }

    public IReadOnlyList<MailboxMessage> Mails => mails;

    public IReadOnlyList<string> MailIds => mailIds;

    public bool HasMails => mails.Count > 0;

    //loads from resources
    public static MailboxMessageCatalog LoadFromResources()
    {
        return LoadFromResources(DefaultResourcePath);
    }

    //loads from resources
    public static MailboxMessageCatalog LoadFromResources(string resourcePath)
    {
        TextAsset textAsset = Resources.Load<TextAsset>(resourcePath);
        if (textAsset == null)
        {
            Debug.LogWarning($"MailboxMessageCatalog: Could not find mailbox JSON at Resources/{resourcePath}.json.");
            return Empty();
        }

        return FromJson(textAsset.text);
    }

    //reads json mail
    public static MailboxMessageCatalog FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.LogWarning("MailboxMessageCatalog: Mailbox JSON is empty.");
            return Empty();
        }

        MailboxMessageList messageList;
        try
        {
            messageList = JsonUtility.FromJson<MailboxMessageList>(json);
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning($"MailboxMessageCatalog: Could not parse mailbox JSON. {exception.Message}");
            return Empty();
        }

        if (messageList?.Mails == null || messageList.Mails.Length == 0)
        {
            Debug.LogWarning("MailboxMessageCatalog: Mailbox JSON does not contain any mails.");
            return Empty();
        }

        List<MailboxMessage> validMails = new List<MailboxMessage>();
        HashSet<string> usedIds = new HashSet<string>();

        foreach (MailboxMessage mail in messageList.Mails)
        {
            if (mail == null)
            {
                Debug.LogWarning("MailboxMessageCatalog: Skipping a null mail entry.");
                continue;
            }

            mail.Id = Normalize(mail.Id);
            if (string.IsNullOrEmpty(mail.Id))
            {
                Debug.LogWarning("MailboxMessageCatalog: Skipping mail with missing Id.");
                continue;
            }

            if (!usedIds.Add(mail.Id))
            {
                Debug.LogWarning($"MailboxMessageCatalog: Skipping duplicate mail Id '{mail.Id}'.");
                continue;
            }

            mail.SenderName = NormalizeOrEmpty(mail.SenderName);
            mail.SenderAddress = NormalizeOrEmpty(mail.SenderAddress);
            mail.Time = NormalizeOrEmpty(mail.Time);
            mail.Subject = NormalizeOrEmpty(mail.Subject);
            mail.Preview = NormalizeOrEmpty(mail.Preview);
            mail.Body = NormalizeOrEmpty(mail.Body);
            mail.ImagePath = NormalizeOrEmpty(mail.ImagePath);
            mail.AttachmentName = NormalizeOrEmpty(mail.AttachmentName);
            mail.AttachmentType = NormalizeOrEmpty(mail.AttachmentType);
            mail.AttachmentPath = NormalizeOrEmpty(mail.AttachmentPath);
            validMails.Add(mail);
        }

        if (validMails.Count == 0)
        {
            Debug.LogWarning("MailboxMessageCatalog: No valid mails were found.");
        }

        return new MailboxMessageCatalog(validMails);
    }

    //creates empty catalog
    private static MailboxMessageCatalog Empty()
    {
        return new MailboxMessageCatalog(new List<MailboxMessage>());
    }

    //cleans text
    private static string Normalize(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    //cleans text or empty
    private static string NormalizeOrEmpty(string value)
    {
        return Normalize(value) ?? string.Empty;
    }
}
