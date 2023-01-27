internal static class VectorExtensions
{

	public static Vector3 RotateAroundPivot( this Vector3 pos, Vector3 pivot, Rotation rot )
	{
		return rot * (pos - pivot) + pivot;
	}

}
