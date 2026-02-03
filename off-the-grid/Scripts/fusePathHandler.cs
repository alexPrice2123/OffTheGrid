using Godot;
using System.Collections.Generic;

public partial class fusePathHandler : Node3D
{
    // Export a packed scene for the mesh you want to place
    [Export]
    public PackedScene PathMeshScene { get; set; }

    // Define the distance between each placed mesh
    [Export]
    public float MeshSpacing { get; set; } = 1.0f;

	private int _count = 0;
	private float _closestDist = 100f;
	private Node3D _closestFuse;

    // Reference to the NavigationServer3D map
    private Rid _navigationMap;
    // List to keep track of spawned meshes for easy cleanup
    private List<Node3D> _spawnedMeshes = new List<Node3D>();

    public override void _Ready()
    {
        // Get the default navigation map RID
        _navigationMap = GetWorld3D().NavigationMap;
    }

	public override void _PhysicsProcess(double delta)
    {
		_count += 1;
        if (_count == 100)
        {
			foreach (Node3D fuse in GetNode<Node3D>("FuseHolder").GetChildren())
			{
				if ((GetNode<Player>("Player").GlobalPosition - fuse.GlobalPosition).Length() < _closestDist)
				{
					_closestDist = (GetNode<Player>("Player").GlobalPosition - fuse.GlobalPosition).Length();
					_closestFuse = fuse;
				}
			}
           	GenerateAndPlaceMeshes(GetNode<Player>("Player").GlobalPosition, _closestFuse.GlobalPosition); 
        }
		if (_count == 150)
        {
            foreach (GpuParticles3D warn in _spawnedMeshes)
            {
                warn.Emitting = false;
            }
			_count = 0;
        }
		if (_count == 200)
        {
            ClearMeshes();
			_count = 0;
        }
    }

    // Call this method to generate the path and place the meshes
    public void GenerateAndPlaceMeshes(Vector3 startPosition, Vector3 targetPosition)
    {
        // 2. Get the path points from the Navigation Server
        Vector3[] pathPoints = NavigationServer3D.MapGetPath(
            _navigationMap,
            startPosition,
            targetPosition,
            true // optimized path for smoother corners
        );

        if (pathPoints.Length == 0)
        {
            GD.Print("Pathfinding failed or path has no points.");
            return;
        }
        // 3. Place meshes along the path
        PlaceMeshesAlongPath(pathPoints);
    }

    private void PlaceMeshesAlongPath(Vector3[] pathPoints)
    {
        float distanceTraveled = 0f;
        
        // Start from the first point (usually the player's position)
        for (int i = 0; i < pathPoints.Length - 1; i++)
        {
            Vector3 currentPos = pathPoints[i];
            Vector3 nextPos = pathPoints[i + 1];
            float segmentLength = currentPos.DistanceTo(nextPos);

            // Travel along the segment
            while (distanceTraveled < segmentLength)
            {
                // Calculate the position along the segment
                Vector3 spawnPosition = currentPos.Lerp(nextPos, distanceTraveled / segmentLength);

                // Instantiate and place the mesh
                Node3D meshInstance = (Node3D)PathMeshScene.Instantiate();
                AddChild(meshInstance); // Add as child of the PathManager or another suitable parent node
				meshInstance.GlobalPosition = spawnPosition;
                _spawnedMeshes.Add(meshInstance);

                // Move to the next spawn point
                distanceTraveled += MeshSpacing;
            }

            // Reset distance traveled for the next segment, accounting for overlap
            distanceTraveled -= segmentLength;
        }
    }

    private void ClearMeshes()
    {
        foreach (var mesh in _spawnedMeshes)
        {
            mesh.QueueFree(); // Safely remove node
        }
        _spawnedMeshes.Clear();
    }
}
