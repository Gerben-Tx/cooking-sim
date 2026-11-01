using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace CookingSim.Features.PickableObjectFeature;

[Tool]
public partial class PickableObject : Node3D, IFeature {
    public static string FeatureName => "PickableObjectFeature";

    #region Exports and validations

    [Export]
    public RigidBody3D RigidBody {
        get => _rigidBody;
        private set {
            _rigidBody = value;
            UpdateConfigurationWarnings();
        }
    }
    [Export] private float IndicatorHeightOffset { get; set; } = 0.001f;
    [Export] public bool LockAngularAxisWhileHeld { get; private set; } = false;

    public override string[] _GetConfigurationWarnings() {
        List<string> warnings = [];

        if (_rigidBody == null) {
            warnings.Add("Rigid body is null");
        }

        if (_rigidBody != null) {
            if (!_rigidBody.GetCollisionLayerValue(Helpers.PhysicsLayers.GetLayerNumber("pickable_objects"))) {
                warnings.Add("Rigid body is not in pickable_objects collision layer");
            }
            if (!_rigidBody.GetCollisionMaskValue(Helpers.PhysicsLayers.GetLayerNumber("pickable_objects"))) {
                warnings.Add("Rigid body is not in pickable_objects collision mask");
            }
            if (!_rigidBody.GetCollisionMaskValue(Helpers.PhysicsLayers.GetLayerNumber("currently_held_pickable_object"))) {
                warnings.Add("Rigid body is not in currently_held_pickable_object collision layer");
            }
        }

        return warnings.ToArray();   
    }
    #endregion
    
    private RigidBody3D _rigidBody;
    private bool _isHeld;
    private Sprite3D _dropIndicatorSprite;

    public override void _EnterTree() {
        UpdateConfigurationWarnings();
    }
    
    public override void _Ready() {
        _dropIndicatorSprite = GetNode<Sprite3D>("DropIndicatorSprite");
        _dropIndicatorSprite.Visible = false;
    }

    public override void _Process(
        double delta
    ) {
        GlobalPosition = _rigidBody.GlobalPosition;
        
        if (!_isHeld) {
            _dropIndicatorSprite.Visible = false;
            // _rigidBody.LockRotation = false;
            return;
        }

        // _rigidBody.LockRotation = LockAngularAxisWhileHeld;
        
        PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
        Vector3 from = _rigidBody.GlobalPosition;
        Vector3 to = from + Vector3.Down * 10f;
        
        PhysicsRayQueryParameters3D query = new() {
            From = from,
            To = to,
            Exclude = [_rigidBody.GetRid()]
        };
        
        Dictionary result = spaceState.IntersectRay(query);
        if (result.Count > 0) {
            Vector3 hitPoint = (Vector3)result["position"];
            _dropIndicatorSprite.GlobalPosition = hitPoint + Vector3.Up * IndicatorHeightOffset;
            _dropIndicatorSprite.Visible = true;            
        } else {
            _dropIndicatorSprite.Visible = false;
        }
    }
    
    public void Start() {
        _isHeld = true;
    }
    
    public void Stop() {
        _isHeld = false;   
    }
}