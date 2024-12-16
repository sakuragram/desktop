using System;

namespace sakuragram;

public static class Constants
{
    public const int MediaPriority = 8;
    public const int ProfilePhotoPriority = 10;
    
    public static readonly Tuple<string, string>[] FolderIcon = 
    [
        new("existing_chats", "\uE8F2"),
        new("new_chats", "\uE8BD"),
        new("airplane", "\uE711"),
        new("all", "\uE8F2"),
        new("book", "\uE82D"),
        new("bots", "\uE99A"),
        new("cat", "\uE711"),
        new("channels", "\uEC24"),
        new("crown", "\uE711"),
        new("custom", "\uE8B7"),
        new("edit", "\uE70F"),
        new("favorite", "\uE734"),
        new("flower", "\uE711"),
        new("game", "\uE7FC"),
        new("group", "\uE902"),
        new("home", "\uE80F"),
        new("light", "\uEA80"),
        new("like", "\uE8E1"),
        new("love", "\uEB51"),
        new("mask", "\uE711"),
        new("money", "\uE711"),
        new("note", "\uEC4F"),
        new("palette", "\uEE56"),
        new("party", "\uED55"),
        new("poo", "\uE711"),
        new("private", "\uE77B"),
        new("setup", "\uE711"),
        new("sport", "\uE711"),
        new("study", "\uE711"),
        new("trade", "\uE711"),
        new("travel", "\uE709"),
        new("unread", "\uEBDB"),
        new("work", "\uE821")
    ];
}