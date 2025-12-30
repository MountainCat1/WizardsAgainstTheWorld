using System.Collections;
using System.Collections.Generic;
using Managers;
using TMPro;
using UI;
using UnityEngine;
using Utilities;
using Zenject;

public class TraderChatCloud : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private UISlide slide;

    [SerializeField] private List<string> chatLines = new List<string>();
    [SerializeField] private float typingSpeed = 20f;

    [Inject] private ICrewManager _crewManager;
    
    private void Start()
    {
        slide.Showed += OnSlideIn;
    }

    private void OnSlideIn()
    {
        gameObject.SetActive(true);
        text.text = "";
        StopAllCoroutines();
        StartCoroutine(StartChat());
    }
    
    private IEnumerator StartChat()
    {
        yield return new WaitForSeconds(0.5f);
        
        var line = chatLines.RandomElement();
        foreach (var letter in line)
        {
            text.text += letter;
            yield return new WaitForSeconds(1f / typingSpeed);
        }
        
        yield return new WaitForSeconds(1.5f);
        text.text = "";
        gameObject.SetActive(false);
    }
}