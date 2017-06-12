using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Faction
{
	bool isPlayer;
	int order;



	public Faction(bool _isPlayer, int _order)
	{
		isPlayer = _isPlayer;
		order = _order;
	}
}
