using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;

public static class DataManager {

    public static Options savedOptions = new Options();
    /*
    public static SaveData file1 = new SaveData();
    public static SaveData file2 = new SaveData();
    public static SaveData file3 = new SaveData();
    */
    
    // Initialize with however many save files you want
    private static int curFile;
    public static SaveData[] data = new SaveData[5];
    public static SaveData currentData => data[curFile];
    private static string saveFilePath => Application.persistentDataPath + "/file" + curFile + ".duck";

    public static void SelectFile(int currentFile)
    {
        if (currentFile < 0 || currentFile >= data.Length) return;
        curFile = currentFile;
    }

    public static void Save() {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(saveFilePath);
        bf.Serialize(file, data[curFile]);
        file.Close();
    }

    public static void Load() {
        BinaryFormatter bf = new BinaryFormatter();

        // Load the save file if it exists
        if (File.Exists(saveFilePath)) {
            FileStream file = File.Open(saveFilePath, FileMode.Open);
            data[curFile] = (SaveData)bf.Deserialize(file);
            file.Close();
        }
        else { data[curFile] = new SaveData(); }
    }

    public static void WipeSave()
    {
        data[curFile].ClearData();
        Save();
    }

    public static void SaveOptions() {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/options.duck");
        bf.Serialize(file, savedOptions);
        file.Close();
    }

    public static void LoadOptions() {
        if (File.Exists(Application.persistentDataPath + "/options.duck")) {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/options.duck", FileMode.Open);
            savedOptions = (Options)bf.Deserialize(file);
            file.Close();
        }
    }

}