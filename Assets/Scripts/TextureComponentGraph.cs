using System.Collections.Generic;
using UnityEngine;


public enum ObstacleType {
	Walls = 0,
	Hole
}

public struct ObstacleComponent {
	public ObstacleType obstacle;
	public List<Vector2Int> pixels; // TODO: Adjust preallocation
}

/// @brief Separates texture data into individual filled regions that share components
public class TextureComponentGraph {
	private Color32[] m_colors;
	private Stack<Vector2Int> m_stack = new Stack<Vector2Int>(300); // Arbitrary capacity
	private bool[] m_visited;
	private int m_width;
	private int m_height;
	
	static readonly Color32 FLOOR_COLOR = new Color32(237, 28, 36, 255);
	static readonly Color32 WALLS_COLOR = new Color32(255, 242, 0, 255);
	static readonly Color32 HOLE_COLOR = Color.black;

	public TextureComponentGraph(Texture2D texture) {
		this.m_colors = texture.GetPixels32();
		this.m_width = texture.width;
		this.m_height = texture.height;
	}

	private int UnwindIndex(in Vector2Int entry, int width) {
		return entry.y * width + entry.x;
	}
	
	private bool ColorEquals(in Color32 color, in Color32 other) {
		return color.r == other.r && color.g == other.g && color.b == other.b && color.a == other.a;
	}

	private void AddNeighborToStack(Color32 baseColor, Vector2Int neighbor, int width, int height, ref ObstacleComponent component) {
		if (neighbor.x <= 0 || neighbor.x >= width || neighbor.y >= height) {
			return;
		}
		int index = UnwindIndex(neighbor, width);
		// Either we already processed it, or the pixel is not a member of the same region
		if (this.m_visited[index] || !ColorEquals(this.m_colors[index], baseColor)) {
			return;
		}
		this.m_visited[index] = true;
		this.m_stack.Push(neighbor);
		// Add it to the components pixel array
		component.pixels.Add(neighbor);
	}

	private ObstacleComponent DepthFirstSearch(in Color32 baseColor, int width, int height) {
		ObstacleComponent result = new ObstacleComponent();
		ObstacleType obsType = ColorEquals(baseColor, in WALLS_COLOR) ? ObstacleType.Walls : ObstacleType.Hole;
		result.pixels = new List<Vector2Int>(50);
		result.obstacle = obsType;

		while (this.m_stack.Count != 0) {
			Vector2Int entry = this.m_stack.Pop();
			int currentIndex = UnwindIndex(entry, width);
			bool wasVisited = this.m_visited[currentIndex];
			if (wasVisited)
				continue;
			
			ref Color32 currentColor = ref this.m_colors[currentIndex];
			// Visit the neighbors and just add them to our component labeling if they match?
			// Get neighbors to the 4 directions
			
			Vector2Int neighborLeft = new Vector2Int(entry.x - 1, entry.y);
			AddNeighborToStack(baseColor, neighborLeft, width, height, ref result);
			
			Vector2Int neighborRight = new Vector2Int(entry.x + 1, entry.y);
			AddNeighborToStack(baseColor, neighborRight, width, height, ref result);
			
			Vector2Int neighborTop = new Vector2Int(entry.x, entry.y + 1);
			AddNeighborToStack(baseColor, neighborTop, width, height, ref result);
			
			Vector2Int neighborBottom = new Vector2Int(entry.x, entry.y - 1);
			AddNeighborToStack(baseColor, neighborBottom, width, height, ref result);
		}
		return result;
	}

	public List<ObstacleComponent> GroupInRegions() {
		List<ObstacleComponent> result = new List<ObstacleComponent>();
		for (int i = 0; i < this.m_colors.Length; i++) {
			int yPos = i / this.m_height;
			int xPos = i % this.m_width;
			ref Color32 color = ref this.m_colors[i];
			this.m_visited[i] = true;
			this.m_stack.Clear();
			this.m_stack.Push(new Vector2Int(xPos, yPos));
			ObstacleComponent compToAdd = DepthFirstSearch(in color, this.m_width, this.m_height);
			result.Add(compToAdd);
		}
		return result;
	}
	
}


