using System.Collections.Generic;
using CookingSim.Helpers;
using Godot;

namespace CookingSim.Features.CuttableObjectFeature;

// TODO: Only allow cutting when this object is on a cutting board?

[Tool]
public partial class CuttableObject : Node3D, IFeature {
    public static string FeatureName => "CuttableObjectFeature";
    private const string CuttingObjectsGroupName = "cutting_objects";

    #region Exports and validations
    
    [Export] private PackedScene _sceneReplacement;
    [Export] private RigidBody3D _rigidBody;
    
    public override string[] _GetConfigurationWarnings() {
        List<string> warnings = [];

        if (_sceneReplacement == null) {
            warnings.Add("Scene Replacement is null");
        }
        
        if (_rigidBody == null) {
            warnings.Add("Rigid Body is null");
        }

        return warnings.ToArray();   
    }
    #endregion
    
    public override void _EnterTree() {
        UpdateConfigurationWarnings();
    }
    
    public override void _Ready() {
        _rigidBody.ContactMonitor = true;
        _rigidBody.MaxContactsReported = 1;
        _rigidBody.BodyEntered += CollisionOnBodyEntered;
    }

    private void CollisionOnBodyEntered(
        Node body
    ) {
        if (body is not RigidBody3D rigidBody3D) {
            return; // Only care about rigid bodies
        }

        if (!rigidBody3D.IsInGroup(CuttingObjectsGroupName)) {
            return; // Only care about objects that are in the cutting objects group
        }

        Node3D sceneReplacementNode = _sceneReplacement.Instantiate<Node3D>();
        Node3D ownerAsNode3D = (Node3D)Owner;
        sceneReplacementNode.Position = ownerAsNode3D.Position;
        ownerAsNode3D.GetParentNode3D().AddChild(sceneReplacementNode);
        
        ownerAsNode3D.QueueFree();
    }
}