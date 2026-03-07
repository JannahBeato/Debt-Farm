using UnityEngine;

[System.Serializable]
public class TileStateSaveData
{
    public int x;
    public int y;
    public int z;
    public string stateId;

    public TileStateSaveData(Vector3Int pos, string id)
    {
        this.x = pos.x;
        this.y = pos.y;
        this.z = pos.z;
        this.stateId = id;
    }

    public Vector3Int GetPosition()
    {
        return new Vector3Int(x, y, z);
    }
}
