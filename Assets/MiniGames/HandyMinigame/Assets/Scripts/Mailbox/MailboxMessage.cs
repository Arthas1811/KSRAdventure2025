using System;

//stores mail message
[Serializable]
public class MailboxMessage
{
    public string Id;
    public string SenderName;
    public string SenderAddress;
    public string Time;
    public string Subject;
    public string Preview;
    public string Body;
    public string ImagePath;
    public string AttachmentName;
    public string AttachmentType;
    public string AttachmentPath;
}

//stores mail message list
[Serializable]
public class MailboxMessageList
{
    public MailboxMessage[] Mails;
}
