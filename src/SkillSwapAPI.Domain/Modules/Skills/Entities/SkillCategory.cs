namespace SkillSwapAPI.Domain.Skills.Entities;

public sealed class SkillCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Skill> Skills { get; set; } = new List<Skill>();
}
