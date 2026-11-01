using System;
using CookingSim.Features.CookableObjectFeature;
using Godot;

namespace CookingSim.Cooker;

public partial class Cooker : Node3D
{
	[Export] private Area3D _cookingArea;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (_cookingArea == null) {
			throw new NullReferenceException("CookingArea is null");
		}
		
		_cookingArea.BodyEntered += CookingAreaOnBodyEntered;
		_cookingArea.BodyExited += CookingAreaOnBodyExited;
	}

	public override void _ExitTree()
	{
		_cookingArea.BodyEntered -= CookingAreaOnBodyEntered;
		_cookingArea.BodyExited -= CookingAreaOnBodyExited;
	}
	
	private void CookingAreaOnBodyExited(
		Node3D body
	) {
		Node? cookableObjectNode = body.GetNodeOrNull($"Features/{CookableObject.FeatureName}");
		if (cookableObjectNode != null) {
			CookableObject cookableObject = cookableObjectNode.GetNode<CookableObject>(cookableObjectNode.GetPath());
			cookableObject.StopCooking();
		}
	}

	private void CookingAreaOnBodyEntered(
		Node3D body
	) {
		Node? cookableObjectNode = body.GetNodeOrNull($"Features/{CookableObject.FeatureName}");
		if (cookableObjectNode != null) {
			CookableObject cookableObject = cookableObjectNode.GetNode<CookableObject>(cookableObjectNode.GetPath());
			cookableObject.StartCooking();
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}