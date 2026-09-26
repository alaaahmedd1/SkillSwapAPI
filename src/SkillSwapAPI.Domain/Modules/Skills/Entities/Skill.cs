namespace SkillSwapAPI.Domain.Skills.Entities;

public sealed class Skill
{
    public Guid Id { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public SkillCategory Category { get; set; } = null!;
}