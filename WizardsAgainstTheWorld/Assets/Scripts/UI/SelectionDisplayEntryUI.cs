using System;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectionDisplayEntryUI : MonoBehaviour
{
    [CanBeNull] private Action<Creature> _onClick;
    
    private static readonly int ReplacementColor = Shader.PropertyToID("_ReplacementColor");
    [SerializeField] private TextMeshProUGUI creatureNameText;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider xpSlider;
    [SerializeField] private Image creatureImage;
    
    private Creature _creature;

    private void Awake()
    {
        // Ensure that the material is instantiated to avoid modifying the shared material
        creatureImage.material = new Material(creatureImage.material);
    }

    private void OnDestroy()
    {
        if (_creature)
        {
            _creature.Health.ValueChanged -= OnHealthValueChanged;
            _creature.LevelingComponent.ChangedXp -= OnXpValueChanged;
        }
        
        if (creatureImage.material)
        {
            Destroy(creatureImage.material);
        }
    }

    public void SetCreature(Creature creature, [CanBeNull] Action<Creature> onClick, int index)
    {
        _creature = creature;
        
        creatureNameText.text = $"[{index + 1}] {creature.name}";
        healthSlider.maxValue = creature.Health.MaxValue;
        healthSlider.value = creature.Health.CurrentValue;
        
        xpSlider.maxValue = 1f;
        xpSlider.minValue = 0f;
        xpSlider.value = creature.LevelingComponent.LevelProgress;
        
        _creature.Health.ValueChanged += OnHealthValueChanged;
        _creature.LevelingComponent.ChangedXp += OnXpValueChanged;
        creatureImage.sprite = creature.GetComponentInChildren<SpriteRenderer>().sprite;
        
        creatureImage.material.SetColor(ReplacementColor, creature.Color);
        
        _onClick = onClick;
    }

    private void OnXpValueChanged()
    {
        if(!_creature)
        {
            return;
        }
        
        xpSlider.value = _creature.LevelingComponent.LevelProgress;
    }

    private void OnHealthValueChanged()
    {
        if(!_creature)
        {
            return;
        }
        
        healthSlider.value = _creature.Health.CurrentValue;
    }
    
    public void HandleClick()
    {
        _onClick?.Invoke(_creature);
    }
}
