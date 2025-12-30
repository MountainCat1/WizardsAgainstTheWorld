using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogData", menuName = "Custom/Dialog Data")]
public class DialogData : ScriptableObject
{
    [field: SerializeField] public List<DialogSentence> Sentences { get; set; }
    
    [field: SerializeField] public List<DialogSentenceSignal> Signals { get; set; }
}

[System.Serializable]
public class DialogSentenceSignal
{
    [field: SerializeField] public Signal Signal { get; set; }
    [field: SerializeField] public int SentenceIndex { get; set; }
}