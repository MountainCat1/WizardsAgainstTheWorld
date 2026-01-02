using GameplayScene.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Utilities.LocalizationHelper;
namespace GameplayScene.UI
{
    public class ResourceDisplayEntryUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text resourceNameText;
        [SerializeField] private TMP_Text resourceAmountText;
        [SerializeField] private Image resourceIconImage;
        
        public void Initialize(GameResource resource)
        {
            resourceNameText.text = L($"Game.Resources.{resource.Type}");
            resourceAmountText.text = resource.Amount.ToString();
            resourceIconImage.sprite = resource.Icon;
        }
    }
}