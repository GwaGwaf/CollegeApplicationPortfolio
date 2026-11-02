using Godot;
using System;

public partial class Main : Node2D
{
    #region Overrides
    public override void _Ready()
    {
        Current = this;
        //Change this below to change whether the player has their real world or dream world appearance
        Consciousness = World.DreamWorld;
        //This below changes the mask of the player to fit the stage of the game
        AreaProgressed = DreamWorldProgression.Area1;
        Player = GetNode<Player>("PlayerBody2D");
        StartNewGame();
    }
    #endregion

    #region Methods
    public void StartNewGame()
    {
        var x = ScreenSize.X / 2;
        var y = ScreenSize.Y / 2;
        Player.Init(new Vector2(x, y));
    }
    #endregion

    #region Properties
    public static Main Current { get; set; }
    public enum World { RealWorld, DreamWorld }
    public static World Consciousness { get; set; }
    public enum DreamWorldProgression { Area1, Area2, Area3, Area4, Area5, Area6 }
    public static DreamWorldProgression AreaProgressed { get; set; }
    public Vector2 ScreenSize => GetViewport().GetVisibleRect().Size;
    public Player Player { get; set; }
    #endregion
}
