
[System.Flags]
public enum CellFlags : byte {
	None = 0,
	Outline = 0b1,
	Wall = 0b10,
	Hole = 0b100
}
