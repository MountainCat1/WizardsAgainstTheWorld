using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Utilities;
using Zenject;
using static Utilities.LocalizationHelper;

namespace UI
{
    public class GameEventPanelUI : MonoBehaviour
    {
        [Inject] private DiContainer _diContainer;
        [Inject] private IGameEventManager _gameEventManager;

        [SerializeField] private GameObject eventPanel;
        [SerializeField] private Button eventOptionPrefab;
        [SerializeField] private Transform eventOptionsContainer;

        [SerializeField] private TextMeshProUGUI eventNameText;
        [SerializeField] private TextMeshProUGUI eventDescriptionText;

        [SerializeField] private Color defaultTextColor = Color.white;
        [SerializeField] private Color specialTextColor = Color.magenta;


        private Queue<GameEvent> _eventQueue = new Queue<GameEvent>();

        // Unity Methods
        private void Start()
        {
            //
            eventPanel.SetActive(false);

            _gameEventManager.EventTriggered += DisplayEvent;
        }

        private void Update()
        {
            //
        }

        public void DisplayEvent(GameEvent gameEvent)
        {
            if (eventPanel.activeSelf)
            {
                // If the panel is already active, queue the event for later
                _eventQueue.Enqueue(gameEvent);
                return;
            }

            eventPanel.SetActive(true);
            eventNameText.text = L(gameEvent.TitleKey);
            eventDescriptionText.text = L(gameEvent.DescriptionKey);

            foreach (Transform child in eventOptionsContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (var option in gameEvent.Options)
            {
                var optionVisible = option.CheckVisible?.Invoke() ?? true;

                if (!optionVisible)
                {
                    continue;
                }

                var buttonGo = _diContainer.InstantiatePrefab(eventOptionPrefab, eventOptionsContainer);
                var buttonText = buttonGo.GetComponentInChildren<TextMeshProUGUI>();

                var color = option.Type == GameEventOptionType.Special
                    ? specialTextColor
                    : defaultTextColor;

                buttonText.text = $"> {StringUtilities.WrapInColor(L(option.LabelKey), color)}";
                buttonGo.GetComponent<Button>().onClick.AddListener(() => OnOptionSelected(gameEvent, option));

                var optionEnabled = option.CheckEnabled?.Invoke() ?? true;
                buttonGo.GetComponent<Button>().interactable = optionEnabled;

                var tooltipTrigger = buttonGo.GetComponent<TooltipTrigger>();

                var tooltipTextBuilder = new StringBuilder();
                bool hasTooltipContent = false;

                // Regular effects
                foreach (var effect in option.Effects)
                {
                    if (effect.Hidden)
                        continue;

                    if (string.IsNullOrEmpty(effect.PositiveLabelKey)) continue;

                    var (negative, negativeValue) = IsNegativeNumber(effect.Value);

                    string line = !negative
                        ? L(effect.PositiveLabelKey, effect.Value)
                        : L(effect.NegativeLabelKey, -negativeValue);

                    tooltipTextBuilder.AppendLine(line);
                    tooltipTextBuilder.AppendLine();
                    hasTooltipContent = true;
                }

                // Grouped effects (probabilistic)
                foreach (var group in option.EffectGroups ?? new List<OptionEffectGroup>())
                {
                    if (group.Effects
                        .All(e => (string.IsNullOrEmpty(e.PositiveLabelKey) && string.IsNullOrEmpty(e.ToolTipKey)) ||
                                  e.Hidden
                        ))
                        continue;

                    float chance = Mathf.Round(group.Chance * 100f);
                    tooltipTextBuilder.AppendLine($"({chance}%):");

                    foreach (var effect in group.Effects)
                    {
                        if (effect.Hidden || (string.IsNullOrEmpty(effect.PositiveLabelKey) &&
                                              string.IsNullOrEmpty(effect.ToolTipKey)))
                            continue;

                        var (negative, negativeValue) = IsNegativeNumber(effect.Value);

                        string line = !negative
                            ? L(effect.PositiveLabelKey, effect.Value)
                            : L(effect.NegativeLabelKey, -negativeValue);

                        tooltipTextBuilder.AppendLine($"  • {line}");
                    }

                    tooltipTextBuilder.AppendLine();
                    hasTooltipContent = true;
                }

                if (hasTooltipContent)
                {
                    tooltipTrigger.enabled = true;

                    // Remove trailing empty line
                    if (tooltipTextBuilder.Length >= 2)
                        tooltipTextBuilder.Length -= Environment.NewLine.Length;

                    tooltipTrigger.text = tooltipTextBuilder.ToString();
                }
                else
                {
                    tooltipTrigger.enabled = false;
                }
            }
        }

        private void OnOptionSelected(GameEvent gameEvent, GameEventOption option)
        {
            try
            {
                option.Invoke();
            }
            catch (Exception e)
            {
                GameLogger.LogException(e);
            }

            // TODO: fix
            // we check for it inside DisplayEvent() so we need to set it
            // its stupid but it works
            eventPanel.SetActive(false);

            if (_eventQueue.Any())
            {
                DisplayEvent(_eventQueue.Dequeue());
            }
        }

        public static (bool isNegative, decimal? value) IsNegativeNumber(object obj)
        {
            if (obj == null)
                return (false, null);

            try
            {
                var typeCode = Type.GetTypeCode(obj.GetType());
                switch (typeCode)
                {
                    case TypeCode.SByte:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.Single:
                    case TypeCode.Double:
                    case TypeCode.Decimal:
                        decimal number = Convert.ToDecimal(obj);
                        return (number < 0, number);
                    default:
                        return (false, null);
                }
            }
            catch
            {
                return (false, null);
            }
        }
    }
}