[System.Serializable]
public struct Faction
{
	/// <summary>
	/// 자신의 캐릭터일 경우 true. 상대일 경우 false.
	/// </summary>
	public bool isPlayer;
	/// <summary>
	/// 캐릭터의 배치 순서.
	/// </summary>
	public int order;



	public Faction(bool _isPlayer, int _order)
	{
		isPlayer = _isPlayer;
		order = _order;
	}
}
