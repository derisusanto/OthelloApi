

namespace OthelloAPI.Models;

    public class Player : IPlayer
{
    public string Name {get; set;}
    public PlayerColor Color{get; set;}
    public Player(string name, PlayerColor color)
    {
        Name = name;
        Color = color;
    }
}