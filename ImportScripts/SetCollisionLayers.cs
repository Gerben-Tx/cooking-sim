using Godot;

namespace CookingSim.ImportScripts;

[Tool]
public partial class SetCollisionLayers : EditorScenePostImport
{
    public override Node _PostImport(
        Node node
    ) {
        if (node is not Node3D node3D) {
            return node;
        }
        
        Configure(node3D);
        return node3D;
    }

    private static void Configure(
        Node3D node
    ) {
        switch (node) {
            case StaticBody3D staticBody:
                staticBody.SetCollisionMaskValue(Helpers.PhysicsLayers.GetLayerNumber("walls"), true);
                staticBody.SetCollisionMaskValue(Helpers.PhysicsLayers.GetLayerNumber("player"), true);
                staticBody.SetCollisionMaskValue(Helpers.PhysicsLayers.GetLayerNumber("pickable_objects"), true);
                staticBody.SetCollisionMaskValue(Helpers.PhysicsLayers.GetLayerNumber("currently_held_pickable_object"), true);
                break;
            case RigidBody3D rigidBody:
                rigidBody.SetCollisionMaskValue(Helpers.PhysicsLayers.GetLayerNumber("walls"), true);
                rigidBody.SetCollisionMaskValue(Helpers.PhysicsLayers.GetLayerNumber("player"), true);
                rigidBody.SetCollisionMaskValue(Helpers.PhysicsLayers.GetLayerNumber("pickable_objects"), true);
                rigidBody.SetCollisionMaskValue(Helpers.PhysicsLayers.GetLayerNumber("currently_held_pickable_object"), true);
                break;
        }

        foreach (Node child in node.GetChildren()) {
            if (child is not Node3D child3D) {
                continue;
            }
            
            Configure(child3D);
        }
    }
}