using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GridGenerator : MonoBehaviour {

    public GameObject towerObject;
    public float originX = 0;
    public float originY = -8;
    public float shiftX = 2.572f;
    public float shiftY = 0.752f;
    public GameObject hexObject;
    private GameObject[,] board = new GameObject[GameEngine.BOARD_SIZE_X*2+3, GameEngine.BOARD_SIZE_Y*2+3];

    private GameObject spawnHex(float x, float y)
    {
        return Instantiate(hexObject, new Vector3(x, y, 0), Quaternion.identity) as GameObject;
    }

    private void spawnRow(int rowSize, float rowY, int mapRowY)
    {
        if (rowSize % 2 == 1)
        {
            int lim = Math.Min((rowSize - 1) / 2, GameEngine.BOARD_SIZE_X/2);
            for (int i = -lim; i <= lim; i++)
            {
                int crdX = i * 2 + GameEngine.BOARD_SIZE_X;
                board[crdX, mapRowY] = spawnHex(originX + i * shiftX, rowY);
                board[crdX, mapRowY].GetComponent<Field>().init(crdX, mapRowY);
            }
        }
        else
        {
            int lim = Math.Min(rowSize / 2, GameEngine.BOARD_SIZE_X/2);
            float origin = shiftX / 2;
            for (int i = 0; i < lim; i++)
            {
                int crdX = i * 2 + 1 + GameEngine.BOARD_SIZE_X;
                board[crdX, mapRowY] = spawnHex(origin + originX + i * shiftX, rowY);
                board[crdX, mapRowY].GetComponent<Field>().init(crdX, mapRowY);
                crdX = -i * 2 - 1 + GameEngine.BOARD_SIZE_X;
                board[crdX, mapRowY] = spawnHex(-(origin + i * shiftX) + originX, rowY);
                board[crdX, mapRowY].GetComponent<Field>().init(crdX, mapRowY);
            }
        }
    }

    public GameObject[,] GenerateGrid()
    {

        int rowSize = 1;
        float rowY = originY;
        int rowId = 0;

        for(int i = 0; i < GameEngine.BOARD_SIZE_Y; i++)
        {
            spawnRow(rowSize, rowY, rowId);
            rowSize++;
            rowId++;
            rowY += shiftY;
        }
        for (int i = 0; i <= GameEngine.BOARD_SIZE_Y; i++)
        {
            spawnRow(rowSize, rowY, rowId);
            rowSize--;
            rowId++;
            rowY += shiftY;
        }

        return board;
    }
	
}
