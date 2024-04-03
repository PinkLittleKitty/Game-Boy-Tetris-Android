using System;
using System.IO;
using System.Text;
using UnityEngine;

public class DataSaver
{
    public static void saveData<T>(T dataToSave, string dataFileName)
    {
        string tempPath = Path.Combine(Application.persistentDataPath, "data");
        tempPath = Path.Combine(tempPath, dataFileName + ".txt");

        //Convertir a Json y después a bytes
        string jsonData = JsonUtility.ToJson(dataToSave, true);
        byte[] jsonByte = Encoding.ASCII.GetBytes(jsonData);

        //Crear el directorio si no existe
        if (!Directory.Exists(Path.GetDirectoryName(tempPath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(tempPath));
        }

        try
        {
            File.WriteAllBytes(tempPath, jsonByte);
            Debug.Log("Se guardaron los datos en: " + tempPath.Replace("/", "\\"));
        }
        catch (Exception e)
        {
            Debug.LogWarning("Fallo al guardar los datos en: " + tempPath.Replace("/", "\\"));
            Debug.LogWarning("Error: " + e.Message);
        }
    }

    //Cargar
    public static T loadData<T>(string dataFileName)
    {
        string tempPath = Path.Combine(Application.persistentDataPath, "data");
        tempPath = Path.Combine(tempPath, dataFileName + ".txt");

        //Salir si el directorio o archivo no existe.
        if (!Directory.Exists(Path.GetDirectoryName(tempPath)))
        {
            Debug.LogWarning("El directorio no existe");
            return default(T);
        }

        if (!File.Exists(tempPath))
        {
            Debug.Log("El archivo no existe");
            return default(T);
        }

        //Cargar el Json
        byte[] jsonByte = null;
        try
        {
            jsonByte = File.ReadAllBytes(tempPath);
            Debug.Log("Se cargó desde: " + tempPath.Replace("/", "\\"));
        }
        catch (Exception e)
        {
            Debug.LogWarning("Fallo al cargar los datos desde: " + tempPath.Replace("/", "\\"));
            Debug.LogWarning("Error: " + e.Message);
        }

        string jsonData = Encoding.ASCII.GetString(jsonByte);

        //Convert a Objeto
        object resultValue = JsonUtility.FromJson<T>(jsonData);
        return (T)Convert.ChangeType(resultValue, typeof(T));
    }

    public static bool deleteData(string dataFileName)
    {
        bool success = false;

        string tempPath = Path.Combine(Application.persistentDataPath, "data");
        tempPath = Path.Combine(tempPath, dataFileName + ".txt");

        //Salir si el directorio o archivo no existe.
        if (!Directory.Exists(Path.GetDirectoryName(tempPath)))
        {
            Debug.LogWarning("El directorio no existe");
            return false;
        }

        if (!File.Exists(tempPath))
        {
            Debug.Log("El archivo no existe");
            return false;
        }

        try
        {
            File.Delete(tempPath);
            Debug.Log("Se eliminaron datos de: " + tempPath.Replace("/", "\\"));
            success = true;
        }
        catch (Exception e)
        {
            Debug.LogWarning("Fallo al eliminar datos de: " + e.Message);
        }

        return success;
    }
}