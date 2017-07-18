using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Divide : MonoBehaviour
{
	public int divideNum = 0;
	public List<Vector2> stagePos;
	public List<string> stages;
	public Vector2 camCenter;
	public float camSize;



	public int[] GetStageEnemys(int _num)
	{
		string[] s = stages[_num - 1].Split(',');
		int[] e = new int[5];
		for (int i = 0; i < 5; ++i)
			e[i] = int.Parse(s[i]);

		return e;
	}
}