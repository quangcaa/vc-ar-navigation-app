using System.Collections.Generic;

/// <summary>
/// Interface for searchable location data.
/// Implement this interface to provide your own data source.
/// </summary>
public interface ISearchableData
{
    /// <summary>
    /// Gets all available locations for searching
    /// </summary>
    List<LocationData> GetAllLocations();

    /// <summary>
    /// Searches for locations matching the query
    /// </summary>
    /// <param name="query">Search query string</param>
    /// <returns>List of matching locations</returns>
    List<LocationData> SearchLocations(string query);
}

/// <summary>
/// Data structure representing a location
/// </summary>
[System.Serializable]
public class LocationData
{
    public string id;
    public string name;
    public string description;
    
    // Add more fields as needed (coordinates, category, etc.)
    // public Vector3 position;
    // public string category;
}

