using Godot;
using System;
using System.Collections.Generic;

public class Hook : Node2D
{
    [Export] private int ChainLength = 15;
    [Export] private float JointSoftness = 0.05f;
    private float LengthOfChainLink = 0f;

    private RigidBody2D ChainLink;
    private Timer _Timer;
    private Godot.Collections.Array PlayerArr = new Godot.Collections.Array();
    private Player _Player;

    private bool IsFired = false;
    private List<Node2D> Chain;

    public override void _Ready()
    {
        ChainLink = GetNode<RigidBody2D>("ChainLink");
        _Timer = GetNode<Timer>("Timer");
        _Player = GetNode<Player>("../Player");
        PlayerArr.Add(_Player);

        ChainLink.Visible = false;
        Chain = new List<Node2D>(ChainLength * 2);

        LengthOfChainLink = 2 * ((CircleShape2D)GetNode<CollisionShape2D>("ChainLink/CollisionShape2D").Shape).Radius;
    }

    private int CheckCollisions(Vector2 StartPosition, Vector2 EndPosition)
    {
        int PossibleLength = 0;
        Physics2DDirectSpaceState spaceState = GetWorld2d().DirectSpaceState;
        Vector2 Direction = (EndPosition - StartPosition).Normalized();
        Vector2 rayEnd = (StartPosition + (Direction * (25 + (ChainLength * LengthOfChainLink))));
        Godot.Collections.Dictionary result = spaceState.IntersectRay(StartPosition, rayEnd, PlayerArr);

        Vector2 positionOfChainEnd = StartPosition;

        if (result.Count != 0)
        {
            Vector2 collisionPosition = (Vector2)result["position"];

            double distanceBetween = Math.Sqrt(Math.Pow((collisionPosition.x - positionOfChainEnd.x), 2) + Math.Pow((collisionPosition.y - positionOfChainEnd.y), 2));
            double unitsBetween = (distanceBetween / LengthOfChainLink);

            PossibleLength = Convert.ToInt32(Math.Floor(unitsBetween));
        }
        else
        {
            PossibleLength = ChainLength;
        }

        return PossibleLength;
    }

    private Node2D GetCollidingNode(Vector2 StartPosition, Vector2 EndPosition, Vector2 NodePosition)
    {
        Physics2DDirectSpaceState spaceState = GetWorld2d().DirectSpaceState;
        Vector2 Direction = (EndPosition - StartPosition).Normalized();
        Vector2 rayEnd = (StartPosition + (Direction * (25 + (ChainLength * LengthOfChainLink))));
        Godot.Collections.Dictionary result = spaceState.IntersectRay(NodePosition, rayEnd, PlayerArr);

        if (result.Count != 0)
        {
            return (Node2D)result["collider"];
        }

        return null;
    }

    public void FireHook(Vector2 StartPosition, Vector2 EndPosition)
    {
        RigidBody2D Link = (RigidBody2D)ChainLink.Duplicate();
        RigidBody2D NextLink = null;
        Link.Position = StartPosition;
        Link.LinearVelocity = Vector2.Zero;
        Link.Visible = true;
        AddChild(Link);

        Vector2 Direction = (EndPosition - StartPosition).Normalized();
        int length = CheckCollisions(StartPosition, EndPosition);

        PinJoint2D Joint = new PinJoint2D();
        Joint.Position = StartPosition - (Direction * LengthOfChainLink * (2f / 3f));
        AddChild(Joint);
        Joint.NodeA = _Player.GetPath();
        Joint.NodeB = Link.GetPath();

        Chain.Add(Joint);

        Joint = new PinJoint2D();
        Joint.Softness = JointSoftness;
        Joint.Position = StartPosition - (Direction * LengthOfChainLink * (2f / 3f));

        for (int i = 0; i < length; i++) {
            NextLink = (RigidBody2D)Link.Duplicate();
            NextLink.Position += (Direction * LengthOfChainLink);
            AddChild(NextLink);

            Chain.Add(NextLink);
            Chain.Add(Link);
            Chain.Add(Joint);

            Joint.NodeA = Link.GetPath();
            Joint.NodeB = NextLink.GetPath();

            Link = NextLink;

            AddChild(Joint);

            Vector2 OldPosition = Joint.Position;
            Joint = new PinJoint2D();
            Joint.Softness = JointSoftness;
            Joint.Position = (OldPosition + (Direction * LengthOfChainLink));
        }

        Node2D hitNode = GetCollidingNode(StartPosition, EndPosition, NextLink.Position);

        if (hitNode != null)
        {
            GD.Print(hitNode.GetPath());
            PinJoint2D FinalJoint = new PinJoint2D();
            AddChild(FinalJoint);

            FinalJoint.Position = (NextLink.Position + (Direction * LengthOfChainLink * (2f / 3f)));
            FinalJoint.NodeA = NextLink.GetPath();
            FinalJoint.NodeB = hitNode.GetPath();

            Chain.Add(FinalJoint);
        }

        IsFired = true;
    }

    public void ReleaseHook()
    {
        if (IsFired)
        {
            foreach (Node2D chainNode in Chain)
            {
                if (Godot.Object.IsInstanceValid(chainNode))
                {
                    if (chainNode is Joint2D)
                        chainNode.QueueFree();
                }
            }

            foreach (Node2D chainNode in Chain)
            {
                if (Godot.Object.IsInstanceValid(chainNode))
                {
                    chainNode.QueueFree();
                }
            }
            
            IsFired = false;
        }
    }
}