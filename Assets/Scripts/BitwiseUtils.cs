public static class BitwiseUtils {
	public static bool HasCompositeFlag(byte b, byte compositeFlag) {
		return (b & compositeFlag) != 0;
	}
}
