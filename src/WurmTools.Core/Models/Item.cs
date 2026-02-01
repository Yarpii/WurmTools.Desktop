namespace WurmTools.Core.Models;

public class Item
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? Material { get; set; }
    public double? Weight { get; set; }
    public string? Skill { get; set; }
    public double? Difficulty { get; set; }
    public bool IsContainer { get; set; }
    public bool IsImproved { get; set; }
    public string? WikiUrl { get; set; }
    public string? Notes { get; set; }
}
