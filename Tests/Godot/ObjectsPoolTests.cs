using System.Threading.Tasks;
using B4DLib.Godot;
using Godot;

namespace B4DLib.Tests.Godot
{
    /// <summary>
    /// Test simulator since Testing framework are not integrated with GODOT
    /// </summary>
    public class ObjectsPoolTests : Node
    {
        // public static void RunTests()
        // {
        //     var scene = ResourceLoader.Load<PackedScene>("res://Scenes/World/Coin.tscn");
        //     var objectsPool = new ObjectsPool(scene);
        //     
        //     Task.Run(async () =>
        //     {
        //         await objectsPool.Fill(10);
        //         GD.Print(objectsPool.GetInactiveCount() == 10 ? "Fill Test PASSED" : "Fill Test FAILED");
        //
        //         var node1 = objectsPool.Pull();
        //         var node2 = objectsPool.Pull();
        //         
        //         GD.Print(objectsPool.GetInactiveCount() == 8  && objectsPool.GetActiveCount() == 2 ? "Pull Test PASSED" : "Pull Test FAILED");
        //         
        //         objectsPool.Push();
        //         
        //         GD.Print(objectsPool.GetInactiveCount() == 9  && objectsPool.GetActiveCount() == 1 ? "Push Test PASSED" : "Push Test FAILED");
        //         
        //     });
        // }
        
    }
}