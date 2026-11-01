using System;
using CookingSim.Features.PickableObjectFeature;
using CookingSim.Helpers;
using Godot;

namespace CookingSim.PlayerCharacter;

public partial class PickupScript : Node {
    private const string CurrentlyHeldPickableObjectLayerName = "currently_held_pickable_object";
    private const string PickableObjectsLayerName = "pickable_objects";
    [Export] private Marker3D _pickupAnchorPoint;
    [Export] private Camera3D _camera;
    [Export] private RayCast3D _grabRay;
    [Export] private CollisionObject3D _playerCollider;
    [Export] private float _grabForce = 30f;
    [Export] private float _throwForce = 10f;
    [Export] private float _holdDistance = 2f;
    [Export] private float _rotationSpeed = 0.1f;
    [Export] private float _forwardStep = 0.1f;
    [Export] private float _minHoldDistance = 0f;
    [Export] private float _maxHoldDistance = 1f;

    private PickableObject _grabbedObject;
    private Vector2 _mouseDeltaAccum;
    private bool _isRotating;
    private float _forwardOffset;

    public override void _EnterTree() {
        base._EnterTree();

        // Helper that tries to find the nodes in the scene tree
        _pickupAnchorPoint ??= GetNode<Marker3D>("PickupAnchorPoint");
        _camera ??= GetNode<Camera3D>("Camera");
        _grabRay ??= GetNode<RayCast3D>("GrabRay");
        _playerCollider ??= GetNode<CollisionObject3D>("Player");
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        if (_pickupAnchorPoint == null) {
            throw new NullReferenceException("PickupAnchorPoint is null");
        }

        if (_camera == null) {
            throw new NullReferenceException("Camera is null");
        }

        if (_grabRay == null) {
            throw new NullReferenceException("GrabRay is null");
        }

        if (_playerCollider == null) {
            throw new NullReferenceException("PlayerCollider is null");
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(
        double delta
    ) {
        if (Input.IsActionJustPressed("pickup_object")) {
            if (_grabbedObject == null) {
                TryGrab();
            } else {
                Drop();
            }
        }

        if (Input.IsActionJustPressed("throw_object")) {
            Throw();
        }

        if (_grabbedObject != null) {
            if (Input.IsActionJustPressed("rotate_object")) {
                _isRotating = true;
            } else if (Input.IsActionJustReleased("rotate_object")) {
                _isRotating = false;
            }

            MoveGrabbedObject((float)delta);
        }
    }

    private void Drop() {
        if (_grabbedObject == null) {
            return;
        }

        _grabbedObject.RigidBody.LinearVelocity = Vector3.Zero;
        _grabbedObject.RigidBody.AngularVelocity = Vector3.Zero;

        ResetGrabbedBody();
    }

    private void MoveGrabbedObject(
        float delta
    ) {
        if (_grabbedObject == null) {
            return;
        }

        Vector3 targetPos = _pickupAnchorPoint.GlobalPosition;
        targetPos += _pickupAnchorPoint.GlobalTransform.Basis.Z * (_forwardOffset * -1); // Forward/backward offset from the anchor point
        Vector3 direction = targetPos - _grabbedObject.GlobalPosition;
        Vector3 requiredVelocity = direction / delta;

        // Blend with existing velocity for stability
        _grabbedObject.RigidBody.LinearVelocity = requiredVelocity * _grabForce * delta;
        _grabbedObject.RigidBody.AngularVelocity *= 0.9f; // Damp rotation while held

        // Node3D owner = (Node3D)_grabbedBody.Owner;
        // GD.Print($"Local position: {owner.Position} | {_grabbedBody.Position}");
        // GD.Print($"Global position: {owner.GlobalPosition} | {_grabbedBody.GlobalPosition}");
        // owner.GlobalPosition = _grabbedBody.GlobalPosition;
        
        _grabbedObject.RigidBody.LockRotation = _grabbedObject.LockAngularAxisWhileHeld; // Lock (or whatever the original state was) rotation while held and not rotating
        
        if (!_isRotating || _mouseDeltaAccum == Vector2.Zero) {
            return;
        }

        _grabbedObject.RigidBody.LockRotation = false; // Make sure we can always rotate the object even if it's rotation was locked

        Transform3D tr = _grabbedObject.GlobalTransform;
        float yaw = (_mouseDeltaAccum.X * -1) * _rotationSpeed;
        float pitch = (_mouseDeltaAccum.Y * -1) * _rotationSpeed;

        Vector3 angularVelocity = Vector3.Zero;
        angularVelocity += tr.Basis.Y * yaw;
        angularVelocity += tr.Basis.X * pitch;
        // angularVelocity += tr.Basis.Z * pitch;
        _grabbedObject.RigidBody.AngularVelocity = angularVelocity;
        _mouseDeltaAccum = Vector2.Zero;
    }

    private void Throw() {
        if (_grabbedObject == null) {
            return;
        }

        Vector3 throwDir = _camera.GlobalTransform.Basis.Z * -1;
        _grabbedObject.RigidBody.ApplyCentralImpulse(throwDir * _throwForce); //* _grabbedBody.Mass);

        ResetGrabbedBody();
    }

    private void ResetGrabbedBody() {
        if (_grabbedObject == null) {
            return;
        }
        
        _grabbedObject.Stop();

        _forwardOffset = 0f;
        
        _grabbedObject.RigidBody.SetCollisionLayerValue(PhysicsLayers.GetLayerNumber(CurrentlyHeldPickableObjectLayerName), false);
        _grabbedObject.RigidBody.SetCollisionLayerValue(PhysicsLayers.GetLayerNumber(PickableObjectsLayerName), true);
        _grabbedObject.RigidBody.LockRotation = false;
        _grabbedObject = null;
    }

    private void TryGrab() {
        if (_grabbedObject != null) {
            return;
        }

        _grabRay.ForceRaycastUpdate();
        if (_grabRay.IsColliding()) {
            GodotObject collider = _grabRay.GetCollider();
            if (collider is RigidBody3D rigidBody) {
                PickableObject pickableObject =
                    rigidBody.GetNode<PickableObject>($"Features/{PickableObject.FeatureName}");
                if (pickableObject != null) {
                    _grabbedObject = pickableObject;
                    _grabbedObject.RigidBody.SetCollisionLayerValue(
                        PhysicsLayers.GetLayerNumber(CurrentlyHeldPickableObjectLayerName),
                        true);
                    _grabbedObject.RigidBody.SetCollisionLayerValue(PhysicsLayers.GetLayerNumber(PickableObjectsLayerName), false);

                    pickableObject.Start();
                }
            }
        }
    }

    public override void _Input(
        InputEvent @event
    ) {
        if (_grabbedObject == null) return;

        if (@event.IsAction("forward_move_object")) {
            if (_forwardOffset < _maxHoldDistance) {
                _forwardOffset += _forwardStep;
            }
        } else if (@event.IsAction("backward_move_object")) {
            if (_forwardOffset > _minHoldDistance - _forwardStep) {
                _forwardOffset -= _forwardStep;
            }
        }
        _forwardOffset = Mathf.Clamp(_forwardOffset, _minHoldDistance, _maxHoldDistance);

        if (!_isRotating) return;

        if (@event is InputEventMouseMotion mouseMotion) {
            _mouseDeltaAccum += mouseMotion.Relative;
        }
    }
}