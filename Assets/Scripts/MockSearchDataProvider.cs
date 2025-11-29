using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Mock data provider for testing search functionality.
/// Replace this with your actual data source later.
/// </summary>
public class MockSearchDataProvider : MonoBehaviour, ISearchableData
{
    [SerializeField] private List<LocationData> mockLocations = new List<LocationData>();

    void Awake()
    {
        // Initialize with some sample data for testing
        if (mockLocations.Count == 0)
        {
            mockLocations = new List<LocationData>
            {
                new LocationData { id = "1", name = "Shop A", description = "Main shopping area" },
                new LocationData { id = "2", name = "Shop B", description = "Electronics store" },
                new LocationData { id = "3", name = "Shop C", description = "Clothing store" },
                new LocationData { id = "4", name = "Restaurant", description = "Food court" },
                new LocationData { id = "5", name = "Elevator", description = "Main elevator" },
                new LocationData { id = "6", name = "Exit", description = "Main exit" },
                new LocationData { id = "7", name = "Toilet", description = "Restroom" },
                new LocationData { id = "8", name = "Information Desk", description = "Customer service" }
            };
        }
    }

    public List<LocationData> GetAllLocations()
    {
        return mockLocations;
    }

    public List<LocationData> SearchLocations(string query)
    {
        if (string.IsNullOrEmpty(query))
            return new List<LocationData>();

        string lowerQuery = query.ToLower().Trim();
        
        return mockLocations.Where(location =>
            location.name.ToLower().Contains(lowerQuery) ||
            location.description.ToLower().Contains(lowerQuery)
        ).ToList();
    }

    /// <summary>
    /// Call this method to set your actual data when ready
    /// </summary>
    public void SetLocations(List<LocationData> locations)
    {
        mockLocations = locations;
    }
}

