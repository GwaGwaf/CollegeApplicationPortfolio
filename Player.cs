using Godot;

public partial class Player : CharacterBody2D
{
    #region Overrides
    public override void _Ready()
    {
        _animatedSprite = GetNode<AnimatedSprite2D>("PlayerAnimatedSprite2D");
        Main.Consciousness = Main.World.DreamWorld;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Main.Consciousness == Main.World.DreamWorld)
        {
            if (Main.AreaProgressed == Main.DreamWorldProgression.Area1) Mask = "Happy";
            else if (Main.AreaProgressed == Main.DreamWorldProgression.Area2) Mask = "Meh";
            else if (Main.AreaProgressed == Main.DreamWorldProgression.Area3) Mask = "Lost";
            else if (Main.AreaProgressed == Main.DreamWorldProgression.Area4) Mask = "Sad";
            else if (Main.AreaProgressed == Main.DreamWorldProgression.Area5) Mask = "Angry";
            else if (Main.AreaProgressed == Main.DreamWorldProgression.Area6) Mask = "Unmasked";
        }
        if (FirstFrame)
        {
            if (Main.Consciousness == Main.World.DreamWorld) _animatedSprite.Play($"{Mask}DownIdle");
            else
            {
                _animatedSprite.Play("WalkDown");
                _animatedSprite.Stop();
            }

            FirstFrame = false;
        }
        Vector2 velocity = Vector2.Zero;
        const float speedPerSecond = 250;

        if (Input.IsActionPressed("Up")) velocity.Y -= 1;
        else if (Input.IsActionPressed("Down")) velocity.Y += 1;
        if (Input.IsActionPressed("Left")) velocity.X -= 1;
        else if (Input.IsActionPressed("Right")) velocity.X += 1;
        if (Main.Consciousness == Main.World.RealWorld)
        {
            if (velocity.Length() > 0)
            {
                if (velocity.X != 0 && velocity.Y != 0)
                {
                    if (velocity.X < 0 && velocity.Y < 0) _animatedSprite.Play("WalkLeft");
                    else if (velocity.X > 0 && velocity.Y < 0) _animatedSprite.Play("WalkRight");
                    else if (velocity.X < 0 && velocity.Y > 0) _animatedSprite.Play("WalkLeft");
                    else if (velocity.X > 0 && velocity.Y > 0) _animatedSprite.Play("WalkRight");
                }
                else if (velocity.Y < 0) _animatedSprite.Play("WalkUp");
                else if (velocity.Y > 0) _animatedSprite.Play("WalkDown");
                else if (velocity.X < 0) _animatedSprite.Play("WalkLeft");
                else if (velocity.X > 0) _animatedSprite.Play("WalkRight");
            }
            if (velocity == Vector2.Zero) _animatedSprite.Stop();
        }
        else if (Main.Consciousness == Main.World.DreamWorld)
        {
            if (velocity.Length() > 0)
            {
                if (velocity.X != 0 && velocity.Y != 0)
                {
                    if (velocity.X < 0 && velocity.Y < 0)
                    {
                        _animatedSprite.Play($"{Mask}Left");
                        LastKeyPressed = "Left";
                    }
                    else if (velocity.X > 0 && velocity.Y < 0)
                    {
                        _animatedSprite.Play($"{Mask}Right");
                        LastKeyPressed = "Right";
                    }
                    else if (velocity.X < 0 && velocity.Y > 0)
                    {
                        _animatedSprite.Play($"{Mask}Left");
                        LastKeyPressed = "Left";
                    }
                    else if (velocity.X > 0 && velocity.Y > 0)
                    {
                        _animatedSprite.Play($"{Mask}Right");
                        LastKeyPressed = "Right";
                    }
                }
                else if (velocity.Y < 0)
                {
                    _animatedSprite.Play("AllDreamUp");
                    LastKeyPressed = "Up";
                }
                else if (velocity.Y > 0)
                {
                    _animatedSprite.Play($"{Mask}Down");
                    LastKeyPressed = "Down";
                }
                else if (velocity.X < 0)
                {
                    _animatedSprite.Play($"{Mask}Left");
                    LastKeyPressed = "Left";
                }   
                else if (velocity.X > 0)
                {
                    _animatedSprite.Play($"{Mask}Right");
                    LastKeyPressed = "Right";
                }
            }
            if (velocity == Vector2.Zero)
            {
                if (LastKeyPressed == "Up")
                {
                    _animatedSprite.Play("DreamUpIdle");
                    _animatedSprite.Stop();
                }
                if (LastKeyPressed == "Down")
                {
                    _animatedSprite.Play($"{Mask}DownIdle");
                    _animatedSprite.Stop();
                }
                if (LastKeyPressed == "Left")
                {
                    _animatedSprite.Play($"{Mask}LeftIdle");
                    _animatedSprite.Stop();
                }
                if (LastKeyPressed == "Right")
                {
                    _animatedSprite.Play($"{Mask}RightIdle");
                    _animatedSprite.Stop();
                }
            }
        }
        
        velocity = velocity.Normalized() * speedPerSecond;

        Velocity = velocity;
        MoveAndSlide();
    }
    #endregion

    #region Methods
    public void Init(Vector2 position)
    {
        Position = position;
    }
    #endregion

    #region Properties
    protected AnimatedSprite2D _animatedSprite;
    public static string LastKeyPressed { get; set; }
    protected bool FirstFrame = true;
    protected string Mask { get; set; }
    #endregion
}