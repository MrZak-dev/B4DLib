using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

namespace B4DLib.Godot
{
    public interface IPoolObject
    {
        void Activate();
        void Deactivate();
    }
    public class ObjectsPool : Node 
    {
        public event Action Updated;
        /// <summary>
        /// Represents the scene that will fill the pool
        /// </summary>
        private readonly PackedScene Scene;
        /// <summary>
        /// represents the active nodes (that are used in the game)
        /// </summary>
        private readonly Queue<Node> Active;
        /// <summary>
        /// represents the inactive nodes
        /// </summary>
        private readonly Queue<Node> Inactive;
        
        
        /// <summary>
        /// Create a new Objects pool give a pool a scene
        /// </summary>
        /// <param name="scene"></param>
        public ObjectsPool(PackedScene scene)
        {
            Scene = scene;
            Active = new Queue<Node>();
            Inactive = new Queue<Node>();
        }

        /// <summary>
        /// Fill The pool with size of scenes nodes
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public async Task Fill(int size)
        {
            await Task.Run(() =>
            {
                for (var i = 0; i < size; i++)
                {
                    var nodeInstance = Scene.Instance();
                    Inactive.Enqueue(nodeInstance);
                }
            });
        }

        /// <summary>
        /// Returns one node removed from Inactive and add to Active
        /// </summary>
        /// <returns></returns>
        public Node Pull()
        {
            var targetNode = Inactive.Dequeue();
            Active.Enqueue(targetNode);
            Updated?.Invoke();
            return targetNode;
        }

        /// <summary>
        /// Push an active node to Inactive nodes pool
        /// </summary>
        public void Push()
        {
            var targetNode = Active.Dequeue();
            Inactive.Enqueue(targetNode);
            Updated?.Invoke();
        }

        /// <summary>
        /// get Active nodes count
        /// </summary>
        /// <returns>Active count</returns>
        public int GetActiveCount()
        {
            return Active.Count;
        }

        /// <summary>
        /// get inactive nodes count 
        /// </summary>
        /// <returns>Inactive count</returns>
        public int GetInactiveCount()
        {
            return Inactive.Count;
        }
    }
}