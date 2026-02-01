namespace WurmTools.Core.Models;

public class CraftingRecipe
{
    public int Id { get; set; }
    public int ResultItemId { get; set; }
    public string ResultItemName { get; set; } = string.Empty;
    public string? Skill { get; set; }
    public double? MinSkillLevel { get; set; }
    public List<RecipeIngredient> Ingredients { get; set; } = new();
    public string? Notes { get; set; }
}

public class RecipeIngredient
{
    public int Id { get; set; }
    public int RecipeId { get; set; }
    public int? ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public bool IsOptional { get; set; }
}
