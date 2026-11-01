using System.Collections.Generic;
using System.Linq;
using Godot;

namespace CookingSim.Features.CookableObjectFeature;

public enum CookedState : int {
    Uncooked = 0,
    Cooked = 1,
    Burned = 2
}

[Tool]
public partial class CookableObject : Node, IFeature {
    #region Exports and validations
    [Export]
    private MeshInstance3D Mesh {
        get => _mesh;
        set {
            _mesh = value;
            UpdateConfigurationWarnings();
        }
    }
    [Export]
    private Material CookedMaterial {
        get => _cookedMaterial;
        set {
            _cookedMaterial = value;
            UpdateConfigurationWarnings();
        }
    }
    [Export]
    private Material BurnedMaterial {
        get => _burnedMaterial;
        set {
            _burnedMaterial = value;
            UpdateConfigurationWarnings();
        }
    }
    [Export] private CookedState CookedState { get => _cookedState; set => _cookedState = value; }
    [Export] private float CookTime { get => _cookTime; set => _cookTime = value; }
    [Export] private float BurnTime { get => _burnTime; set => _burnTime = value; }

    public override string[] _GetConfigurationWarnings() {
        List<string> warnings = [];

        if (_mesh == null) {
            warnings.Add("Mesh is null");
        }

        if (_cookedMaterial == null) {
            warnings.Add("Cooked material is null");
        }

        if (_burnedMaterial == null) {
            warnings.Add("Burned material is null");
        }

        return warnings.ToArray();
    }
    #endregion    

    public static string FeatureName => "CookableObjectFeature";

    private MeshInstance3D _mesh;
    private Material _cookedMaterial;
    private Material _burnedMaterial;
    private float _cookProgress = 0f;
    private float _cookTime = 5f; // Total cook time in seconds
    private float _burnTime = 5f; // Total burn time in seconds (after cooking)
    private CookedState _cookedState = CookedState.Uncooked;
    private bool _canCook = false;

    public override void _EnterTree() {
        base._EnterTree();
        
        UpdateConfigurationWarnings();
    }

    public override void _Process(
        double delta
    ) {
        if (!_canCook) {
            return;
        }
        
        switch (_cookedState) {
            case CookedState.Uncooked:
                _cookProgress += (float)delta;
                if (_cookProgress >= _cookTime) _cookedState = CookedState.Cooked;
                break;
            case CookedState.Cooked:
                _cookProgress += (float)delta;
                if (_cookProgress >= _cookTime + _burnTime) _cookedState = CookedState.Burned;
                break;
            case CookedState.Burned:
                StopCooking();
                break;           
        }
        
        UpdateMaterial();
    }

    public void StartCooking() {
        _canCook = true;
        
        GD.Print("Started cooking...");
    }
    
    public void StopCooking() {
        _canCook = false;
        
        GD.Print("Stopped cooking");
    }   
    
    private void UpdateMaterial() {
        switch (_cookedState) {
            case CookedState.Cooked:
                _mesh.SetSurfaceOverrideMaterial(0, _cookedMaterial);
                break;
            case CookedState.Burned:
                _mesh.SetSurfaceOverrideMaterial(0, _burnedMaterial);
                break;
        }
    }
}