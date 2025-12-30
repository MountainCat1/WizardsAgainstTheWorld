using System;

public interface IReadonlyRangedValue 
{
    public float CurrentValue{ get; }
    public float MinValue{ get; }
    
    public float MaxValue { get; }
    
    public event Action ValueChanged;
}