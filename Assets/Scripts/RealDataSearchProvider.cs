using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Example implementation of ISearchableData for real data.
/// Replace this with your actual data source (API, database, POI system, etc.)
/// </summary>
public class RealDataSearchProvider : MonoBehaviour, ISearchableData
{
    [Header("Data Source Options")]
    [SerializeField] private bool usePOISystem = true; // Use MultiSet POI system
    [SerializeField] private AugmentedSpace augmentedSpace; // Reference to AugmentedSpace if using POI system

    private List<LocationData> cachedLocations = new List<LocationData>();

    void Awake()
    {
        LoadLocations();
    }

    /// <summary>
    /// Load locations from your data source
    /// This is where you'll connect to your actual data
    /// </summary>
    private void LoadLocations()
    {
        cachedLocations.Clear();

        if (usePOISystem && augmentedSpace != null)
        {
            // Convert POIs to LocationData
            POI[] pois = augmentedSpace.GetPOIs();
            foreach (POI poi in pois)
            {
                LocationData location = new LocationData
                {
                    id = poi.GetId().ToString(),
                    name = poi.poiName,
                    description = poi.description
                };
                cachedLocations.Add(location);
            }
        }
        else
        {
            // TODO: Load from your API, database, or other data source
            // Example:
            // cachedLocations = YourAPIService.GetAllLocations();
            // cachedLocations = YourDatabase.GetLocations();
        }
    }

    public List<LocationData> GetAllLocations()
    {
        return new List<LocationData>(cachedLocations);
    }

    public List<LocationData> SearchLocations(string query)
    {
        if (string.IsNullOrEmpty(query))
            return new List<LocationData>();

        string lowerQuery = query.ToLower().Trim();
        List<LocationData> results = new List<LocationData>();

        foreach (LocationData location in cachedLocations)
        {
            if (location.name.ToLower().Contains(lowerQuery) ||
                location.description.ToLower().Contains(lowerQuery))
            {
                results.Add(location);
            }
        }

        return results;
    }

    /// <summary>
    /// Call this to refresh locations from data source
    /// </summary>
    public void RefreshLocations()
    {
        LoadLocations();
    }
}

