namespace AuthService.Dtos;

public class MenuDto
{
    public required string Name { get; set; }
    public List<MenuDto> SubMenus { get; set; } = [];
}
