
[System.Flags]
public enum CellFlags : byte {
	None = 0,
	Outline = 0b1,
	Wall = 0b10,
	Hole = 0b100,
	StartPos = 0b1000,
	EndPos = 0b10000
}
