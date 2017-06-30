using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
	private static Board _instance;
	public static Board Instance
	{
		get
		{
			if (_instance == null)
			{
				Debug.LogError("Board : Instance is null.");
				return null;
			}
			return _instance;
		}
	}
	public static readonly float BOARD_WIDTH = 0.493f;



	void Awake()
	{
		_instance = this;
	}

	public void Initialize()
	{
		
	}

	public void Tick()
	{
		
	}

	public Vector2 GetGridPosition(int _row, int _column)
	{
		int column = Mathf.Clamp(_column, 1, 9) - 5;
		int row = Mathf.Clamp(_row, 1, 9) - 5;
		float x = BOARD_WIDTH * column;
		float y = BOARD_WIDTH * row;
		return new Vector2(x, y);
	}

	public Vector2 GetNearGridPosition(float _x, float _y)
	{
		float frow = _y / BOARD_WIDTH;
		float fcolumn = _x / BOARD_WIDTH;
		int row = Mathf.Clamp((int)Mathf.Round(frow), -4, 4) + 5;
		int column = Mathf.Clamp((int)Mathf.Round(fcolumn), -4, 4) + 5;
		return GetGridPosition(row, column);
	}

	public Vector2 GetNearGrid(float _x, float _y)
	{
		float frow = _y / BOARD_WIDTH;
		float fcolumn = _x / BOARD_WIDTH;
		int row = Mathf.Clamp((int)Mathf.Round(frow), -4, 4) + 5;
		int column = Mathf.Clamp((int)Mathf.Round(fcolumn), -4, 4) + 5;
		return new Vector2(row, column);
	}
}
