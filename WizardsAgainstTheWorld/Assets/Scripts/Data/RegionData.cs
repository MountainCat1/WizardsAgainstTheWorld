using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class RegionData
{
    public string Name;
    public List<LocationData> Locations = new();
    public string Type;
    public int Difficulity = 1;
    public int Seed;
    
    public List<string> AdditionalComments = new();

    public void AddLocation(LocationData location)
    {
        Locations.Add(location);
    }

    public LocationData GetLocation(Guid id)
    {
        return Locations.FirstOrDefault(l => l.Id == id);
    }
    
    public void RecalculateNeighbours()
    {
        foreach (var location in Locations)
        {
            location.Neighbours.Clear();
            foreach (var neighbourId in location.NeighbourIds)
            {
                var neighbour = GetLocation(neighbourId);
                if (neighbour != null)
                {
                    location.Neighbours.Add(neighbour);
                }
            }
        }
    }
}