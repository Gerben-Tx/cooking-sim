using System.Linq;
using Godot;

namespace CookingSim.Features.MergeableObjectFeature;

[Tool]
public partial class MergeableObject : MergeableBase, IFeature {
    public static string FeatureName => "MergeableObjectFeature";
    
    [Export] private PackedScene[] _mergeableObjects = [];

    protected override bool IsMergingAllowed(
        Node3D body
    ) {
        bool baseResult = base.IsMergingAllowed(body);
        if (!baseResult) {
            return false;
        }

        return _mergeableObjects.Any(mergeableObject => body.GetSceneFilePath() == mergeableObject.GetPath());
    }
}