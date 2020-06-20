using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class Save : ISerializable
{
    public Vector3 playerPosition;
    public Vector3 playerAngles;

    public Save(Vector3 playerPosition, Vector3 playerAngles)
    {
        this.playerPosition = playerPosition;
        this.playerAngles = playerAngles;
    }

    public Save(SerializationInfo info, StreamingContext context)
    {
        playerPosition.x = (float)info.GetDouble("playerPositionX");
        playerPosition.y = (float)info.GetDouble("playerPositionY");
        playerPosition.z = (float)info.GetDouble("playerPositionZ");
        playerAngles.x = (float)info.GetDouble("playerAnglesX");
        playerAngles.y = (float)info.GetDouble("playerAnglesY");
        playerAngles.z = (float)info.GetDouble("playerAnglesZ");
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("playerPositionX", playerPosition.x);
        info.AddValue("playerPositionY", playerPosition.y);
        info.AddValue("playerPositionZ", playerPosition.z);
        info.AddValue("playerAnglesX", playerAngles.x);
        info.AddValue("playerAnglesY", playerAngles.y);
        info.AddValue("playerAnglesZ", playerAngles.z);
    }

    public static void SaveGame(Player p, string path)
    {
        // TODO check failures
        Save save = new Save(p.transform.position, p.eulerAngles);

        BinaryFormatter binaryFormatter = new BinaryFormatter();
        FileStream file = File.Create(path);
        binaryFormatter.Serialize(file, save);
        file.Close();

        Debug.Log("Saved into " + path);
    }

    public static bool LoadGame(Player p, string path)
    {
        if(File.Exists(path))
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream file = File.Open(path, FileMode.Open);

            Save save = binaryFormatter.Deserialize(file) as Save;
            file.Close();

            p.eulerAngles = save.playerAngles;
            p.transform.position = save.playerPosition;

            p.chunkManager.Refresh();
            return true;
        }
        return false;
    }
}
