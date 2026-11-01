using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace CookingSim.Features.MergeableObjectFeature;

[Tool]
public partial class MergeableBase : Node3D {
    [Export] private Area3D[] _mergeAreas;
    [Export] private Marker3D _mergeTarget;

    private readonly HashSet<Node3D> _alreadyMerged = [];


    public override string[] _GetConfigurationWarnings() {
        List<string> warnings = [];

        if (_mergeAreas == null || _mergeAreas.Length == 0) {
            warnings.Add("Missing merge areas");
        }

        if (_mergeAreas?.Length > 1) {
            foreach (Area3D mergeTarget in _mergeAreas) {
                if (!mergeTarget.GetCollisionMaskValue(Helpers.PhysicsLayers.GetLayerNumber("pickable_objects"))) {
                    warnings.Add($"Merge area '{mergeTarget.Name}' is not in pickable_objects collision mask");
                }

                if (mergeTarget.FindChildren("", "CollisionShape3D").Count > 1) {
                    warnings.Add(
                        $"Merge area '{mergeTarget.Name}' has more than one collision shape, which is not supported");
                }
            }
        }

        return warnings.ToArray();
    }

    public override void _EnterTree() {
        UpdateConfigurationWarnings();

        foreach (Area3D mergeArea in _mergeAreas) {
            mergeArea.ChildEnteredTree += MergeAreaOnChildEnteredTree;
            mergeArea.ChildExitingTree += MergeAreaOnChildEnteredTree;
        }
    }

    private void MergeAreaOnChildEnteredTree(
        Node node
    ) {
        if (node is not CollisionShape3D) {
            // Only care about collision shapes in merge areas
            return;
        }

        UpdateConfigurationWarnings();
    }

    public override void _Ready() {
        // Setup event handlers
        foreach (Area3D mergeArea in _mergeAreas) {
            mergeArea.BodyEntered += MergeAreaOnBodyEntered;
        }
    }

    protected virtual bool IsMergingAllowed(
        Node3D body
    ) {
        // GD.Print("Entered merge target " + body.Owner.Name);
        if (_alreadyMerged.Contains(body)) {
            // Don't merge with the same object twice'
            return false;
        }

        bool allAreasOverlap = _mergeAreas.All(mergeArea => mergeArea.OverlapsBody(body));
        if (!allAreasOverlap) {
            // All areas have to overlap in order to merge
            return false;
        }

        return true;
    }

    private void MergeAreaOnBodyEntered(
        Node3D body
    ) {
        if (!IsMergingAllowed(body)) {
            return;
        }

        Merge(body);
    }

    protected virtual void Merge(
        Node3D body
    ) {
        _alreadyMerged.Add(body);
        GD.Print("Merging with " + body.Name);

        RigidBody3D rigidBody3D = (RigidBody3D)body;
        rigidBody3D.Reparent(Owner, true);
        rigidBody3D.Freeze = true;
        rigidBody3D.CollisionLayer = 0;
        rigidBody3D.CollisionMask = 0;
        rigidBody3D.Transform = _mergeTarget.Transform;

        // Move merge areas and target up, so other objects can merge on top
        float yOffset = rigidBody3D.GetNode<MeshInstance3D>("MeshInstance3D").GetAabb().End.Y;
        foreach (Area3D mergeArea in _mergeAreas) {
            CollisionShape3D collisionShape = mergeArea.GetNode<CollisionShape3D>("CollisionShape3D");
            // The shape height will help us to move the merge area on top of the merged object
            // otherwise it would be inside the merged object (the origin is in the center of the shape)
            float collisionShapeHeight = collisionShape.Shape switch {
                BoxShape3D boxShape => boxShape.Size.Y,
                SphereShape3D sphereShape => sphereShape.Radius * 2,
                CapsuleShape3D capsuleShape3D => capsuleShape3D.Height,
                CylinderShape3D cylinderShape3D => cylinderShape3D.Height,
                _ => throw new Exception("Unsupported collision shape")
            };
            Vector3 newMergeAreaPosition = mergeArea.Position;
            newMergeAreaPosition.Y = rigidBody3D.Position.Y + yOffset + collisionShapeHeight;
            mergeArea.Position = newMergeAreaPosition;
        }

        Vector3 newMergeTargetPosition = _mergeTarget.Position;
        newMergeTargetPosition.Y = rigidBody3D.Position.Y + yOffset;
        _mergeTarget.Position = newMergeTargetPosition;
    }
}