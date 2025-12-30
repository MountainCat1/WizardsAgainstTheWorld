using System;
using UnityEngine;

public enum DialogType
{
    Default = 0,
    SelfTalk = 1,
}

[System.Serializable]
public struct DialogSentence : IEquatable<DialogSentence>
{
    [field: SerializeField] public DialogType Type { get; set; }
    [field: SerializeField] public string Text { get; set; }

    public bool Equals(DialogSentence other)
    {
        return Type == other.Type && Text == other.Text;
    }

    public override bool Equals(object obj)
    {
        return obj is DialogSentence other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int)Type, Text);
    }
}